using UnityEngine;

[CreateAssetMenu(fileName = "Effect_Draw", menuName = "Duelo/Efeitos/Comprar cartas", order = 12)]
public class DrawCardsEffect : CardEffect
{
    [SerializeField, Min(1)] private int amount = 1;

    public int Amount => amount;

    public override void Execute(IEffectContext context)
    {
        context.DrawCards(amount);
    }
}