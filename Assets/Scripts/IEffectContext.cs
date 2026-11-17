// "Pedidos" que um efeito pode fazer ao duelo.
public interface IEffectContext
{
    void HealOwner(int amount);
    void DamageOpponent(int amount);
    void DrawCards(int amount);

    void DamageStrongestEnemy(int amount);         // dano ao Ser inimigo mais forte
    void DestroyStrongestEnemy(int maxCost);       // destrói o mais forte com custo até maxCost
    void SummonCreatures(CardData card, int count);// invoca cópias da carta no campo do dono
    void DrainOpponentEther(int amount);           // oponente perde Éter no próximo turno
    void NegateCurrentAttack();                    // anula o ataque em andamento
    void ReturnLastDestroyedToHand();              // devolve à mão o último Ser destruído
}