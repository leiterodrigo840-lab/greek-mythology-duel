using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum SlotKind
{
    Hand,
    PlayerField,
    OpponentField,
    PlayerVigil,
    OpponentVigil
}

// Uma zona da tela que mostra uma carta, um Ser, uma Vigília virada ou um espaço vazio.
public class CardView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    public SlotKind Kind { get; private set; }
    public int Index { get; private set; }
    public CardData Card { get; private set; }
    public CreatureInstance Creature { get; private set; }
    public bool IsFaceDown { get; private set; }

    public System.Action<CardView> Clicked;
    public System.Action<CardView> Hovered;

    private Image frame;
    private Image body;
    private Text topText;
    private Text bodyText;
    private Text bottomText;

    public static CardView Create(Transform parent, SlotKind kind, int index)
    {
        RectTransform root = UiBuilder.CreatePanel("Zona_" + kind + "_" + index, parent, new Color(0f, 0f, 0f, 0f));
        CardView view = root.gameObject.AddComponent<CardView>();
        view.Build(kind, index);
        return view;
    }

    private void Build(SlotKind kind, int index)
    {
        Kind = kind;
        Index = index;
        frame = GetComponent<Image>();

        RectTransform bodyRect = UiBuilder.CreatePanel("Corpo", transform, new Color(0.15f, 0.15f, 0.2f));
        UiBuilder.SetAnchors(bodyRect, 0f, 0f, 1f, 1f, 5f);
        body = bodyRect.GetComponent<Image>();
        body.raycastTarget = false;

        topText = UiBuilder.CreateText("Topo", bodyRect, "", 18, TextAnchor.UpperLeft, Color.white);
        UiBuilder.SetAnchors(topText.rectTransform, 0f, 0.68f, 1f, 1f, 4f);
        UiBuilder.EnableBestFit(topText, 9, 18);

        bodyText = UiBuilder.CreateText("Efeito", bodyRect, "", 14, TextAnchor.UpperLeft, new Color(0.92f, 0.92f, 0.92f));
        UiBuilder.SetAnchors(bodyText.rectTransform, 0f, 0.25f, 1f, 0.68f, 4f);
        UiBuilder.EnableBestFit(bodyText, 8, 14);

        bottomText = UiBuilder.CreateText("Status", bodyRect, "", 18, TextAnchor.MiddleCenter, Color.white);
        UiBuilder.SetAnchors(bottomText.rectTransform, 0f, 0f, 1f, 0.25f, 4f);
        UiBuilder.EnableBestFit(bottomText, 9, 18);

        ShowEmpty();
    }

    // ---------- O que a zona mostra ----------

    public void ShowEmpty()
    {
        Card = null;
        Creature = null;
        IsFaceDown = false;
        body.color = new Color(0.1f, 0.1f, 0.14f, 0.55f);
        topText.text = "";
        bodyText.text = "";
        bottomText.text = "";
    }

    public void ShowFaceDown()
    {
        Card = null;
        Creature = null;
        IsFaceDown = true;
        body.color = new Color(0.25f, 0.18f, 0.35f);
        topText.text = "Vigília";
        bodyText.text = "Virada para baixo";
        bottomText.text = "?";
    }

    public void ShowCard(CardData card)
    {
        Card = card;
        Creature = null;
        IsFaceDown = false;
        body.color = FactionColor(card.Faction);
        topText.text = "(" + card.EtherCost + ") " + card.ShownName;
        bodyText.text = card.EffectText;
        bottomText.text = card.Type == CardType.Ser
            ? "ATQ " + card.Attack + "  VIDA " + card.Health
            : card.Type.ToString();
    }

    public void ShowCreature(CreatureInstance creature)
    {
        Card = creature.Card;
        Creature = creature;
        IsFaceDown = false;
        body.color = FactionColor(creature.Card.Faction);
        topText.text = creature.Card.ShownName;
        bodyText.text = creature.Card.EffectText;
        bottomText.text = "ATQ " + creature.Attack + "  VIDA " + creature.Health;
    }

    public void SetFrame(Color color)
    {
        frame.color = color;
    }

    private static Color FactionColor(Faction faction)
    {
        switch (faction)
        {
            case Faction.Olimpico: return new Color(0.55f, 0.43f, 0.10f);
            case Faction.Titanico: return new Color(0.55f, 0.20f, 0.15f);
            case Faction.Ctonico: return new Color(0.35f, 0.20f, 0.50f);
            case Faction.Heroico: return new Color(0.15f, 0.35f, 0.60f);
            case Faction.Monstruoso: return new Color(0.20f, 0.45f, 0.20f);
            default: return new Color(0.25f, 0.40f, 0.42f);
        }
    }

    // ---------- Mouse ----------

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Clicked != null)
        {
            Clicked(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Hovered != null)
        {
            Hovered(this);
        }
    }
}