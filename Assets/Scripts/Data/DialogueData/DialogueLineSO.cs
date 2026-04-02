using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Dialogue Data/New Line Data", fileName = "Line - ")]

public class DialogueLineSO : ScriptableObject
{
    [Header("Dialogue info")]
    public string dialogueGroupName;
    public DialogueSpeakerSO speaker;
        
    [Header("Text options")]
    [TextArea] public string[] textLine;

    [Header("Answer setup")]
    public bool playCanAnswer; // should be true, if player can make a choice
    public DialogueLineSO[] answerLine; 

    public string GetRandomLine()
    {
        return textLine[Random.Range(0, textLine.Length)];
    }
}
