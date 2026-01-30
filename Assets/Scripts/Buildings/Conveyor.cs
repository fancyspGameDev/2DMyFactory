using UnityEngine;

public class ConveyorBelt : Building
{
    public ItemData currentItem;
    public GameObject itemVisual;

    //  (̵)  
    private bool movedThisTick = false;

    //  [߰] Ƽ ⺻ Update Լ (  ȣ)
    // (OnTick)  ǵ , ȭ ׻ ֽ  ¸  ׸ϴ.
    private void Update()
    {
        UpdateVisuals();
    }

    public override void OnTick()
    {
        // [] UpdateVisuals(); 
        // ⼭ ׸ ׸  Ʈ  Ʈ ÿ ׷ Ÿ̹  ϴ.

        // 1.     
        if (currentItem == null) return;

        // 2. ̹ Ͽ   ̶  ( )
        if (movedThisTick)
        {
            movedThisTick = false; //  Ϻʹ ̵ ϵ 
            return;
        }

        // 3.  ġ 
        Vector2Int targetPos = gridPosition + direction.ToVector();

        // 4. GridManager   ŸϷ   õ
        if (GridManager.Instance.TryMoveItem(currentItem, targetPos))
        {
            currentItem = null; //    κ丮 
        }
    }

    // ܺο    ȣ
    public override bool AcceptItem(ItemData item)
    {
        if (currentItem != null) return false; // ̹   

        currentItem = item;

        //   ǥ (̹  ̵ )
        movedThisTick = true;

        // [] UpdateVisuals();
        // ȭ  Update() ˾Ƽ óմϴ.

        return true;
    }

    private void UpdateVisuals()
    {
        if (itemVisual == null) return;

        //   Ͱ  Ѱ,  ϴ.
        if (currentItem != null)
        {
            itemVisual.SetActive(true);

            //  ̹ ü
            if (currentItem.icon != null)
            {
                SpriteRenderer sr = itemVisual.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = currentItem.icon;
            }
        }
        else
        {
            itemVisual.SetActive(false);
        }
    }
}
