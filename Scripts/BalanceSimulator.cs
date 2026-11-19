using UnityEngine;

// Roda muitos duelos IA contra IA e mostra no Console quem vence mais.
// Serve para ajustar decks e dificuldade com números, e não no chute.
public class BalanceSimulator : MonoBehaviour
{
    [Header("Lado A")]
    [SerializeField] private DeckData deckA;
    [SerializeField] private AIProfile profileA = AIProfile.Equilibrado;
    [SerializeField] private AIDifficulty difficultyA = AIDifficulty.Normal;

    [Header("Lado B")]
    [SerializeField] private DeckData deckB;
    [SerializeField] private AIProfile profileB = AIProfile.Equilibrado;
    [SerializeField] private AIDifficulty difficultyB = AIDifficulty.Normal;

    [Header("Simulação")]
    [SerializeField, Range(10, 1000)] private int duelCount = 200;

    // Use: botão "⋮" do componente > "Rodar simulação".
    [ContextMenu("Rodar simulação")]
    private void RunSimulation()
    {
        if (deckA == null || deckB == null || deckA.Cards.Count == 0 || deckB.Cards.Count == 0)
        {
            Debug.LogWarning("Arraste um Deck com cartas para 'Deck A' e 'Deck B'.");
            return;
        }

        int winsA = 0;
        int winsB = 0;
        int draws = 0;
        int totalTurns = 0;
        int ascensionsA = 0;
        int ascensionsB = 0;

        for (int i = 0; i < duelCount; i++)
        {
            // Metade das partidas o lado A começa, na outra metade o B começa (para ser justo).
            bool aFirst = i % 2 == 0;

            DuelGame game = aFirst
                ? new DuelGame("A", deckA, "B", deckB)
                : new DuelGame("B", deckB, "A", deckA);

            game.StartDuel();

            int safety = 0;
            while (!game.IsOver && safety < 200)
            {
                bool aTurn = (game.Current == game.Players[0]) == aFirst;

                if (aTurn)
                {
                    DuelAI.PlayTurn(game, profileA, difficultyA);
                }
                else
                {
                    DuelAI.PlayTurn(game, profileB, difficultyB);
                }

                if (!game.IsOver)
                {
                    game.EndTurn();
                }

                safety++;
            }

            DuelPlayer playerA = aFirst ? game.Players[0] : game.Players[1];
            DuelPlayer playerB = aFirst ? game.Players[1] : game.Players[0];

            totalTurns += game.TurnNumber;
            ascensionsA += playerA.Ascensions;
            ascensionsB += playerB.Ascensions;

            if (game.Winner == null)
            {
                draws++;
            }
            else if (game.Winner == playerA)
            {
                winsA++;
            }
            else
            {
                winsB++;
            }
        }

        float pctA = 100f * winsA / duelCount;
        float pctB = 100f * winsB / duelCount;
        float pctDraw = 100f * draws / duelCount;

        Debug.Log(
            "=== Resultado de " + duelCount + " duelos ===\n" +
            "A (" + deckA.name + ", " + profileA + "/" + difficultyA + "): " + pctA.ToString("F1") + "% de vitórias\n" +
            "B (" + deckB.name + ", " + profileB + "/" + difficultyB + "): " + pctB.ToString("F1") + "% de vitórias\n" +
            "Empates: " + pctDraw.ToString("F1") + "%\n" +
            "Duração média: " + ((float)totalTurns / duelCount).ToString("F1") + " turnos (somando os dois lados)\n" +
            "Ascensões por duelo: A " + ((float)ascensionsA / duelCount).ToString("F2") +
            " | B " + ((float)ascensionsB / duelCount).ToString("F2"));
    }
}