using System.Collections.Generic;
using UnityEngine;

// A campanha inteira: textos, deck inicial do jogador e a lista de capítulos.
[CreateAssetMenu(fileName = "Campaign_Main", menuName = "Duelo/Campanha", order = 3)]
public class CampaignData : ScriptableObject
{
    [SerializeField] private string campaignTitle = "Duelo da Mitologia Grega";
    [SerializeField, TextArea(2, 6)] private string introText;
    [SerializeField, TextArea(3, 8)] private string endingText;
    [SerializeField] private DeckData playerStartDeck;
    [SerializeField] private List<ChapterData> chapters = new List<ChapterData>();

    public string CampaignTitle => campaignTitle;
    public string IntroText => introText;
    public string EndingText => endingText;
    public DeckData PlayerStartDeck => playerStartDeck;
    public int ChapterCount => chapters.Count;

    public ChapterData GetChapter(int index)
    {
        if (index < 0 || index >= chapters.Count)
        {
            return null;
        }

        return chapters[index];
    }
}