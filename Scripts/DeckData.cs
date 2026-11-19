using System.Collections.Generic;
using UnityEngine;

// Um baralho. Pode ser montado por lista de cartas, por receita em texto, ou pelos dois.
[CreateAssetMenu(fileName = "Deck_New", menuName = "Duelo/Deck", order = 1)]
public class DeckData : ScriptableObject
{
    [SerializeField] private string deckName;

    [Header("Opção 1: lista direta de cartas")]
    [SerializeField] private List<CardData> cards = new List<CardData>();

    [Header("Opção 2: receita em texto")]
    [Tooltip("Um deck cuja lista de cartas serve de biblioteca para a receita.")]
    [SerializeField] private DeckData library;
    [SerializeField, TextArea(8, 16)] private string recipe;

    public string DeckName => deckName;

    // Todas as cartas do deck (lista direta + receita).
    public IReadOnlyList<CardData> Cards => BuildCards();

    private List<CardData> BuildCards()
    {
        List<CardData> result = new List<CardData>();

        foreach (CardData card in cards)
        {
            if (card != null)
            {
                result.Add(card);
            }
        }

        if (library == null || string.IsNullOrWhiteSpace(recipe))
        {
            return result;
        }

        string[] lines = recipe.Split('\n');
        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            // Linhas vazias e comentários (que começam com #) são ignorados.
            if (line.Length == 0 || line.StartsWith("#"))
            {
                continue;
            }

            string cardName = line;
            int count = 1;

            int colon = line.LastIndexOf(':');
            if (colon >= 0)
            {
                cardName = line.Substring(0, colon).Trim();
                if (!int.TryParse(line.Substring(colon + 1).Trim(), out count))
                {
                    count = 1;
                }
            }

            CardData found = library.FindCardByName(cardName);
            if (found == null)
            {
                Debug.LogWarning("Receita do deck '" + name + "': carta não encontrada na biblioteca: " + cardName);
                continue;
            }

            for (int i = 0; i < count; i++)
            {
                result.Add(found);
            }
        }

        return result;
    }

    // Procura uma carta da lista direta pelo nome do arquivo (ex.: Card_Nyx).
    public CardData FindCardByName(string cardName)
    {
        foreach (CardData card in cards)
        {
            if (card != null && card.name == cardName)
            {
                return card;
            }
        }

        return null;
    }

    // Cria um deck novo só na memória (usado para somar recompensas da campanha).
    public static DeckData CreateRuntime(string newName, IEnumerable<CardData> allCards)
    {
        DeckData deck = CreateInstance<DeckData>();
        deck.deckName = newName;
        deck.cards = new List<CardData>(allCards);
        return deck;
    }

    // Ferramenta de ajuda: botão "⋮" do Inspector > escreve no Console os nomes das cartas.
    [ContextMenu("Listar nomes das cartas desta lista")]
    private void LogCardNames()
    {
        foreach (CardData card in cards)
        {
            if (card != null)
            {
                Debug.Log(card.name);
            }
        }
    }
}