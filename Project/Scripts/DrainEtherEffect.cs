using UnityEngine;

[CreateAssetMenu(fileName = "Effect_DrainEther", menuName = "Duelo/Efeitos/Drenar Éter do oponente", order = 16)]
public class DrainEtherEffect : CardEffect
{
    [SerializeField, Min(1)] private int amount = 1;

    public int Amount => amount;

    public override void Execute(IEffectContext context)
    {
        context.DrainOpponentEther(amount);
    }
}