using UnityEngine;

public class Inserter : Building
{
    public enum InserterState
    {
        Idle,
        MoveToPick,
        Pick,
        MoveToDrop,
        Drop
    }

    [Header("Inserter Settings")]
    [SerializeField] private float ticksPerMove = 2f; // Number of ticks (0.1s each) to move half circle. 0.2s total? No, 2 ticks = 0.2s.

    [Header("Inserter State")]
    [SerializeField] private InserterState currentState = InserterState.Idle;
    [SerializeField] private ItemStack heldItem;
    
    // Logic variables
    private float stateTimer; // Counts logic ticks
    private Building sourceBuilding;
    private IItemSource sourceInterface;
    private Building destinationBuilding;
    private IItemReceiver destinationInterface;

    // Visual variables
    [Header("Visuals")]
    public Transform arm; // The rotating part
    public Transform handItemSlot; // Where the item is held visually
    public SpriteRenderer handItemRenderer; // The sprite of the held item

    // Interpolation
    private Quaternion startRotation;
    private Quaternion targetRotation;
    private float interpolationT; // 0 to 1

    private void Start()
    {
        // Initial visual state
        UpdateVisuals();
        startRotation = Quaternion.Euler(0, 0, 180); // Start at "Pick" position (back) logic-wise or Idle?
        targetRotation = Quaternion.Euler(0, 0, 180);
    }

    private void Update()
    {
        // Interpolate arm rotation
        // We estimate progress based on Time.deltaTime relative to the Tick rate, 
        // but simple Lerp towards targetRotation is safer and smoother.
        // Rotation speed should be synced with logic speed.
        
        if (arm != null)
        {
             // Smooth move towards target logic rotation
             // Using a high speed for 'catch up' but limited by logic state changes would be ideal.
             // For now, simple Slerp.
             float step = Time.deltaTime * (1.0f / (ticksPerMove * 0.1f)); // normalized speed
             arm.localRotation = Quaternion.RotateTowards(arm.localRotation, targetRotation, 360f * step * Time.deltaTime * 50f); 
             // Actually, RotateTowards is degree step.
             // Let's just use Slerp with a factor.
             arm.localRotation = Quaternion.Slerp(arm.localRotation, targetRotation, Time.deltaTime * 10f);
        }

        // Update item sprite position (it follows the handSlot)
        if (heldItem.item != null && handItemRenderer != null)
        {
            handItemRenderer.sprite = heldItem.item.icon; // Assume ItemData has 'icon'
            handItemRenderer.enabled = true;
        }
        else if (handItemRenderer != null)
        {
            handItemRenderer.enabled = false;
        }
    }

    public override void OnTick()
    {
        // Logic Tick: 0.1s fixed interval
        
        switch (currentState)
        {
            case InserterState.Idle:
                FindSourceAndDestination();
                
                // In Idle, we usually wait at the "Pick" position (Back) or "Drop" (Front).
                // Let's assume Idle means "Ready to Pick" (Back/180)
                SetTargetRotation(180);

                if (sourceInterface != null && destinationInterface != null)
                {
                    // Check if we can pick (simple existence check, real logic might be complex)
                    // We don't have a "Peek" method, so we just blindly transition to MoveToPick
                    // assuming the source MIGHT have something. 
                    // Optimization: Check sourceBuilding data if possible.
                    
                    if (heldItem.item == null)
                    {
                        currentState = InserterState.MoveToPick;
                        stateTimer = 0;
                    }
                    else
                    {
                        // If we somehow have an item, try to drop it
                        currentState = InserterState.MoveToDrop;
                        stateTimer = 0;
                    }
                }
                break;

            case InserterState.MoveToPick:
                SetTargetRotation(180); // Back
                stateTimer++;
                if (stateTimer >= ticksPerMove)
                {
                    currentState = InserterState.Pick;
                    stateTimer = 0;
                }
                break;

            case InserterState.Pick:
                // Try to take item
                if (sourceInterface != null)
                {
                    ItemStack picked = sourceInterface.TakeItem();
                    if (picked.item != null)
                    {
                        heldItem = picked;
                        currentState = InserterState.MoveToDrop;
                        stateTimer = 0;
                    }
                    else
                    {
                        // Failed to pick, go back to Idle (wait)
                        currentState = InserterState.Idle;
                        stateTimer = 0;
                    }
                }
                else
                {
                    currentState = InserterState.Idle;
                }
                break;

            case InserterState.MoveToDrop:
                SetTargetRotation(0); // Front
                stateTimer++;
                if (stateTimer >= ticksPerMove)
                {
                    currentState = InserterState.Drop;
                    stateTimer = 0;
                }
                break;

            case InserterState.Drop:
                // Try to push item
                if (destinationInterface != null && heldItem.item != null)
                {
                    if (destinationInterface.TryReceiveItem(heldItem))
                    {
                        heldItem = default; // Clear item
                        // Success! Now go back to pick
                        currentState = InserterState.MoveToPick; 
                        stateTimer = 0;
                    }
                    else
                    {
                        // Failed to drop (full?), stay in Drop or wait
                        // We stay in Drop state, retrying every tick
                    }
                }
                else
                {
                    // Destination vanished?
                     currentState = InserterState.Idle;
                }
                break;
        }
    }

    private void SetTargetRotation(float zAngle)
    {
        targetRotation = Quaternion.Euler(0, 0, zAngle);
    }

    private void FindSourceAndDestination()
    {
        // Back is opposite to direction. If direction is North (Up), Back is Down.
        Vector2Int backPos = gridPosition + GetVectorForDirection((Direction)(((int)direction + 2) % 4));
        Vector2Int frontPos = gridPosition + GetVectorForDirection(direction);

        sourceBuilding = GridManager.Instance.GetBuildingAt(backPos);
        destinationBuilding = GridManager.Instance.GetBuildingAt(frontPos);

        sourceInterface = sourceBuilding as IItemSource;
        destinationInterface = destinationBuilding as IItemReceiver;
    }
    
    private Vector2Int GetVectorForDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return Vector2Int.up;
            case Direction.East:  return Vector2Int.right;
            case Direction.South: return Vector2Int.down;
            case Direction.West:  return Vector2Int.left;
        }
        return Vector2Int.zero;
    }

    private void UpdateVisuals()
    {
        // Called when loading or state changes significantly
        if (heldItem.item != null && handItemRenderer != null)
        {
            handItemRenderer.sprite = heldItem.item.icon;
        }
    }
    
    public override void GetSaveData(BuildingSaveData data)
    {
        base.GetSaveData(data);
        if (heldItem.item != null)
        {
            data.heldItem = new InventoryItemSaveData { id = heldItem.item.id, count = heldItem.count };
        }
    }

    public override void LoadSaveData(BuildingSaveData data)
    {
        base.LoadSaveData(data);
        if (data.heldItem != null && data.heldItem.id != 0)
        {
            heldItem = new ItemStack 
            { 
                item = SaveManager.Instance.GetItemDataById(data.heldItem.id), 
                count = data.heldItem.count 
            };
            UpdateVisuals();
        }
    }
}
