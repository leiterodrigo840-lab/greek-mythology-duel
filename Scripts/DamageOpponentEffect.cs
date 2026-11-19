using UnityEngine;

[CreateAssetMenu(fileName = "Effect_Damage", menuName = "Duelo/Efeitos/Dano ao oponente", order = 11)]
public class DamageOpponentEffect : CardEffect
{
    [SerializeField, Min(1)] private int amount = 1;

    public int Amount => amount;

    public override void Execute(IEffectContext context)
    {
        context.DamageOpponent(amount);
    }
}