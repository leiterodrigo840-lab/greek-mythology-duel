using UnityEngine;

// Roda um duelo completo entre duas IAs e escreve tudo no Console.
public class DuelSimulator : MonoBehaviour
{
    [SerializeField] private DeckData deckA;
    [SerializeField] private DeckData deckB;
    [SerializeField] private string nameA = "Oráculo";
    [SerializeField] private string nameB = "Rival";

    private void Start()
    {
        if (deckA == null || deckB == null || deckA.Cards.Count == 0 || deckB.Cards.Count == 0)
        {
            Debug.LogWarning("Arraste um Deck com cartas para 'Deck A' e 'Deck B'.");
            return;
        }

        DuelGame game = new DuelGame(nameA, deckA, nameB, deckB);
        game.LogAdded += message => Debug.Log(message);

        game.StartDuel();

        // Trava de segurança: o duelo nunca passa de 200 repetições.
        int safety = 0;
        while (!game.IsOver && safety < 200)
        {
            SimpleAutoPlayer.PlayTurn(game);

            if (!game.IsOver)
            {
                game.EndTurn();
            }

            safety++;
        }

        Debug.Log("Fim. Ascensões: " + nameA + " " + game.Players[0].Ascensions + " | " + nameB + " " + game.Players[1].Ascensions);
    }
}