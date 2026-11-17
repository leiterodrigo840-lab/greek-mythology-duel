using UnityEngine;

[CreateAssetMenu(fileName = "Effect_DamageEnemy", menuName = "Duelo/Efeitos/Dano ao Ser inimigo mais forte", order = 13)]
public class DamageStrongestEnemyEffect : CardEffect
{
    [SerializeField, Min(1)] private int amount = 1;

    public int Amount => amount;

    public override void Execute(IEffectContext context)
    {
        context.DamageStrongestEnemy(amount);
    }
}