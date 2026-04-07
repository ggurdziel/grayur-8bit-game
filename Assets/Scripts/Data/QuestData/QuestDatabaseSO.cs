using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Quest Data/Quest Database", fileName = "QUEST DATABASE")]
public class QuestDatabaseSO : ScriptableObject
{
    public QuestDataSO[] allQuests;
}