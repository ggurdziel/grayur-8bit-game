using System;

[Serializable]
public class Quest
{
    public QuestDataSO questData;
    public bool isStarted;
    public bool isCompleted;
    public int currentAmount;

    public Quest(QuestDataSO data)
    {
        questData = data;
        isStarted = true;
        isCompleted = false;
        currentAmount = 0;
    }
}