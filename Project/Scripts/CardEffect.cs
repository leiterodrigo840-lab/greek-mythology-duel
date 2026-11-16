using UnityEngine;

// Base de todos os efeitos. É abstrata: você não cria "CardEffect" direto,
// só os efeitos concretos que herdam dela.
public abstract class CardEffect : ScriptableObject
{
    [SerializeField] private EffectTrigger trigger = EffectTrigger.AoEntrar;
    [SerializeField, TextArea(1, 3)] private string description;

    public EffectTrigger Trigger => trigger;
    public string Description => description;

    // Cada efeito concreto escreve aqui o que faz.
    public abstract void Execute(IEffectContext context);
}