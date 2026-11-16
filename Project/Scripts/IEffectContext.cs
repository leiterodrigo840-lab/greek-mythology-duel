// "Pedidos" que um efeito pode fazer ao duelo.
// O sistema de duelo (Etapa 4) vai implementar isto de verdade.
public interface IEffectContext
{
    void HealOwner(int amount);        // cura a Essência do dono da carta
    void DamageOpponent(int amount);   // causa dano à Essência do oponente
    void DrawCards(int amount);        // o dono compra cartas
}