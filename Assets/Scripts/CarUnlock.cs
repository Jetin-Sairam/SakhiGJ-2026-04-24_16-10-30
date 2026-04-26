using UnityEngine;

public class CarUnlock : MonoBehaviour
{
    public GameObject Car;
    void Update()
    {
        if (InventoryManager.Instance.GetItems().Contains("Safe Key"))
        {
            Car.GetComponent<Collider2D>().enabled = true;
            return;
        }
    }
}
