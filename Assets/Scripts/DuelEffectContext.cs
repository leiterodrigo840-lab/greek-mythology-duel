// Liga os efeitos das cartas ao duelo de verdade.
public class DuelEffectContext : IEffectContext
{
    private readonly DuelGame game;
    private readonly DuelPlayer owner;
    private readonly DuelPlayer opponent;
    private readonly bool fromRite;

    public DuelEffectContext(DuelGame game, DuelPlayer owner, DuelPlayer opponent, CardData sourceCard)
    {
        this.game = game;
        this.owner = owner;
        this.opponent = opponent;
        // Se a carta que gerou o efeito é um Rito, o Herói Lendário fica protegido.
        fromRite = sourceCard != null && sourceCard.Type == CardType.Rito;
    }

    public void HealOwner(int amount)
    {
        game.HealPlayer(owner, amount);
    }

    public void DamageOpponent(int amount)
    {
        game.DamagePlayer(opponent, amount);
    }

    public void DrawCards(int amount)
    {
        game.DrawCards(owner, amount);
    }

    public void DamageStrongestEnemy(int amount)
    {
        game.DamageStrongestEnemyCreature(owner, fromRite, amount);
    }

    public void DestroyStrongestEnemy(int maxCost)
    {
        game.DestroyStrongestEnemyCreature(owner, fromRite, maxCost);
    }

    public void SummonCreatures(CardData card, int count)
    {
        game.SummonCreatures(owner, card, count);
    }

    public void DrainOpponentEther(int amount)
    {
        game.DrainEther(opponent, amount);
    }

    public void NegateCurrentAttack()
    {
        game.NegateAttack();
    }

    public void ReturnLastDestroyedToHand()
    {
        game.ReturnLastDestroyedToHand(owner);
    }
}