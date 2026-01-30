using System.Collections.Generic;
using UnityEngine;

public class TickManager : MonoBehaviour
{
    public static TickManager Instance { get; private set; }

    [Header("Tick Settings")]
    [SerializeField] private float tickInterval = 0.1f; // 100ms tick rate

    private float timer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= tickInterval)
        {
            timer -= tickInterval;
            Tick();
        }
    }

    private void Tick()
    {
        // Get all active buildings from the GridManager and tick them.
        // Create a copy of the list to avoid collection modification errors if a building is removed during OnTick.
        List<Building> buildings = new List<Building>(GridManager.Instance.GetAllActiveBuildings());
        foreach (Building building in buildings)
        {
            if (building != null) // Check if null in case it was destroyed in the same frame
            {
                building.OnTick();
            }
        }
    }
}
