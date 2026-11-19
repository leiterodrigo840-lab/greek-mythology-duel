using System.Collections.Generic;
using UnityEngine;

// Um duelista: Essência, Éter, baralho, mão, descarte, campo e Vigílias.
public class DuelPlayer
{
    public const int FieldSlots = 3;
    public const int MaxVigils = 2;
    public const int MaxHandSize = 7;
    public const int MaxEtherCap = 6;
    public const int StartEssence = 20;
    public const int StartHandSize = 4;

    public string Name { get; private set; }
    public int Essence { get; set; }
    public int Ether { get; set; }
    public int MaxEther { get; set; }
    public int EtherPenalty { get; set; }
    public int Ascensions { get; set; }
    public List<CardData> Deck { get; private set; }
    public List<CardData> Hand { get; private set; }
    public List<CardData> Discard { get; private set; }
    public List<CardData> Vigils { get; private set; }
    public CreatureInstance[] Field { get; private set; }

    public DuelPlayer(string name, DeckData deckData)
    {
        Name = name;
        Essence = StartEssence;
        Ether = 0;
        MaxEther = 0;
        EtherPenalty = 0;
        Ascensions = 0;

        Deck = new List<CardData>();
        foreach (CardData card in deckData.Cards)
        {
            if (card != null)
            {
                Deck.Add(card);
            }
        }

        Hand = new List<CardData>();
        Discard = new List<CardData>();
        Vigils = new List<CardData>();
        Field = new CreatureInstance[FieldSlots];
        Shuffle();
    }

    private void Shuffle()
    {
        for (int i = Deck.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            CardData temp = Deck[i];
            Deck[i] = Deck[j];
            Deck[j] = temp;
        }
    }

    public int FirstFreeSlot()
    {
        for (int i = 0; i < FieldSlots; i++)
        {
            if (Field[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    public int CreatureCount()
    {
        int count = 0;
        for (int i = 0; i < FieldSlots; i++)
        {
            if (Field[i] != null)
            {
                count++;
            }
        }
        return count;
    }
}