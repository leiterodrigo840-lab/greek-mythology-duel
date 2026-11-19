using System.Collections.Generic;
using UnityEngine;

// A IA do oponente.
// A cada passo ela lista todas as jogadas válidas, dá uma nota a cada uma
// e executa a melhor. O perfil muda os pesos das notas; a dificuldade muda o quanto ela erra.
public static class DuelAI
{
    private const int MaxActionsPerTurn = 30;
    private const float MinScoreToAct = 0.5f;

    private enum ActionType
    {
        Play,
        Ascend,
        Attack
    }

    private class AIAction
    {
        public ActionType Type;
        public CardData Card;
        public int Slot;
        public int Target;
        public float Score;
    }

    // Pesos do perfil. Maior = a IA liga mais para isso.
    private class Weights
    {
        public float Face = 1f;     // dano direto na Essência
        public float Kill = 1f;     // eliminar Seres inimigos
        public float Safety = 1f;   // não perder os próprios Seres
        public float Develop = 1f;  // colocar Seres no campo
        public float Ascend = 1f;   // fazer Ascensão
        public float Rite = 1f;     // usar Ritos
        public float Vigil = 1f;    // preparar Vigílias
    }

    // Joga o turno do jogador atual usando o perfil e a dificuldade guardados no DuelGame.
    public static void PlayTurn(DuelGame game)
    {
        PlayTurn(game, game.AiProfile, game.AiDifficulty);
    }

    public static void PlayTurn(DuelGame game, AIProfile profile, AIDifficulty difficulty)
    {
        Weights weights = GetWeights(profile);
        float noise = GetNoise(profile, difficulty);
        float mistakeChance = GetMistakeChance(profile, difficulty);

        for (int step = 0; step < MaxActionsPerTurn; step++)
        {
            if (game.IsOver)
            {
                return;
            }

            List<AIAction> actions = BuildActions(game, weights);
            if (actions.Count == 0)
            {
                return;
            }

            AIAction chosen = null;
            bool mistake = mistakeChance > 0f && Random.value < mistakeChance;

            if (mistake)
            {
                // Erro "humano": faz uma jogada válida qualquer.
                chosen = actions[Random.Range(0, actions.Count)];
            }
            else
            {
                float bestScore = float.MinValue;

                foreach (AIAction action in actions)
                {
                    float noisy = action.Score + Random.Range(-noise, noise);
                    if (noisy > bestScore)
                    {
                        bestScore = noisy;
                        chosen = action;
                    }
                }

                // Se nenhuma jogada vale a pena, termina o turno.
                if (chosen == null || bestScore < MinScoreToAct)
                {
                    return;
                }
            }

            if (!Execute(game, chosen))
            {
                return;
            }
        }
    }

    // ---------- Perfis e dificuldade ----------

    private static Weights GetWeights(AIProfile profile)
    {
        Weights w = new Weights();

        switch (profile)
        {
            case AIProfile.Agressivo:
                w.Face = 1.7f;
                w.Kill = 0.8f;
                w.Safety = 0.5f;
                w.Develop = 1.1f;
                break;

            case AIProfile.Defensivo:
                w.Face = 0.6f;
                w.Kill = 1.2f;
                w.Safety = 1.7f;
                w.Vigil = 1.3f;
                break;

            case AIProfile.Controlador:
                w.Face = 0.7f;
                w.Kill = 1.6f;
                w.Safety = 1.2f;
                w.Ascend = 1.2f;
                w.Rite = 1.4f;
                w.Vigil = 1.4f;
                break;

            case AIProfile.Caotico:
                w.Face = 1.2f;
                w.Safety = 0.7f;
                break;
        }

        return w;
    }

    // Ruído somado às notas: quanto maior, mais a IA "se confunde".
    private static float GetNoise(AIProfile profile, AIDifficulty difficulty)
    {
        float noise = 1.0f;

        if (difficulty == AIDifficulty.Facil)
        {
            noise = 3.0f;
        }
        else if (difficulty == AIDifficulty.Dificil)
        {
            noise = 0.25f;
        }

        if (profile == AIProfile.Caotico)
        {
            noise += 3.0f;
        }

        return noise;
    }

    // Chance de, em vez da melhor jogada, fazer uma jogada válida qualquer.
    private static float GetMistakeChance(AIProfile profile, AIDifficulty difficulty)
    {
        float chance = 0.05f;

        if (difficulty == AIDifficulty.Facil)
        {
            chance = 0.20f;
        }
        else if (difficulty == AIDifficulty.Dificil)
        {
            chance = 0f;
        }

        if (profile == AIProfile.Caotico)
        {
            chance += 0.10f;
        }

        return chance;
    }

    // ---------- Lista de jogadas e notas ----------

    private static List<AIAction> BuildActions(DuelGame game, Weights w)
    {
        List<AIAction> list = new List<AIAction>();
        DuelPlayer me = game.Current;
        DuelPlayer enemy = game.Opponent;

        // 1) Ascensões.
        for (int slot = 0; slot < DuelPlayer.FieldSlots; slot++)
        {
            if (!game.CanAscendSlot(slot))
            {
                continue;
            }

            CreatureInstance creature = me.Field[slot];
            CardData target = creature.Card.AscensionTarget;

            float gain = CardValue(target) - CreatureValue(creature);
            float healed = Mathf.Max(0, creature.Card.Health - creature.Health);
            float score = (gain + healed * 0.5f) * w.Ascend + 1f;

            AIAction action = new AIAction();
            action.Type = ActionType.Ascend;
            action.Slot = slot;
            action.Score = score;
            list.Add(action);
        }

        // 2) Jogar cartas da mão.
        foreach (CardData card in me.Hand)
        {
            if (!game.CanPlayCard(card))
            {
                continue;
            }

            AIAction action = new AIAction();
            action.Type = ActionType.Play;
            action.Card = card;
            action.Score = ScorePlay(game, card, w);
            list.Add(action);
        }

        // 3) Ataques.
        for (int slot = 0; slot < DuelPlayer.FieldSlots; slot++)
        {
            if (!game.CanAttackWith(slot))
            {
                continue;
            }

            CreatureInstance attacker = me.Field[slot];
            if (attacker.Attack <= 0)
            {
                continue;
            }

            foreach (int targetSlot in game.GetLegalTargets(slot))
            {
                float score;

                if (targetSlot == -1)
                {
                    score = attacker.Attack * w.Face * 1.2f;
                    if (attacker.Attack >= enemy.Essence)
                    {
                        score += 100f; // golpe final
                    }
                }
                else
                {
                    CreatureInstance defender = enemy.Field[targetSlot];
                    int dealt = defender.IsGuarding ? Mathf.Max(0, attacker.Attack - DuelGame.GuardReduction) : attacker.Attack;
                    bool kills = dealt >= defender.Health;
                    bool dies = defender.Attack >= attacker.Health;

                    score = kills ? CreatureValue(defender) * w.Kill : attacker.Attack * 0.3f;

                    if (dies)
                    {
                        score -= CreatureValue(attacker) * w.Safety;
                    }

                    if (dies && !kills)
                    {
                        score -= 20f; // não faz trocas suicidas
                    }
                }

                AIAction action = new AIAction();
                action.Type = ActionType.Attack;
                action.Slot = slot;
                action.Target = targetSlot;
                action.Score = score;
                list.Add(action);
            }
        }

        return list;
    }

    private static float ScorePlay(DuelGame game, CardData card, Weights w)
    {
        DuelPlayer me = game.Current;

        if (card.Type == CardType.Ser)
        {
            float score = CardValue(card) * w.Develop + card.EtherCost * 0.3f;

            if (card.HasProvoke && me.Essence <= 10)
            {
                score += 1.5f; // precisa se proteger
            }

            return score;
        }

        if (card.Type == CardType.Vigilia)
        {
            return (2.5f + card.EtherCost * 0.4f) * w.Vigil;
        }

        return ScoreRite(game, card, w);
    }

    // A nota de um Rito é a soma do valor dos seus efeitos "ao entrar".
    private static float ScoreRite(DuelGame game, CardData card, Weights w)
    {
        DuelPlayer me = game.Current;
        DuelPlayer enemy = game.Opponent;

        float total = 0f;
        bool useful = false;

        foreach (CardEffect effect in card.Effects)
        {
            if (effect == null || effect.Trigger != EffectTrigger.AoEntrar)
            {
                continue;
            }

            HealOwnerEffect heal = effect as HealOwnerEffect;
            if (heal != null)
            {
                int real = Mathf.Min(heal.Amount, DuelPlayer.StartEssence - me.Essence);
                if (real > 0)
                {
                    total += real * (me.Essence <= 10 ? 1.5f : 1f);
                    useful = true;
                }
                continue;
            }

            DrawCardsEffect draw = effect as DrawCardsEffect;
            if (draw != null)
            {
                if (me.Hand.Count < DuelPlayer.MaxHandSize - 1)
                {
                    total += draw.Amount * 2f;
                    useful = true;
                }
                continue;
            }

            DamageOpponentEffect damage = effect as DamageOpponentEffect;
            if (damage != null)
            {
                total += damage.Amount * w.Face;
                if (damage.Amount >= enemy.Essence)
                {
                    total += 100f;
                }
                useful = true;
                continue;
            }

            DamageStrongestEnemyEffect hit = effect as DamageStrongestEnemyEffect;
            if (hit != null)
            {
                CreatureInstance hitTarget = StrongestEnemy(enemy, true, -1);
                if (hitTarget != null)
                {
                    bool kills = hit.Amount >= hitTarget.Health;
                    total += kills ? CreatureValue(hitTarget) * w.Kill : hit.Amount * 0.4f;
                    useful = true;
                }
                continue;
            }

            DestroyStrongestEnemyEffect destroy = effect as DestroyStrongestEnemyEffect;
            if (destroy != null)
            {
                CreatureInstance destroyTarget = StrongestEnemy(enemy, true, destroy.MaxCost);
                if (destroyTarget != null)
                {
                    total += CreatureValue(destroyTarget) * w.Kill * 1.1f;
                    useful = true;
                }
                continue;
            }

            DrainEtherEffect drain = effect as DrainEtherEffect;
            if (drain != null)
            {
                total += drain.Amount * 1.5f;
                useful = true;
            }
        }

        if (!useful)
        {
            return -100f; // Rito sem efeito útil agora: não vale jogar
        }

        return total * w.Rite;
    }

    // ---------- Valores e ferramentas ----------

    private static float CardValue(CardData card)
    {
        float value = card.Attack * 1.0f + card.Health * 0.8f;

        if (card.HasProvoke)
        {
            value += 1.5f;
        }

        if (card.HasHaste)
        {
            value += 1.0f;
        }

        if (card.CanAscend)
        {
            value += 1.0f;
        }

        value += card.Effects.Count * 1.0f;
        return value;
    }

    private static float CreatureValue(CreatureInstance creature)
    {
        float value = creature.Attack * 1.0f + creature.Health * 0.8f;

        if (creature.Card.HasProvoke)
        {
            value += 1.5f;
        }

        if (creature.Card.CanAscend)
        {
            value += 1.0f;
        }

        return value;
    }

    // O Ser inimigo com maior Ataque (mesma regra do DuelGame para efeitos com alvo).
    private static CreatureInstance StrongestEnemy(DuelPlayer enemy, bool respectShield, int maxCost)
    {
        CreatureInstance best = null;

        for (int i = 0; i < DuelPlayer.FieldSlots; i++)
        {
            CreatureInstance creature = enemy.Field[i];
            if (creature == null)
            {
                continue;
            }

            if (respectShield && IsRiteShielded(creature.Card))
            {
                continue;
            }

            if (maxCost >= 0 && creature.Card.EtherCost > maxCost)
            {
                continue;
            }

            if (best == null || creature.Attack > best.Attack)
            {
                best = creature;
            }
        }

        return best;
    }

    private static bool IsRiteShielded(CardData card)
    {
        foreach (CardEffect effect in card.Effects)
        {
            if (effect is RiteShieldEffect)
            {
                return true;
            }
        }

        return false;
    }

    private static bool Execute(DuelGame game, AIAction action)
    {
        switch (action.Type)
        {
            case ActionType.Play:
                return game.PlayCard(action.Card);
            case ActionType.Ascend:
                return game.Ascend(action.Slot);
            case ActionType.Attack:
                return game.Attack(action.Slot, action.Target);
        }

        return false;
    }
}