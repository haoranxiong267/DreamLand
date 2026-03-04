// Teleporter.cs
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public int targetMapIndex = 1;  // Target map index (set in Inspector)

    void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered teleporter, switching to map: " + targetMapIndex);

            // Find the MapSwitcher component
            MapSwitcher mapSwitcher = FindObjectOfType<MapSwitcher>();

            if (mapSwitcher != null)
            {
                // Switch to the target map
                mapSwitcher.LoadMap(targetMapIndex);
            }
            else
            {
                Debug.LogError("MapSwitcher component not found!");
            }
        }
    }
}