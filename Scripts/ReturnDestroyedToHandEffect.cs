using UnityEngine;

[CreateAssetMenu(fileName = "Effect_ReturnToHand", menuName = "Duelo/Efeitos/Devolver Ser destruído à mão", order = 18)]
public class ReturnDestroyedToHandEffect : CardEffect
{
    public override void Execute(IEffectContext context)
    {
        context.ReturnLastDestroyedToHand();
    }
}