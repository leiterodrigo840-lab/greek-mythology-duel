using UnityEngine;

[CreateAssetMenu(fileName = "Effect_DestroyEnemy", menuName = "Duelo/Efeitos/Destruir Ser inimigo mais forte", order = 14)]
public class DestroyStrongestEnemyEffect : CardEffect
{
    // Só pode destruir Seres com custo até este valor.
    [SerializeField, Min(0)] private int maxCost = 3;

    public int MaxCost => maxCost;

    public override void Execute(IEffectContext context)
    {
        context.DestroyStrongestEnemy(maxCost);
    }
}