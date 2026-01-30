using UnityEngine;

public abstract class Building : MonoBehaviour
{
    [Header("Base Building Info")]
    public Vector2Int gridPosition;
    public Vector2Int size = Vector2Int.one;
    public Direction direction = Direction.North;

    /// <summary>
    /// Called by the TickManager for game logic updates.
    /// </summary>
    public virtual void OnTick()
    {
        // Base implementation does nothing.
    }

    /// <summary>
    /// Rotates the building 90 degrees clockwise.
    /// </summary>
    public void Rotate()
    {
        direction = (Direction)(((int)direction + 1) % 4);
        transform.rotation = Quaternion.Euler(0, 0, -90 * (int)direction);
    }

    /// <summary>
    /// Sets the initial position and state of the building.
    /// </summary>
    public virtual void Place(Vector2Int pos)
    {
        gridPosition = pos;
        transform.position = new Vector3(pos.x, pos.y, 0);
    }

    /// <summary>
    /// Fills a BuildingSaveData object with the building's current state.
    /// </summary>
    public virtual void GetSaveData(BuildingSaveData data)
    {
        data.type = GetType().Name;
        data.x = gridPosition.x;
        data.y = gridPosition.y;
        data.dir = (int)direction;
    }

    /// <summary>
    /// Restores the building's state from a BuildingSaveData object.
    /// </summary>
    public virtual void LoadSaveData(BuildingSaveData data)
    {
        direction = (Direction)data.dir;
        transform.rotation = Quaternion.Euler(0, 0, -90 * (int)direction);
    }

    public virtual bool AcceptItem(ItemData item)
    {
        return false;
    }
}
