using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Material item ", fileName = "Material data - ")]
public class ItemDataSO : ScriptableObject
{
    public string saveID { get; private set; }

    [Header("NPC details")]
    public int minStackSizeToGive = 1;
    public int maxStackSizeToGive = 1;

    public string itemName;
    public Sprite itemIcon;
    public ItemType itemType;

    public GameObject worldPrefab;

    private void OnValidate()
    {
#if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(this);
        saveID = AssetDatabase.AssetPathToGUID(path);
#endif
    }

}
