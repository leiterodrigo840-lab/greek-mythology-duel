using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// A tela do duelo: você (jogador 1) contra a IA (jogador 2).
// É chamada pela campanha: BeginDuel(...).
public class DuelUI : MonoBehaviour
{
    [SerializeField] private Font uiFont; // opcional

    private static readonly Color NoFrame = new Color(0f, 0f, 0f, 0f);
    private static readonly Color GreenFrame = new Color(0.3f, 0.9f, 0.3f);
    private static readonly Color YellowFrame = new Color(1f, 0.9f, 0.2f);
    private static readonly Color RedFrame = new Color(1f, 0.25f, 0.25f);

    private DuelGame game;
    private int selectedSlot = -1;
    private bool busy;
    private bool built;

    // Dados do duelo atual (vêm da campanha).
    private DeckData currentPlayerDeck;
    private DeckData currentOpponentDeck;
    private string currentPlayerName;
    private string currentOpponentName;
    private System.Action<DuelGame> configureGame;
    private System.Action<bool> onFinished;

    private readonly CardView[] handViews = new CardView[DuelPlayer.MaxHandSize];
    private readonly CardView[] playerFieldViews = new CardView[DuelPlayer.FieldSlots];
    private readonly CardView[] opponentFieldViews = new CardView[DuelPlayer.FieldSlots];
    private readonly CardView[] playerVigilViews = new CardView[DuelPlayer.MaxVigils];
    private readonly CardView[] opponentVigilViews = new CardView[DuelPlayer.MaxVigils];

    private GameObject canvasObject;
    private Text opponentInfoText;
    private Text playerInfoText;
    private Text logText;
    private Text detailText;
    private Text statusText;
    private Text resultText;
    private Text guardLabel;
    private Image opponentInfoFrame;
    private Button endTurnButton;
    private Button ascendButton;
    private Button guardButton;
    private GameObject resultPanel;

    // ---------- Chamado pela campanha ----------

    public void BeginDuel(DeckData playerDeck, DeckData opponentDeck, string playerName, string opponentName,
        System.Action<DuelGame> configure, System.Action<bool> finished)
    {
        EnsureBuilt();

        currentPlayerDeck = playerDeck;
        currentOpponentDeck = opponentDeck;
        currentPlayerName = playerName;
        currentOpponentName = opponentName;
        configureGame = configure;
        onFinished = finished;

        canvasObject.SetActive(true);
        StartNewDuel();
    }

    public void Hide()
    {
        StopAllCoroutines();
        busy = false;

        if (canvasObject != null)
        {
            canvasObject.SetActive(false);
        }
    }

    private void EnsureBuilt()
    {
        if (built)
        {
            return;
        }

        if (uiFont != null)
        {
            UiBuilder.SetFont(uiFont);
        }

        if (EventSystem.current == null)
        {
            Debug.LogWarning("Falta um EventSystem na cena: GameObject > UI > Event System.");
        }

        BuildLayout();
        built = true;
    }

    // ---------- Montagem da tela ----------

    private void BuildLayout()
    {
        canvasObject = new GameObject("DuelCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        Transform root = canvasObject.transform;

        RectTransform background = UiBuilder.CreatePanel("Fundo", root, new Color(0.08f, 0.07f, 0.12f));
        UiBuilder.SetAnchors(background, 0f, 0f, 1f, 1f, 0f);
        background.GetComponent<Image>().raycastTarget = false;

        // Coluna da esquerda: detalhes da carta e registro.
        detailText = BuildTextPanel(root, "Detalhes", 0.01f, 0.52f, 0.20f, 0.98f, 18, TextAnchor.UpperLeft);
        logText = BuildTextPanel(root, "Registro", 0.01f, 0.02f, 0.20f, 0.50f, 15, TextAnchor.LowerLeft);

        // Barra do oponente (clicável para atacar a Essência dele).
        RectTransform opponentFrameRect = UiBuilder.CreatePanel("OponenteMoldura", root, NoFrame);
        UiBuilder.SetAnchors(opponentFrameRect, 0.21f, 0.91f, 0.79f, 0.99f, 0f);
        opponentInfoFrame = opponentFrameRect.GetComponent<Image>();
        opponentInfoFrame.raycastTarget = false;

        Button opponentButton = UiBuilder.CreateButton("OponenteInfo", opponentFrameRect, "", 22, new Color(0.25f, 0.12f, 0.15f), OnOpponentInfoClicked);
        UiBuilder.SetAnchors(opponentButton.GetComponent<RectTransform>(), 0f, 0f, 1f, 1f, 4f);
        opponentInfoText = opponentButton.GetComponentInChildren<Text>();

        // Zonas do centro.
        BuildRow(opponentVigilViews, SlotKind.OpponentVigil, root, 0.21f, 0.64f, 0.29f, 0.90f, true);
        BuildRow(opponentFieldViews, SlotKind.OpponentField, root, 0.30f, 0.64f, 0.70f, 0.90f, false);
        BuildRow(playerFieldViews, SlotKind.PlayerField, root, 0.30f, 0.34f, 0.70f, 0.60f, false);
        BuildRow(playerVigilViews, SlotKind.PlayerVigil, root, 0.21f, 0.34f, 0.29f, 0.60f, true);

        playerInfoText = BuildTextPanel(root, "JogadorInfo", 0.21f, 0.26f, 0.79f, 0.33f, 22, TextAnchor.MiddleCenter);

        BuildRow(handViews, SlotKind.Hand, root, 0.21f, 0.01f, 0.79f, 0.25f, false);

        // Coluna da direita: título, status e botões.
        Text title = UiBuilder.CreateText("Titulo", root, "Duelo da Mitologia Grega", 26, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.4f));
        UiBuilder.SetAnchors(title.rectTransform, 0.81f, 0.90f, 0.99f, 0.99f, 0f);

        statusText = BuildTextPanel(root, "Status", 0.81f, 0.58f, 0.99f, 0.88f, 18, TextAnchor.UpperLeft);

        ascendButton = UiBuilder.CreateButton("BotaoAscender", root, "Ascender", 28, new Color(0.45f, 0.25f, 0.65f), OnAscendClicked);
        UiBuilder.SetAnchors(ascendButton.GetComponent<RectTransform>(), 0.81f, 0.46f, 0.99f, 0.55f, 0f);

        guardButton = UiBuilder.CreateButton("BotaoGuarda", root, "Postura de Guarda", 22, new Color(0.35f, 0.35f, 0.5f), OnGuardClicked);
        UiBuilder.SetAnchors(guardButton.GetComponent<RectTransform>(), 0.81f, 0.34f, 0.99f, 0.43f, 0f);
        guardLabel = guardButton.GetComponentInChildren<Text>();

        endTurnButton = UiBuilder.CreateButton("BotaoPassarTurno", root, "Passar turno", 28, new Color(0.2f, 0.4f, 0.55f), OnEndTurnClicked);
        UiBuilder.SetAnchors(endTurnButton.GetComponent<RectTransform>(), 0.81f, 0.22f, 0.99f, 0.31f, 0f);

        // Tela de resultado (por cima de tudo).
        RectTransform resultRect = UiBuilder.CreatePanel("Resultado", root, new Color(0f, 0f, 0f, 0.82f));
        UiBuilder.SetAnchors(resultRect, 0f, 0f, 1f, 1f, 0f);
        resultPanel = resultRect.gameObject;

        resultText = UiBuilder.CreateText("ResultadoTexto", resultRect, "", 90, TextAnchor.MiddleCenter, Color.white);
        UiBuilder.SetAnchors(resultText.rectTransform, 0.1f, 0.5f, 0.9f, 0.8f, 0f);

        Button continueButton = UiBuilder.CreateButton("Continuar", resultRect, "Continuar", 36, new Color(0.2f, 0.45f, 0.2f), OnResultButtonClicked);
        UiBuilder.SetAnchors(continueButton.GetComponent<RectTransform>(), 0.38f, 0.30f, 0.62f, 0.42f, 0f);

        resultPanel.SetActive(false);
    }

    private Text BuildTextPanel(Transform parent, string name, float xMin, float yMin, float xMax, float yMax, int fontSize, TextAnchor align)
    {
        RectTransform panel = UiBuilder.CreatePanel(name, parent, new Color(0.12f, 0.11f, 0.18f, 0.95f));
        UiBuilder.SetAnchors(panel, xMin, yMin, xMax, yMax, 0f);
        panel.GetComponent<Image>().raycastTarget = false;

        Text text = UiBuilder.CreateText(name + "Texto", panel, "", fontSize, align, Color.white);
        UiBuilder.SetAnchors(text.rectTransform, 0f, 0f, 1f, 1f, 8f);
        return text;
    }

    private void BuildRow(CardView[] views, SlotKind kind, Transform parent, float xMin, float yMin, float xMax, float yMax, bool vertical)
    {
        RectTransform row = UiBuilder.CreatePanel(kind + "_Linha", parent, new Color(0f, 0f, 0f, 0f));
        UiBuilder.SetAnchors(row, xMin, yMin, xMax, yMax, 0f);
        row.GetComponent<Image>().raycastTarget = false;

        int count = views.Length;
        for (int i = 0; i < count; i++)
        {
            CardView view = CardView.Create(row, kind, i);
            RectTransform rect = view.GetComponent<RectTransform>();

            if (vertical)
            {
                UiBuilder.SetAnchors(rect, 0f, (float)(count - 1 - i) / count, 1f, (float)(count - i) / count, 0f);
            }
            else
            {
                UiBuilder.SetAnchors(rect, (float)i / count, 0f, (float)(i + 1) / count, 1f, 0f);
            }

            view.Clicked += OnSlotClicked;
            view.Hovered += OnSlotHovered;
            views[i] = view;
        }
    }

    // ---------- Início e controle do duelo ----------

    private void StartNewDuel()
    {
        StopAllCoroutines();
        busy = false;
        selectedSlot = -1;
        resultPanel.SetActive(false);
        detailText.text = "";

        game = new DuelGame(currentPlayerName, currentPlayerDeck, currentOpponentName, currentOpponentDeck);

        // A campanha pode aplicar regras especiais antes de o duelo começar.
        if (configureGame != null)
        {
            configureGame(game);
        }

        game.StartDuel();

        SetStatus("Seu turno. Clique em uma carta da mão para jogá-la.");
        Refresh();
    }

    private void OnResultButtonClicked()
    {
        bool won = game != null && game.Winner != null && game.Winner == game.Players[0];
        System.Action<bool> callback = onFinished;

        Hide();

        if (callback != null)
        {
            callback(won);
        }
    }

    private bool IsPlayerTurn()
    {
        return game != null && !game.IsOver && !busy && game.Current == game.Players[0];
    }

    private void SetStatus(string message)
    {
        statusText.text = message;
    }

    // ---------- Cliques ----------

    private void OnSlotClicked(CardView view)
    {
        if (!IsPlayerTurn())
        {
            return;
        }

        switch (view.Kind)
        {
            case SlotKind.Hand:
                HandleHandClick(view);
                break;
            case SlotKind.PlayerField:
                HandleOwnFieldClick(view);
                break;
            case SlotKind.OpponentField:
                HandleEnemyFieldClick(view);
                break;
        }

        Refresh();
    }

    private void HandleHandClick(CardView view)
    {
        CardData card = view.Card;
        if (card == null)
        {
            return;
        }

        if (game.PlayCard(card))
        {
            selectedSlot = -1;
            SetStatus("Você jogou " + card.ShownName + ".");
        }
        else
        {
            SetStatus(WhyCannotPlay(card));
        }
    }

    private string WhyCannotPlay(CardData card)
    {
        DuelPlayer me = game.Players[0];

        if (me.Ether < card.EtherCost)
        {
            return "Éter insuficiente: essa carta custa " + card.EtherCost + ".";
        }

        if (card.Type == CardType.Ser)
        {
            return "Seu campo está cheio.";
        }

        if (card.Type == CardType.Vigilia)
        {
            return "Você já tem " + DuelPlayer.MaxVigils + " Vigílias.";
        }

        return "Não dá para jogar essa carta agora.";
    }

    private void HandleOwnFieldClick(CardView view)
    {
        CreatureInstance creature = game.Players[0].Field[view.Index];
        if (creature == null)
        {
            selectedSlot = -1;
            return;
        }

        if (selectedSlot == view.Index)
        {
            selectedSlot = -1;
            SetStatus("Seleção cancelada.");
            return;
        }

        selectedSlot = view.Index;

        string info = creature.Card.ShownName + " selecionado. ";

        if (creature.IsGuarding)
        {
            info += "Ele está em Postura de Guarda e não ataca (use o botão para sair da guarda).";
        }
        else
        {
            info += game.CanAttackWith(view.Index)
                ? "Clique em um alvo inimigo para atacar."
                : "Ele não pode atacar agora.";
        }

        if (game.CanAscendSlot(view.Index))
        {
            info += " Você pode usar Ascender.";
        }

        SetStatus(info);
    }

    private void HandleEnemyFieldClick(CardView view)
    {
        if (game.Players[1].Field[view.Index] == null)
        {
            SetStatus("Não há Ser nessa zona.");
            return;
        }

        if (selectedSlot < 0)
        {
            SetStatus("Selecione primeiro um Ser seu para atacar.");
            return;
        }

        if (!game.CanAttackWith(selectedSlot))
        {
            SetStatus("Esse Ser não pode atacar agora.");
            return;
        }

        if (!game.GetLegalTargets(selectedSlot).Contains(view.Index))
        {
            SetStatus("Alvo inválido: há Seres que precisam ser atacados primeiro (Provocar ou Voo).");
            return;
        }

        game.Attack(selectedSlot, view.Index);
        selectedSlot = -1;
        SetStatus("Ataque realizado.");
    }

    private void OnOpponentInfoClicked()
    {
        if (!IsPlayerTurn())
        {
            return;
        }

        if (selectedSlot < 0)
        {
            SetStatus("Selecione primeiro um Ser seu para atacar.");
        }
        else if (!game.CanAttackWith(selectedSlot))
        {
            SetStatus("Esse Ser não pode atacar agora.");
        }
        else if (!game.GetLegalTargets(selectedSlot).Contains(-1))
        {
            SetStatus("Você não pode atacar a Essência agora: ataque um Ser do rival.");
        }
        else
        {
            game.Attack(selectedSlot, -1);
            selectedSlot = -1;
            SetStatus("Ataque direto!");
        }

        Refresh();
    }

    private void OnAscendClicked()
    {
        if (!IsPlayerTurn() || selectedSlot < 0)
        {
            return;
        }

        if (!game.CanAscendSlot(selectedSlot))
        {
            SetStatus("Esse Ser não pode ascender agora (sem Ascensão ou Éter insuficiente).");
        }
        else
        {
            string before = game.Players[0].Field[selectedSlot].Card.ShownName;
            game.Ascend(selectedSlot);
            SetStatus("ASCENSÃO! " + before + " se transformou.");
        }

        Refresh();
    }

    private void OnGuardClicked()
    {
        if (!IsPlayerTurn() || selectedSlot < 0)
        {
            return;
        }

        if (!game.CanToggleGuard(selectedSlot))
        {
            SetStatus("Esse Ser já atacou neste turno e não pode assumir a Postura de Guarda.");
        }
        else
        {
            bool wasGuarding = game.Players[0].Field[selectedSlot].IsGuarding;
            game.ToggleGuard(selectedSlot);

            if (wasGuarding)
            {
                SetStatus("Postura de Guarda desfeita.");
            }
            else
            {
                SetStatus("Postura de Guarda: ele recebe " + DuelGame.GuardReduction + " de dano a menos, mas não ataca.");
            }
        }

        Refresh();
    }

    private void OnEndTurnClicked()
    {
        if (!IsPlayerTurn())
        {
            return;
        }

        StartCoroutine(OpponentTurnRoutine());
    }

    // O oponente joga o turno dele com pausas, para você acompanhar.
    private IEnumerator OpponentTurnRoutine()
    {
        busy = true;
        selectedSlot = -1;
        SetStatus("Turno do oponente...");

        game.EndTurn();
        Refresh();

        if (!game.IsOver)
        {
            yield return new WaitForSeconds(1.0f);
            DuelAI.PlayTurn(game);
            Refresh();
        }

        if (!game.IsOver)
        {
            yield return new WaitForSeconds(1.0f);
            game.EndTurn();
        }

        busy = false;

        if (!game.IsOver)
        {
            SetStatus("Seu turno.");
        }

        Refresh();
    }

    // ---------- Atualização da tela ----------

    private void OnSlotHovered(CardView view)
    {
        detailText.text = BuildDetails(view);
    }

    private string BuildDetails(CardView view)
    {
        if (view.IsFaceDown)
        {
            return "<b>Vigília do oponente</b>\nVirada para baixo.";
        }

        CardData card = view.Card;
        if (card == null)
        {
            return "";
        }

        string text = "<b>" + card.ShownName + "</b>\n";
        text += card.Type + " | " + card.Faction + " | " + card.Rarity + "\n";
        text += "Custo de Éter: " + card.EtherCost + "\n";

        if (card.IsCreature)
        {
            int attack = view.Creature != null ? view.Creature.Attack : card.Attack;
            int health = view.Creature != null ? view.Creature.Health : card.Health;
            text += "ATQ " + attack + " / VIDA " + health + "\n";
        }

        if (card.HasProvoke)
        {
            text += "Provocar: deve ser atacado primeiro.\n";
        }

        if (card.HasHaste)
        {
            text += "Ataque imediato: ataca no turno em que entra.\n";
        }

        if (card.HasFlying)
        {
            text += "Voo: pode atacar a Essência mesmo com Seres no campo, se o rival não tiver Seres com Voo.\n";
        }

        if (view.Creature != null && view.Creature.IsGuarding)
        {
            text += "Em Postura de Guarda: recebe " + DuelGame.GuardReduction + " de dano a menos em ataques e não ataca.\n";
        }

        text += "\n" + card.EffectText;

        if (card.CanAscend)
        {
            text += "\n\nAscensão (" + card.AscensionCost + " Éter): " + card.AscensionTarget.ShownName;
        }

        return text;
    }

    private string BuildInfo(DuelPlayer player)
    {
        string turnMark = (!game.IsOver && player == game.Current) ? "   [TURNO]" : "";
        return player.Name + "   Essência " + player.Essence + "   Éter " + player.Ether + "/" + player.MaxEther
            + "   Mão " + player.Hand.Count + "   Baralho " + player.Deck.Count + turnMark;
    }

    private void Refresh()
    {
        if (game == null)
        {
            return;
        }

        DuelPlayer me = game.Players[0];
        DuelPlayer enemy = game.Players[1];
        bool myTurn = IsPlayerTurn();

        if (selectedSlot >= 0 && me.Field[selectedSlot] == null)
        {
            selectedSlot = -1;
        }

        bool canAttack = myTurn && selectedSlot >= 0 && game.CanAttackWith(selectedSlot);
        List<int> legal = canAttack ? game.GetLegalTargets(selectedSlot) : new List<int>();

        // Mão.
        for (int i = 0; i < handViews.Length; i++)
        {
            CardView view = handViews[i];
            if (i < me.Hand.Count)
            {
                CardData card = me.Hand[i];
                view.ShowCard(card);
                view.SetFrame(myTurn && game.CanPlayCard(card) ? GreenFrame : NoFrame);
            }
            else
            {
                view.ShowEmpty();
                view.SetFrame(NoFrame);
            }
        }

        // Campo do jogador.
        for (int i = 0; i < playerFieldViews.Length; i++)
        {
            CardView view = playerFieldViews[i];
            CreatureInstance creature = me.Field[i];

            if (creature == null)
            {
                view.ShowEmpty();
                view.SetFrame(NoFrame);
            }
            else
            {
                view.ShowCreature(creature);

                if (i == selectedSlot)
                {
                    view.SetFrame(YellowFrame);
                }
                else
                {
                    bool ready = myTurn && creature.CanAttack && !creature.IsGuarding;
                    view.SetFrame(ready ? GreenFrame : NoFrame);
                }
            }
        }

        // Campo do oponente.
        for (int i = 0; i < opponentFieldViews.Length; i++)
        {
            CardView view = opponentFieldViews[i];
            CreatureInstance creature = enemy.Field[i];

            if (creature == null)
            {
                view.ShowEmpty();
                view.SetFrame(NoFrame);
            }
            else
            {
                view.ShowCreature(creature);
                view.SetFrame(legal.Contains(i) ? RedFrame : NoFrame);
            }
        }

        // Vigílias.
        for (int i = 0; i < playerVigilViews.Length; i++)
        {
            if (i < me.Vigils.Count)
            {
                playerVigilViews[i].ShowCard(me.Vigils[i]);
            }
            else
            {
                playerVigilViews[i].ShowEmpty();
            }

            playerVigilViews[i].SetFrame(NoFrame);
        }

        for (int i = 0; i < opponentVigilViews.Length; i++)
        {
            if (i < enemy.Vigils.Count)
            {
                opponentVigilViews[i].ShowFaceDown();
            }
            else
            {
                opponentVigilViews[i].ShowEmpty();
            }

            opponentVigilViews[i].SetFrame(NoFrame);
        }

        // Informações e botões.
        opponentInfoText.text = BuildInfo(enemy);
        playerInfoText.text = BuildInfo(me);
        opponentInfoFrame.color = legal.Contains(-1) ? RedFrame : NoFrame;

        endTurnButton.interactable = myTurn;
        ascendButton.interactable = myTurn && selectedSlot >= 0 && game.CanAscendSlot(selectedSlot);
        guardButton.interactable = myTurn && selectedSlot >= 0 && game.CanToggleGuard(selectedSlot);

        bool selectedIsGuarding = selectedSlot >= 0 && me.Field[selectedSlot] != null && me.Field[selectedSlot].IsGuarding;
        guardLabel.text = selectedIsGuarding ? "Sair da guarda" : "Postura de Guarda";

        RefreshLog();

        if (game.IsOver)
        {
            if (game.Winner == null)
            {
                resultText.text = "EMPATE";
            }
            else if (game.Winner == me)
            {
                resultText.text = "VITÓRIA!";
            }
            else
            {
                resultText.text = "DERROTA";
            }

            resultPanel.SetActive(true);
        }
    }

    private void RefreshLog()
    {
        List<string> lines = game.LogLines;
        int start = Mathf.Max(0, lines.Count - 14);

        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        for (int i = start; i < lines.Count; i++)
        {
            builder.AppendLine(lines[i]);
        }

        logText.text = builder.ToString();
    }
}