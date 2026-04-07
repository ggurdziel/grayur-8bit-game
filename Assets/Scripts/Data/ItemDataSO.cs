using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Material item ", fileName = "Material data - ")]
public class ItemDataSO : ScriptableObject
{
    [Header("NPC details")]
    public int minStackSizeToGive = 1;
    public int maxStackSizeToGive = 1;

    public string itemName;
    public Sprite itemIcon;
    public ItemType itemType;

    public GameObject worldPrefab;

    

}
