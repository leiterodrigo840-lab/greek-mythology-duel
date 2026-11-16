using UnityEngine;

[CreateAssetMenu(fileName = "Effect_Heal", menuName = "Duelo/Efeitos/Curar dono", order = 10)]
public class HealOwnerEffect : CardEffect
{
    [SerializeField, Min(1)] private int amount = 1;

    public int Amount => amount;

    public override void Execute(IEffectContext context)
    {
        context.HealOwner(amount);
    }
}