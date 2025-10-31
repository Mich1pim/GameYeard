using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private BoxCollider2D colllider;
    [SerializeField] private float moveSpeed;

    private Item item;
    public void Initialize(Item item)
    {
        this.item = item;
        sr.sprite = item.image;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            bool canAdd = InventoryManager.Instance.AddItem(item);
            if (canAdd)
            {
                StartCoroutine(MoveAndCollect(other.transform));
            }
        }
    }
    private IEnumerator MoveAndCollect(Transform target)
    {
        Destroy(colllider);

        while(transform.position != target.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            yield return 0;
        }

        Destroy(gameObject);
    }
}
