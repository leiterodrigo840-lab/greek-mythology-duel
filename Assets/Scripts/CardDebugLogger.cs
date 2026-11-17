using UnityEngine;

// Script de teste temporário: mostra os dados de uma carta no Console.
public class CardDebugLogger : MonoBehaviour
{
    [SerializeField] private CardData card;

    private void Start()
    {
        if (card == null)
        {
            Debug.LogWarning("Arraste uma carta para o campo 'Card' no Inspector.");
            return;
        }

        Debug.Log($"{card.DisplayName} | {card.Type} | {card.Faction} | Custo {card.EtherCost} | {card.Attack}/{card.Health}");
        Debug.Log($"Efeito: {card.EffectText}");

        if (card.CanAscend)
        {
            Debug.Log($"Ascensão ({card.AscensionCost} Éter) -> {card.AscensionTarget.DisplayName}");
        }
    }
}