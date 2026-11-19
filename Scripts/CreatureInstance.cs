// Um Ser que está no campo durante o duelo.
// A carta (CardData) é o molde; esta classe guarda o estado ao vivo (vida atual etc.).
public class CreatureInstance
{
    public CardData Card { get; private set; }
    public DuelPlayer Owner { get; private set; }
    public int Attack { get; set; }
    public int Health { get; set; }
    public bool CanAttack { get; set; }
    public bool IsGuarding { get; set; }
    public bool AttackedThisTurn { get; set; }

    public CreatureInstance(CardData card, DuelPlayer owner)
    {
        Card = card;
        Owner = owner;
        Attack = card.Attack;
        Health = card.Health;
        CanAttack = false;
        IsGuarding = false;
        AttackedThisTurn = false;
    }
}