using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Controla todas as telas fora do duelo: título, mapa, história, resultado, instruções e créditos.
public class CampaignFlow : MonoBehaviour
{
    private const string ProgressKey = "duelo_progresso";

    private const string InstructionsText =
        "<b>Objetivo:</b> reduzir a Essência do rival a zero. Você começa com 20 de Essência e 4 cartas.\n\n" +
        "<b>Éter:</b> sobe 1 por turno (até 6) e serve para jogar cartas e fazer Ascensões.\n\n" +
        "<b>Seu turno:</b> clique em uma carta da mão para jogá-la (moldura verde = dá para jogar). " +
        "Seres ocupam uma das 3 zonas. Ritos têm efeito imediato. Vigílias ficam viradas para baixo (máximo 2) e disparam sozinhas.\n\n" +
        "<b>Atacar:</b> clique em um Ser seu (amarelo) e depois em um alvo com moldura vermelha. " +
        "Se o rival não tiver Seres, clique na barra dele para atacar a Essência. Seres com Provocar devem ser atacados primeiro.\n\n" +
        "<b>Ascensão (SHIFT):</b> selecione um Ser com Ascensão e use o botão Ascender para transformá-lo na forma superior, pagando Éter.\n\n" +
        "Passe o mouse sobre uma carta para ver os detalhes. Clique em Passar turno quando terminar.";

    private const string CreditsText =
        "<b>Duelo da Mitologia Grega</b>\nFeito para o GitHub Game Off 2026.\n\n" +
        "Design e programação: Rodrigo Tecco\n" +
        "Assistência de código e design: Claude, IA da Anthropic\n" +
        "Engine: Unity 6.2\n\n" +
        "Arte e áudio: a definir (serão listados aqui, com a licença de cada um).\n\n" +
        "Jogo original, inspirado em mitos gregos de domínio público e sem relação com outros jogos de cartas.";

    // Descreve um botão do menu.
    private class MenuButton
    {
        public string Label;
        public UnityAction OnClick;
        public bool Interactable;

        public MenuButton(string label, UnityAction onClick, bool interactable = true)
        {
            Label = label;
            OnClick = onClick;
            Interactable = interactable;
        }
    }

    [SerializeField] private CampaignData campaign;
    [SerializeField] private DuelUI duelUI;
    [SerializeField] private Font uiFont; // opcional

    private GameObject menuRoot;
    private Text titleText;
    private Text bodyText;
    private RectTransform buttonArea;
    private readonly List<GameObject> spawnedButtons = new List<GameObject>();

    // Quantos capítulos o jogador já concluiu (salvo no navegador/computador).
    private int Progress
    {
        get { return PlayerPrefs.GetInt(ProgressKey, 0); }
        set
        {
            PlayerPrefs.SetInt(ProgressKey, value);
            PlayerPrefs.Save();
        }
    }

    private void Start()
    {
        if (campaign == null || duelUI == null || campaign.PlayerStartDeck == null || campaign.ChapterCount == 0)
        {
            Debug.LogWarning("Preencha 'Campaign' e 'Duel UI' no CampaignFlow. A campanha precisa de um deck inicial e de capítulos.");
            return;
        }

        if (uiFont != null)
        {
            UiBuilder.SetFont(uiFont);
        }

        BuildMenu();
        ShowTitle();
    }

    // ---------- Montagem do menu ----------

    private void BuildMenu()
    {
        menuRoot = new GameObject("MenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        menuRoot.transform.SetParent(transform, false);

        Canvas canvas = menuRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = menuRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        Transform root = menuRoot.transform;

        RectTransform background = UiBuilder.CreatePanel("Fundo", root, new Color(0.07f, 0.06f, 0.11f));
        UiBuilder.SetAnchors(background, 0f, 0f, 1f, 1f, 0f);
        background.GetComponent<Image>().raycastTarget = false;

        titleText = UiBuilder.CreateText("Titulo", root, "", 64, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.4f));
        UiBuilder.SetAnchors(titleText.rectTransform, 0.05f, 0.82f, 0.95f, 0.96f, 0f);
        UiBuilder.EnableBestFit(titleText, 28, 64);

        RectTransform bodyPanel = UiBuilder.CreatePanel("CorpoPainel", root, new Color(0.13f, 0.12f, 0.2f, 0.95f));
        UiBuilder.SetAnchors(bodyPanel, 0.12f, 0.42f, 0.88f, 0.80f, 0f);
        bodyPanel.GetComponent<Image>().raycastTarget = false;

        bodyText = UiBuilder.CreateText("Corpo", bodyPanel, "", 28, TextAnchor.UpperLeft, Color.white);
        UiBuilder.SetAnchors(bodyText.rectTransform, 0f, 0f, 1f, 1f, 18f);
        UiBuilder.EnableBestFit(bodyText, 14, 28);

        buttonArea = UiBuilder.CreatePanel("AreaBotoes", root, new Color(0f, 0f, 0f, 0f));
        UiBuilder.SetAnchors(buttonArea, 0.28f, 0.03f, 0.72f, 0.40f, 0f);
        buttonArea.GetComponent<Image>().raycastTarget = false;
    }

    // Mostra uma tela: título, texto e uma lista de botões empilhados.
    private void ShowScreen(string title, string body, params MenuButton[] buttons)
    {
        titleText.text = title;
        bodyText.text = body;

        foreach (GameObject old in spawnedButtons)
        {
            old.SetActive(false);
            Destroy(old);
        }
        spawnedButtons.Clear();

        int count = buttons.Length;
        for (int i = 0; i < count; i++)
        {
            MenuButton spec = buttons[i];

            Button button = UiBuilder.CreateButton("Botao_" + i, buttonArea, spec.Label, 26, new Color(0.2f, 0.38f, 0.55f), spec.OnClick);
            UiBuilder.SetAnchors(button.GetComponent<RectTransform>(), 0f, (float)(count - 1 - i) / count, 1f, (float)(count - i) / count, 4f);
            button.interactable = spec.Interactable;

            spawnedButtons.Add(button.gameObject);
        }
    }

    // ---------- Telas ----------

    private void ShowTitle()
    {
        string playLabel = Progress > 0 ? "Continuar campanha" : "Jogar";

        ShowScreen(campaign.CampaignTitle, campaign.IntroText,
            new MenuButton(playLabel, ShowMap),
            new MenuButton("Como jogar", ShowInstructions),
            new MenuButton("Créditos", ShowCredits),
            new MenuButton("Recomeçar campanha", ShowResetConfirm));
    }

    private void ShowInstructions()
    {
        ShowScreen("Como jogar", InstructionsText, new MenuButton("Voltar", ShowTitle));
    }

    private void ShowCredits()
    {
        ShowScreen("Créditos", CreditsText, new MenuButton("Voltar", ShowTitle));
    }

    private void ShowResetConfirm()
    {
        ShowScreen("Recomeçar campanha?", "Todo o seu progresso e as cartas ganhas como recompensa serão apagados.",
            new MenuButton("Sim, recomeçar", ResetProgress),
            new MenuButton("Não, voltar", ShowTitle));
    }

    private void ResetProgress()
    {
        Progress = 0;
        ShowTitle();
    }

    private void ShowMap()
    {
        int done = Progress;
        List<MenuButton> buttons = new List<MenuButton>();

        for (int i = 0; i < campaign.ChapterCount; i++)
        {
            ChapterData chapter = campaign.GetChapter(i);
            if (chapter == null)
            {
                continue;
            }

            bool completed = i < done;
            bool unlocked = i <= done;
            int index = i;

            string label = (i + 1) + ". " + chapter.ChapterTitle;
            if (completed)
            {
                label += "  (concluído)";
            }
            else if (!unlocked)
            {
                label += "  (bloqueado)";
            }

            buttons.Add(new MenuButton(label, () => ShowChapterIntro(index), unlocked));
        }

        buttons.Add(new MenuButton("Voltar", ShowTitle));

        ShowScreen("Mapa da campanha",
            "Do Caos primordial à morte de Héracles: cada duelo é uma mudança no Fio do Destino.\n\nCapítulos concluídos: " + done + " de " + campaign.ChapterCount + ".",
            buttons.ToArray());
    }

    private void ShowChapterIntro(int index)
    {
        ChapterData chapter = campaign.GetChapter(index);
        if (chapter == null)
        {
            ShowMap();
            return;
        }

        string body = chapter.StoryText
            + "\n\n<b>Rival:</b> " + chapter.OpponentName
            + "\n<b>Regra especial:</b> " + chapter.RuleText;

        if (chapter.RewardCard != null)
        {
            body += "\n<b>Recompensa:</b> " + chapter.RewardCard.ShownName;
        }

        ShowScreen("Capítulo " + (index + 1) + ": " + chapter.ChapterTitle, body,
            new MenuButton("Duelar", () => StartChapter(index)),
            new MenuButton("Voltar ao mapa", ShowMap));
    }

    // ---------- Duelo ----------

    private void StartChapter(int index)
    {
        ChapterData chapter = campaign.GetChapter(index);
        if (chapter == null || chapter.OpponentDeck == null)
        {
            Debug.LogWarning("O capítulo " + (index + 1) + " está sem deck do rival.");
            return;
        }

        // Deck do jogador = deck inicial + cartas ganhas nos capítulos já concluídos.
        List<CardData> playerCards = new List<CardData>(campaign.PlayerStartDeck.Cards);
        for (int i = 0; i < Progress && i < campaign.ChapterCount; i++)
        {
            ChapterData done = campaign.GetChapter(i);
            if (done != null && done.RewardCard != null)
            {
                playerCards.Add(done.RewardCard);
            }
        }

        DeckData playerDeck = DeckData.CreateRuntime("Deck do Oráculo", playerCards);

        menuRoot.SetActive(false);

        duelUI.BeginDuel(playerDeck, chapter.OpponentDeck, "Oráculo", chapter.OpponentName,
            duel => ApplyRule(duel, chapter),
            won => OnDuelFinished(index, won));
    }

    // Aplica a regra especial do capítulo, antes de o duelo começar.
    private void ApplyRule(DuelGame duel, ChapterData chapter)
    {
        duel.AiProfile = chapter.AiProfile;
        duel.AiDifficulty = chapter.AiDifficulty;

        switch (chapter.Rule)
        {
            case ChapterRule.RivalComEterExtra:
                duel.Players[1].MaxEther = 1;
                break;

            case ChapterRule.CartaExtraParaOsDois:
                duel.DrawCards(duel.Players[0], 1);
                duel.DrawCards(duel.Players[1], 1);
                break;

            case ChapterRule.EssenciaReduzidaParaOsDois:
                duel.Players[0].Essence = 15;
                duel.Players[1].Essence = 15;
                break;

            case ChapterRule.DesafioDeHeracles:
                duel.Players[1].MaxEther = 2;
                duel.DrawCards(duel.Players[0], 1);
                break;
        }

        if (chapter.Rule != ChapterRule.Nenhuma)
        {
            duel.Log("Regra do capítulo: " + chapter.RuleText);
        }
    }

    private void OnDuelFinished(int index, bool won)
    {
        menuRoot.SetActive(true);
        ChapterData chapter = campaign.GetChapter(index);

        if (!won)
        {
            ShowScreen("Derrota",
                "O Fio do Destino resistiu desta vez. Ajuste sua estratégia e tente de novo.",
                new MenuButton("Tentar de novo", () => StartChapter(index)),
                new MenuButton("Voltar ao mapa", ShowMap));
            return;
        }

        bool firstTime = index >= Progress;
        if (firstTime)
        {
            Progress = index + 1;
        }

        string body = chapter.VictoryText;
        if (chapter.RewardCard != null)
        {
            body += firstTime
                ? "\n\n<b>Recompensa:</b> " + chapter.RewardCard.ShownName + " foi adicionada ao seu baralho."
                : "\n\n(" + chapter.RewardCard.ShownName + " já estava no seu baralho.)";
        }

        bool isLast = index >= campaign.ChapterCount - 1;
        if (isLast)
        {
            ShowScreen("Epílogo", chapter.VictoryText + "\n\n" + campaign.EndingText,
                new MenuButton("Créditos", ShowCredits),
                new MenuButton("Voltar ao mapa", ShowMap));
            return;
        }

        ShowScreen("Vitória!", body,
            new MenuButton("Próximo capítulo", () => ShowChapterIntro(index + 1)),
            new MenuButton("Voltar ao mapa", ShowMap));
    }
}