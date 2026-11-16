using UnityEngine;

// Simula o duelo: executa os efeitos de uma carta e escreve o resultado no Console.
public class EffectTester : MonoBehaviour, IEffectContext
{
    [SerializeField] private CardData card;
    [SerializeField] private EffectTrigger trigger = EffectTrigger.AoEntrar;

    private void Start()
    {
        if (card == null)
        {
            Debug.LogWarning("Arraste uma carta para o campo 'Card'.");
            return;
        }

        int executed = 0;

        foreach (CardEffect effect in card.Effects)
        {
            if (effect == null)
            {
                continue;
            }

            if (effect.Trigger == trigger)
            {
                effect.Execute(this);
                executed++;
            }
        }

        Debug.Log($"[Teste] {card.DisplayName} | gatilho {trigger} | efeitos executados: {executed}");
    }

    public void HealOwner(int amount)
    {
        Debug.Log($"[Teste] Dono cura {amount} de Essência.");
    }

    public void DamageOpponent(int amount)
    {
        Debug.Log($"[Teste] Oponente recebe {amount} de dano.");
    }

    public void DrawCards(int amount)
    {
        Debug.Log($"[Teste] Dono compra {amount} carta(s).");
    }
}