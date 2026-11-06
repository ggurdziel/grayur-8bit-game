using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Material item ", fileName = "Material data - ")]
public class ItemDataSO : ScriptableObject
{
    [SerializeField, HideInInspector] private string _saveID;
    public string saveID => _saveID;

    [Header("NPC details")]
    public int minStackSizeToGive = 1;
    public int maxStackSizeToGive = 1;

    public string itemName;
    public Sprite itemIcon;
    public ItemType itemType;

    public GameObject worldPrefab;

#if UNITY_EDITOR
    private void OnValidate()
    {
        string id = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(this));
        if (!string.IsNullOrEmpty(id) && _saveID != id)
        {
            _saveID = id;
            EditorUtility.SetDirty(this);
        }
    }
#endif
}