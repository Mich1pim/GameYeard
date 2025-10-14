using UnityEngine;
using UnityEngine.Tilemaps;

public class RoofHouse : MonoBehaviour
{
    public TilemapRenderer roof;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            roof.enabled = false;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            roof.enabled = true;
    }
}
