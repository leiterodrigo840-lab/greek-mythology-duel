using System.Collections.Generic;

// IA bem simples, só para testar o duelo sozinho.
// (A IA de verdade, com perfis e dificuldade, vem em outra etapa.)
public static class SimpleAutoPlayer
{
    public static void PlayTurn(DuelGame game)
    {
        DuelPlayer me = game.Current;

        // 1) Ascensão sempre que possível.
        for (int slot = 0; slot < DuelPlayer.FieldSlots; slot++)
        {
            if (game.CanAscendSlot(slot))
            {
                game.Ascend(slot);
            }
        }

        // 2) Joga cartas, da mais cara para a mais barata, enquanto couber.
        bool played = true;
        while (played && !game.IsOver)
        {
            played = false;
            CardData best = null;

            foreach (CardData card in me.Hand)
            {
                if (!game.CanPlayCard(card) || !IsUseful(game, card))
                {
                    continue;
                }

                if (best == null || card.EtherCost > best.EtherCost)
                {
                    best = card;
                }
            }

            if (best != null)
            {
                game.PlayCard(best);
                played = true;
            }
        }

        // 3) Ataca com o que puder.
        for (int slot = 0; slot < DuelPlayer.FieldSlots; slot++)
        {
            if (game.IsOver)
            {
                return;
            }

            if (!game.CanAttackWith(slot))
            {
                continue;
            }

            CreatureInstance attacker = me.Field[slot];
            if (attacker.Attack <= 0)
            {
                continue;
            }

            int target = ChooseTarget(game, attacker);
            if (target != -2)
            {
                game.Attack(slot, target);
            }
        }
    }

    // Decide se vale a pena jogar a carta agora.
    private static bool IsUseful(DuelGame game, CardData card)
    {
        if (card.Type == CardType.Ser)
        {
            return true;
        }

        DuelPlayer me = game.Current;

        if (card.Type == CardType.Vigilia)
        {
            foreach (CardEffect effect in card.Effects)
            {
                if (effect != null
                    && (effect.Trigger == EffectTrigger.AoInimigoAtacar || effect.Trigger == EffectTrigger.AoSeuSerDestruido))
                {
                    return true;
                }
            }
            return false;
        }

        // Rito: só joga se tiver efeito, e se o efeito fizer sentido agora.
        bool hasEffect = false;
        foreach (CardEffect effect in card.Effects)
        {
            if (effect == null || effect.Trigger != EffectTrigger.AoEntrar)
            {
                continue;
            }

            hasEffect = true;

            bool needsEnemyCreature = effect is DamageStrongestEnemyEffect || effect is DestroyStrongestEnemyEffect;
            if (needsEnemyCreature && game.Opponent.CreatureCount() == 0)
            {
                return false;
            }

            if (effect is HealOwnerEffect && me.Essence > DuelPlayer.StartEssence - 2)
            {
                return false;
            }
        }

        return hasEffect;
    }

    // Devolve o alvo escolhido, -1 para a Essência, ou -2 para "não atacar".
    private static int ChooseTarget(DuelGame game, CreatureInstance attacker)
    {
        List<int> legal = game.GetLegalTargets();

        if (legal.Count == 1 && legal[0] == -1)
        {
            return -1;
        }

        int bestTarget = -2;
        int bestScore = int.MinValue;

        foreach (int slot in legal)
        {
            CreatureInstance defender = game.Opponent.Field[slot];
            bool kills = attacker.Attack >= defender.Health;
            bool dies = defender.Attack >= attacker.Health;

            if (dies && !kills)
            {
                continue;
            }

            int score = (kills ? defender.Card.EtherCost + 2 : 0) - (dies ? attacker.Card.EtherCost + 2 : 0);
            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = slot;
            }
        }

        return bestTarget;
    }
}