using UnityEngine;

// Regras especiais que um capítulo pode aplicar ao duelo.
public enum ChapterRule
{
    Nenhuma,
    RivalComEterExtra,
    CartaExtraParaOsDois,
    EssenciaReduzidaParaOsDois,
    DesafioDeHeracles
}

// Um capítulo da campanha.
[CreateAssetMenu(fileName = "Chapter_New", menuName = "Duelo/Capitulo", order = 2)]
public class ChapterData : ScriptableObject
{
    [SerializeField] private string chapterTitle;
    [SerializeField, TextArea(3, 8)] private string storyText;
    [SerializeField, TextArea(2, 6)] private string victoryText;

    [Header("Rival")]
    [SerializeField] private string opponentName;
    [SerializeField] private DeckData opponentDeck;

    [Header("Inteligência do rival")]
    [SerializeField] private AIProfile aiProfile = AIProfile.Equilibrado;
    [SerializeField] private AIDifficulty aiDifficulty = AIDifficulty.Normal;

    [Header("Regra especial e recompensa")]
    [SerializeField] private ChapterRule rule = ChapterRule.Nenhuma;
    [SerializeField, TextArea(1, 3)] private string ruleText;
    [SerializeField] private CardData rewardCard;

    public string ChapterTitle => chapterTitle;
    public string StoryText => storyText;
    public string VictoryText => victoryText;
    public string OpponentName => opponentName;
    public DeckData OpponentDeck => opponentDeck;
    public AIProfile AiProfile => aiProfile;
    public AIDifficulty AiDifficulty => aiDifficulty;
    public ChapterRule Rule => rule;
    public string RuleText => ruleText;
    public CardData RewardCard => rewardCard;
}