using System.Collections.Generic;
using UnityEngine;

// As regras do duelo. Não usa tela nem MonoBehaviour: só dados e regras.
public class DuelGame
{
    public const int MaxTurns = 60;
    public const int GuardReduction = 2;

    public DuelPlayer[] Players { get; private set; }
    public int CurrentIndex { get; private set; }
    public int TurnNumber { get; private set; }
    public bool IsOver { get; private set; }
    public DuelPlayer Winner { get; private set; }
    public List<string> LogLines { get; private set; }

    // Inteligência da IA que joga contra o jogador (definida pela campanha).
    public AIProfile AiProfile { get; set; } = AIProfile.Equilibrado;
    public AIDifficulty AiDifficulty { get; set; } = AIDifficulty.Normal;

    public event System.Action<string> LogAdded;

    private bool attackNegated;
    private CardData lastDestroyedCard;

    public DuelPlayer Current => Players[CurrentIndex];
    public DuelPlayer Opponent => Players[1 - CurrentIndex];

    public DuelGame(string nameA, DeckData deckA, string nameB, DeckData deckB)
    {
        Players = new DuelPlayer[]
        {
            new DuelPlayer(nameA, deckA),
            new DuelPlayer(nameB, deckB)
        };
        LogLines = new List<string>();
        CurrentIndex = 0;
        TurnNumber = 0;
        IsOver = false;
        Winner = null;
    }

    public void Log(string message)
    {
        LogLines.Add(message);
        LogAdded?.Invoke(message);
    }

    public DuelPlayer OpponentOf(DuelPlayer player)
    {
        return player == Players[0] ? Players[1] : Players[0];
    }

    // ---------- Início do duelo e dos turnos ----------

    public void StartDuel()
    {
        Log("=== O duelo começa! ===");

        foreach (DuelPlayer player in Players)
        {
            for (int i = 0; i < DuelPlayer.StartHandSize; i++)
            {
                if (player.Deck.Count > 0)
                {
                    player.Hand.Add(player.Deck[0]);
                    player.Deck.RemoveAt(0);
                }
            }
            Log(player.Name + " começa com " + player.Hand.Count + " cartas na mão.");
        }

        CurrentIndex = 0;
        BeginTurn();
    }

    private void BeginTurn()
    {
        TurnNumber++;
        DuelPlayer player = Current;

        if (player.MaxEther < DuelPlayer.MaxEtherCap)
        {
            player.MaxEther++;
        }

        player.Ether = Mathf.Max(0, player.MaxEther - player.EtherPenalty);
        if (player.EtherPenalty > 0)
        {
            Log(player.Name + " começa o turno com " + player.EtherPenalty + " de Éter a menos.");
            player.EtherPenalty = 0;
        }

        for (int i = 0; i < DuelPlayer.FieldSlots; i++)
        {
            if (player.Field[i] != null)
            {
                player.Field[i].CanAttack = true;
                player.Field[i].AttackedThisTurn = false;
            }
        }

        Log("--- Turno " + TurnNumber + ": " + player.Name + " | Essência " + player.Essence + " | Éter " + player.Ether + " ---");
        DrawCards(player, 1);
    }

    public void EndTurn()
    {
        if (IsOver)
        {
            return;
        }

        DuelPlayer player = Current;

        for (int i = 0; i < DuelPlayer.FieldSlots; i++)
        {
            if (IsOver)
            {
                return;
            }

            CreatureInstance creature = player.Field[i];
            if (creature != null)
            {
                RunEffects(player, creature.Card, EffectTrigger.FimDoTurno);
            }
        }

        if (IsOver)
        {
            return;
        }

        if (TurnNumber >= MaxTurns)
        {
            IsOver = true;
            Winner = null;
            Log("Limite de " + MaxTurns + " turnos atingido: empate.");
            return;
        }

        CurrentIndex = 1 - CurrentIndex;
        BeginTurn();
    }

    // ---------- Cartas e Essência ----------

    public void DrawCards(DuelPlayer player, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (IsOver)
            {
                return;
            }

            if (player.Deck.Count == 0)
            {
                Log(player.Name + " não tem mais cartas no baralho e perde 1 de Essência.");
                DamagePlayer(player, 1);
                continue;
            }

            CardData card = player.Deck[0];
            player.Deck.RemoveAt(0);

            if (player.Hand.Count >= DuelPlayer.MaxHandSize)
            {
                player.Discard.Add(card);
                Log(player.Name + " está com a mão cheia e descarta " + card.ShownName + ".");
            }
            else
            {
                player.Hand.Add(card);
                Log(player.Name + " compra uma carta.");
            }
        }
    }

    public void HealPlayer(DuelPlayer player, int amount)
    {
        int before = player.Essence;
        player.Essence = Mathf.Min(DuelPlayer.StartEssence, player.Essence + amount);
        Log(player.Name + " cura " + (player.Essence - before) + " (Essência " + player.Essence + ").");
    }

    public void DamagePlayer(DuelPlayer player, int amount)
    {
        player.Essence -= amount;
        Log(player.Name + " sofre " + amount + " de dano (Essência " + player.Essence + ").");
        CheckWinner();
    }

    private void CheckWinner()
    {
        bool firstDead = Players[0].Essence <= 0;
        bool secondDead = Players[1].Essence <= 0;

        if (!firstDead && !secondDead)
        {
            return;
        }

        IsOver = true;

        if (firstDead && secondDead)
        {
            Winner = null;
            Log("=== Empate! ===");
        }
        else
        {
            Winner = firstDead ? Players[1] : Players[0];
            Log("=== " + Winner.Name + " venceu o duelo! ===");
        }
    }

    // ---------- Jogar cartas ----------

    public bool CanPlayCard(CardData card)
    {
        if (IsOver || card == null)
        {
            return false;
        }

        DuelPlayer player = Current;

        if (!player.Hand.Contains(card) || player.Ether < card.EtherCost)
        {
            return false;
        }

        if (card.Type == CardType.Ser)
        {
            return player.FirstFreeSlot() >= 0;
        }

        if (card.Type == CardType.Vigilia)
        {
            return player.Vigils.Count < DuelPlayer.MaxVigils;
        }

        return true; // Rito
    }

    public bool PlayCard(CardData card)
    {
        if (!CanPlayCard(card))
        {
            return false;
        }

        DuelPlayer player = Current;
        player.Ether -= card.EtherCost;
        player.Hand.Remove(card);

        if (card.Type == CardType.Ser)
        {
            int slot = player.FirstFreeSlot();
            CreatureInstance creature = new CreatureInstance(card, player);
            creature.CanAttack = card.HasHaste;
            player.Field[slot] = creature;
            Log(player.Name + " invoca " + card.ShownName + " (" + card.Attack + "/" + card.Health + ") na zona " + (slot + 1) + ".");
            RunEffects(player, card, EffectTrigger.AoEntrar);
        }
        else if (card.Type == CardType.Vigilia)
        {
            player.Vigils.Add(card);
            Log(player.Name + " coloca uma Vigília virada para baixo.");
        }
        else
        {
            Log(player.Name + " usa o Rito " + card.ShownName + ".");
            RunEffects(player, card, EffectTrigger.AoEntrar);
            player.Discard.Add(card);
        }

        return true;
    }

    // ---------- Ascensão Mítica (SHIFT) ----------

    private bool IsValidSlot(int slot)
    {
        return slot >= 0 && slot < DuelPlayer.FieldSlots;
    }

    public bool CanAscendSlot(int slot)
    {
        if (IsOver || !IsValidSlot(slot))
        {
            return false;
        }

        CreatureInstance creature = Current.Field[slot];
        return creature != null
            && creature.Card.CanAscend
            && Current.Ether >= creature.Card.AscensionCost;
    }

    public bool Ascend(int slot)
    {
        if (!CanAscendSlot(slot))
        {
            return false;
        }

        DuelPlayer player = Current;
        CreatureInstance old = player.Field[slot];
        CardData target = old.Card.AscensionTarget;

        player.Ether -= old.Card.AscensionCost;

        CreatureInstance evolved = new CreatureInstance(target, player);
        evolved.CanAttack = old.CanAttack;
        evolved.AttackedThisTurn = old.AttackedThisTurn;
        evolved.IsGuarding = old.IsGuarding;
        player.Field[slot] = evolved;
        player.Ascensions++;

        Log("ASCENSÃO! " + player.Name + ": " + old.Card.ShownName + " se transforma em " + target.ShownName + " (" + target.Attack + "/" + target.Health + ").");

        // A forma nova dispara os efeitos "ao entrar" (a revelação).
        RunEffects(player, target, EffectTrigger.AoEntrar);
        return true;
    }

    // ---------- Postura de Guarda ----------

    // Só dá para entrar em guarda se o Ser ainda não atacou neste turno. Sair é sempre permitido.
    public bool CanToggleGuard(int slot)
    {
        if (IsOver || !IsValidSlot(slot))
        {
            return false;
        }

        CreatureInstance creature = Current.Field[slot];
        if (creature == null)
        {
            return false;
        }

        return creature.IsGuarding || !creature.AttackedThisTurn;
    }

    public bool ToggleGuard(int slot)
    {
        if (!CanToggleGuard(slot))
        {
            return false;
        }

        CreatureInstance creature = Current.Field[slot];
        creature.IsGuarding = !creature.IsGuarding;

        if (creature.IsGuarding)
        {
            Log(creature.Card.ShownName + " assume a Postura de Guarda (recebe " + GuardReduction + " de dano a menos em ataques, mas não ataca).");
        }
        else
        {
            Log(creature.Card.ShownName + " sai da Postura de Guarda.");
        }

        return true;
    }

    // ---------- Combate ----------

    public bool CanAttackWith(int slot)
    {
        if (IsOver || !IsValidSlot(slot))
        {
            return false;
        }

        CreatureInstance creature = Current.Field[slot];
        return creature != null && creature.CanAttack && !creature.IsGuarding;
    }

    // Versão antiga: alvos de um atacante comum (sem Voo).
    public List<int> GetLegalTargets()
    {
        return BuildTargets(false);
    }

    // Versão nova: leva em conta se o atacante tem Voo.
    public List<int> GetLegalTargets(int attackerSlot)
    {
        bool flying = false;

        if (IsValidSlot(attackerSlot))
        {
            CreatureInstance attacker = Current.Field[attackerSlot];
            flying = attacker != null && attacker.Card.HasFlying;
        }

        return BuildTargets(flying);
    }

    // Alvos legais: números de zonas do oponente, ou -1 para a Essência do oponente.
    private List<int> BuildTargets(bool flying)
    {
        List<int> targets = new List<int>();
        DuelPlayer opponent = Opponent;

        if (flying)
        {
            // Seres com Voo inimigos devem ser atacados primeiro.
            for (int i = 0; i < DuelPlayer.FieldSlots; i++)
            {
                CreatureInstance creature = opponent.Field[i];
                if (creature != null && creature.Card.HasFlying)
                {
                    targets.Add(i);
                }
            }

            if (targets.Count > 0)
            {
                return targets;
            }

            // Sem Voo inimigo: ignora Provocar e pode atacar qualquer Ser ou a Essência.
            for (int i = 0; i < DuelPlayer.FieldSlots; i++)
            {
                if (opponent.Field[i] != null)
                {
                    targets.Add(i);
                }
            }

            targets.Add(-1);
            return targets;
        }

        // Atacante comum: Provocar primeiro.
        for (int i = 0; i < DuelPlayer.FieldSlots; i++)
        {
            CreatureInstance creature = opponent.Field[i];
            if (creature != null && creature.Card.HasProvoke)
            {
                targets.Add(i);
            }
        }

        if (targets.Count > 0)
        {
            return targets;
        }

        // Depois, qualquer Ser inimigo.
        for (int i = 0; i < DuelPlayer.FieldSlots; i++)
        {
            if (opponent.Field[i] != null)
            {
                targets.Add(i);
            }
        }

        // Sem Seres inimigos: ataca a Essência.
        if (targets.Count == 0)
        {
            targets.Add(-1);
        }

        return targets;
    }

    public bool Attack(int attackerSlot, int targetSlot)
    {
        if (!CanAttackWith(attackerSlot))
        {
            return false;
        }

        if (!GetLegalTargets(attackerSlot).Contains(targetSlot))
        {
            return false;
        }

        DuelPlayer owner = Current;
        DuelPlayer opponent = Opponent;
        CreatureInstance attacker = owner.Field[attackerSlot];
        attacker.CanAttack = false;
        attacker.AttackedThisTurn = true;

        // Vigílias do defensor podem reagir ao ataque.
        attackNegated = false;
        TriggerVigils(opponent, EffectTrigger.AoInimigoAtacar);

        if (IsOver)
        {
            return true;
        }

        if (attackNegated)
        {
            Log("O ataque de " + attacker.Card.ShownName + " foi anulado.");
            return true;
        }

        if (targetSlot == -1)
        {
            if (attacker.Card.HasFlying)
            {
                Log(attacker.Card.ShownName + " voa sobre as defesas e ataca " + opponent.Name + " diretamente.");
            }
            else
            {
                Log(attacker.Card.ShownName + " ataca " + opponent.Name + " diretamente.");
            }

            DamagePlayer(opponent, attacker.Attack);
            return true;
        }

        CreatureInstance defender = opponent.Field[targetSlot];
        if (defender == null)
        {
            return true;
        }

        int damageToDefender = attacker.Attack;
        string guardNote = "";

        if (defender.IsGuarding)
        {
            damageToDefender = Mathf.Max(0, damageToDefender - GuardReduction);
            guardNote = " [Postura de Guarda: dano reduzido para " + damageToDefender + "]";
        }

        Log(attacker.Card.ShownName + " (" + attacker.Attack + "/" + attacker.Health + ") ataca "
            + defender.Card.ShownName + " (" + defender.Attack + "/" + defender.Health + ")." + guardNote);

        defender.Health -= damageToDefender;
        attacker.Health -= defender.Attack;

        if (defender.Health <= 0)
        {
            DestroyCreature(opponent, targetSlot);
        }

        if (attacker.Health <= 0)
        {
            DestroyCreature(owner, attackerSlot);
        }

        return true;
    }

    private void DestroyCreature(DuelPlayer owner, int slot)
    {
        CreatureInstance creature = owner.Field[slot];
        if (creature == null)
        {
            return;
        }

        owner.Field[slot] = null;
        owner.Discard.Add(creature.Card);
        lastDestroyedCard = creature.Card;
        Log(creature.Card.ShownName + " de " + owner.Name + " é destruído.");

        RunEffects(owner, creature.Card, EffectTrigger.AoSerDestruido);
        TriggerVigils(owner, EffectTrigger.AoSeuSerDestruido);
    }

    // ---------- Efeitos com alvo automático ----------

    public void DamageCreature(DuelPlayer owner, int slot, int amount)
    {
        CreatureInstance creature = owner.Field[slot];
        if (creature == null)
        {
            return;
        }

        creature.Health -= amount;
        Log(creature.Card.ShownName + " de " + owner.Name + " sofre " + amount + " de dano (Vida " + creature.Health + ").");

        if (creature.Health <= 0)
        {
            DestroyCreature(owner, slot);
        }
    }

    private bool IsRiteShielded(CardData card)
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

    // Procura o Ser com maior Ataque. maxCost < 0 significa "sem limite de custo".
    private int FindStrongestSlot(DuelPlayer enemy, bool respectRiteShield, int maxCost)
    {
        int bestSlot = -1;
        int bestAttack = -1;

        for (int i = 0; i < DuelPlayer.FieldSlots; i++)
        {
            CreatureInstance creature = enemy.Field[i];
            if (creature == null)
            {
                continue;
            }

            if (respectRiteShield && IsRiteShielded(creature.Card))
            {
                continue;
            }

            if (maxCost >= 0 && creature.Card.EtherCost > maxCost)
            {
                continue;
            }

            if (creature.Attack > bestAttack)
            {
                bestAttack = creature.Attack;
                bestSlot = i;
            }
        }

        return bestSlot;
    }

    public void DamageStrongestEnemyCreature(DuelPlayer owner, bool fromRite, int amount)
    {
        DuelPlayer enemy = OpponentOf(owner);
        int slot = FindStrongestSlot(enemy, fromRite, -1);

        if (slot < 0)
        {
            Log("O efeito não encontrou um alvo válido.");
            return;
        }

        DamageCreature(enemy, slot, amount);
    }

    public void DestroyStrongestEnemyCreature(DuelPlayer owner, bool fromRite, int maxCost)
    {
        DuelPlayer enemy = OpponentOf(owner);
        int slot = FindStrongestSlot(enemy, fromRite, maxCost);

        if (slot < 0)
        {
            Log("O efeito não encontrou um alvo válido.");
            return;
        }

        DestroyCreature(enemy, slot);
    }

    public void SummonCreatures(DuelPlayer owner, CardData card, int count)
    {
        for (int i = 0; i < count; i++)
        {
            int slot = owner.FirstFreeSlot();
            if (slot < 0)
            {
                Log("Não há espaço no campo para invocar " + card.ShownName + ".");
                return;
            }

            owner.Field[slot] = new CreatureInstance(card, owner);
            Log(owner.Name + " invoca " + card.ShownName + " na zona " + (slot + 1) + ".");
        }
    }

    public void DrainEther(DuelPlayer target, int amount)
    {
        target.EtherPenalty += amount;
        Log(target.Name + " perderá " + amount + " de Éter no próximo turno.");
    }

    public void NegateAttack()
    {
        attackNegated = true;
    }

    public void ReturnLastDestroyedToHand(DuelPlayer owner)
    {
        if (lastDestroyedCard == null)
        {
            return;
        }

        if (owner.Hand.Count >= DuelPlayer.MaxHandSize)
        {
            Log("A mão de " + owner.Name + " está cheia: " + lastDestroyedCard.ShownName + " não volta.");
        }
        else if (owner.Discard.Remove(lastDestroyedCard))
        {
            owner.Hand.Add(lastDestroyedCard);
            Log(lastDestroyedCard.ShownName + " volta para a mão de " + owner.Name + ".");
        }

        lastDestroyedCard = null;
    }

    // ---------- Efeitos e Vigílias ----------

    private bool HasEffectWithTrigger(CardData card, EffectTrigger trigger)
    {
        foreach (CardEffect effect in card.Effects)
        {
            if (effect != null && effect.Trigger == trigger)
            {
                return true;
            }
        }
        return false;
    }

    // Revela e ativa a primeira Vigília do dono que reage a este gatilho.
    private void TriggerVigils(DuelPlayer owner, EffectTrigger trigger)
    {
        for (int i = 0; i < owner.Vigils.Count; i++)
        {
            if (IsOver)
            {
                return;
            }

            CardData vigil = owner.Vigils[i];
            if (!HasEffectWithTrigger(vigil, trigger))
            {
                continue;
            }

            owner.Vigils.RemoveAt(i);
            owner.Discard.Add(vigil);
            Log("VIGÍLIA REVELADA! " + owner.Name + " ativa " + vigil.ShownName + ".");
            RunEffects(owner, vigil, trigger);
            return; // só uma Vigília por evento
        }
    }

    private void RunEffects(DuelPlayer owner, CardData card, EffectTrigger trigger)
    {
        DuelEffectContext context = new DuelEffectContext(this, owner, OpponentOf(owner), card);

        foreach (CardEffect effect in card.Effects)
        {
            if (IsOver)
            {
                return;
            }

            if (effect == null || effect.Trigger != trigger)
            {
                continue;
            }

            effect.Execute(context);
        }
    }
}