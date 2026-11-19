// Momento em que um efeito dispara.
public enum EffectTrigger
{
    AoEntrar,          // quando o Ser entra no campo, ou o Rito é usado
    FimDoTurno,        // no fim do turno do dono
    AoSerDestruido,    // o próprio Ser foi destruído (ex.: Hidra)
    AoSerAtacado,      // (reservado para uso futuro)
    AoInimigoAtacar,   // Vigília: quando um Ser inimigo ataca
    AoSeuSerDestruido  // Vigília: quando um Ser seu é destruído
}