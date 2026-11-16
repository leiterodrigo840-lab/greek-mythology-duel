using System.Collections.Generic;
using UnityEngine;

// Esta linha faz aparecer o menu: botão direito > Create > Duelo > Carta
[CreateAssetMenu(fileName = "Card_New", menuName = "Duelo/Carta", order = 0)]
public class CardData : ScriptableObject
{
    [Header("Identidade")]
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private CardType type = CardType.Ser;
    [SerializeField] private Faction faction = Faction.Olimpico;
    [SerializeField] private Rarity rarity = Rarity.Comum;

    [Header("Custo e Status")]
    [SerializeField, Min(0)] private int etherCost = 1;
    [SerializeField, Min(0)] private int attack = 0;
    [SerializeField, Min(0)] private int health = 1;

    [Header("Texto e Arte")]
    [SerializeField, TextArea(2, 4)] private string effectText;
    [SerializeField] private Sprite artwork;

    [Header("Efeitos")]
    [SerializeField] private List<CardEffect> effects = new List<CardEffect>();

    [Header("Ascensão Mítica (SHIFT)")]
    [SerializeField] private CardData ascensionTarget;
    [SerializeField, Min(0)] private int ascensionCost = 2;

    // Leitura pública (outros scripts só leem, não alteram).
    public string Id => id;
    public string DisplayName => displayName;
    public CardType Type => type;
    public Faction Faction => faction;
    public Rarity Rarity => rarity;
    public int EtherCost => etherCost;
    public int Attack => attack;
    public int Health => health;
    public string EffectText => effectText;
    public Sprite Artwork => artwork;
    public IReadOnlyList<CardEffect> Effects => effects;
    public CardData AscensionTarget => ascensionTarget;
    public int AscensionCost => ascensionCost;

    public bool IsCreature => type == CardType.Ser;
    public bool CanAscend => ascensionTarget != null;

    // O Unity chama isto no editor sempre que você muda algo no Inspector.
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            id = name;
        }

        if (ascensionTarget == this)
        {
            ascensionTarget = null;
            Debug.LogWarning("Uma carta não pode ascender para ela mesma.");
        }
    }

#if UNITY_EDITOR
    // Métodos usados apenas pelo script de criação em lote (só existem no editor).
    public void EditorSetup(string newId, string newName, CardType newType, Faction newFaction,
        Rarity newRarity, int newCost, int newAttack, int newHealth, string newEffectText)
    {
        id = newId;
        displayName = newName;
        type = newType;
        faction = newFaction;
        rarity = newRarity;
        etherCost = newCost;
        attack = newAttack;
        health = newHealth;
        effectText = newEffectText;
    }

    public void EditorSetAscension(CardData target, int cost)
    {
        ascensionTarget = target;
        ascensionCost = cost;
    }
#endif
}