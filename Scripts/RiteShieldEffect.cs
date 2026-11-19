using UnityEngine;

// Efeito "marcador": não faz nada sozinho.
// O duelo procura por ele para saber que o Ser não pode ser alvo de Ritos inimigos.
[CreateAssetMenu(fileName = "Effect_RiteShield", menuName = "Duelo/Efeitos/Protegido de Ritos", order = 19)]
public class RiteShieldEffect : CardEffect
{
    public override void Execute(IEffectContext context)
    {
        // Intencionalmente vazio.
    }
}