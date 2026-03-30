using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Material item ", fileName = "Material data - ")]
public class ItemDataSO : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public ItemType itemType;
}
