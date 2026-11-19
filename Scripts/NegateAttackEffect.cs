using UnityEngine;

[CreateAssetMenu(fileName = "Effect_NegateAttack", menuName = "Duelo/Efeitos/Anular ataque", order = 17)]
public class NegateAttackEffect : CardEffect
{
    public override void Execute(IEffectContext context)
    {
        context.NegateCurrentAttack();
    }
}