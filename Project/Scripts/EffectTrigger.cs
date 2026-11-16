// Momento em que um efeito dispara.
public enum EffectTrigger
{
    AoEntrar,        // quando o Ser entra no campo, ou o Rito é usado
    FimDoTurno,      // no fim do turno do dono
    AoSerDestruido,  // quando o Ser é destruído
    AoSerAtacado     // quando o Ser (ou a Vigília) é atacado
}