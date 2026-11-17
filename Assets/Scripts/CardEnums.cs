// Tipos de carta do jogo.
public enum CardType
{
    Ser,      // criatura que fica no campo
    Rito,     // efeito imediato
    Vigilia   // fica virada para baixo e dispara depois
}

// Facção (define a cor da moldura mais adiante).
public enum Faction
{
    Olimpico,
    Titanico,
    Ctonico,
    Heroico,
    Monstruoso,
    Destino
}

// Raridade da carta.
public enum Rarity
{
    Comum,
    Rara,
    Epica,
    Lendaria
}