using System.IO;
using UnityEditor;
using UnityEngine;

// Cria as 18 cartas base de uma vez. Cria apenas as que ainda não existem.
public static class CardBatchCreator
{
    private static int createdCount;

    [MenuItem("Duelo/Criar cartas base")]
    public static void CreateBaseCards()
    {
        createdCount = 0;

        // Usa a pasta de qualquer carta que já exista (Card_Nyx, Card_Centelha etc.).
        string[] guids = AssetDatabase.FindAssets("t:CardData");
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("Cartas base", "Não encontrei nenhuma carta no projeto. Crie uma carta antes (Create > Duelo > Carta).", "OK");
            return;
        }

        string firstPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        string folder = Path.GetDirectoryName(firstPath).Replace('\\', '/');

        CardData mudaGaia = Make(folder, "Card_MudaDeGaia", "Muda de Gaia", CardType.Ser, Faction.Titanico, Rarity.Comum, 1, 0, 3,
            "Provocar. Ascensão (3 Éter): vira Gaia, Mãe Terra.");
        CardData gaia = Make(folder, "Card_Gaia", "Gaia, Mãe Terra", CardType.Ser, Faction.Titanico, Rarity.Rara, 4, 3, 6,
            "No fim do seu turno, cure 1 Essência.");
        Make(folder, "Card_TitaAprendiz", "Titã Aprendiz", CardType.Ser, Faction.Titanico, Rarity.Comum, 2, 2, 3,
            "Sem efeito.");
        CardData titaColosso = Make(folder, "Card_TitaColosso", "Titã Colosso", CardType.Ser, Faction.Titanico, Rarity.Rara, 4, 5, 4,
            "Ascensão (3 Éter): vira Titã da Tempestade.");
        CardData titaTempestade = Make(folder, "Card_TitaTempestade", "Titã da Tempestade", CardType.Ser, Faction.Titanico, Rarity.Epica, 6, 7, 5,
            "Ao entrar: 2 de dano ao Ser inimigo mais forte.");
        CardData filhoteHidra = Make(folder, "Card_FilhoteHidra", "Filhote de Hidra", CardType.Ser, Faction.Monstruoso, Rarity.Comum, 2, 2, 2,
            "Ascensão (3 Éter): vira Hidra de Lerna.");
        CardData hidra = Make(folder, "Card_HidraLerna", "Hidra de Lerna", CardType.Ser, Faction.Monstruoso, Rarity.Epica, 5, 4, 6,
            "Ao ser destruída, invoca 2 Filhotes de Hidra.");
        Make(folder, "Card_SatiroFoliao", "Sátiro Folião", CardType.Ser, Faction.Olimpico, Rarity.Comum, 1, 1, 2,
            "Ao entrar: compre 1 carta.");
        Make(folder, "Card_NinfaRio", "Ninfa do Rio", CardType.Ser, Faction.Olimpico, Rarity.Comum, 2, 1, 4,
            "No fim do seu turno, cure 1 Essência.");
        CardData jovemHeroi = Make(folder, "Card_JovemHeroi", "Jovem Herói", CardType.Ser, Faction.Heroico, Rarity.Comum, 2, 2, 2,
            "Ascensão (3 Éter): vira Herói Lendário.");
        CardData heroiLendario = Make(folder, "Card_HeroiLendario", "Herói Lendário", CardType.Ser, Faction.Heroico, Rarity.Epica, 5, 5, 5,
            "Não pode ser alvo de Ritos inimigos.");
        Make(folder, "Card_GuardiaoBronze", "Guardião de Bronze", CardType.Ser, Faction.Olimpico, Rarity.Comum, 3, 2, 5,
            "Provocar.");
        Make(folder, "Card_HarpiaVeloz", "Harpia Veloz", CardType.Ser, Faction.Monstruoso, Rarity.Comum, 2, 3, 1,
            "Pode atacar no turno em que entra.");
        Make(folder, "Card_RaioFragmentado", "Raio Fragmentado", CardType.Rito, Faction.Olimpico, Rarity.Comum, 2, 0, 0,
            "Causa 3 de dano a um Ser inimigo.");
        Make(folder, "Card_BencaoColheita", "Bênção da Colheita", CardType.Rito, Faction.Olimpico, Rarity.Comum, 1, 0, 0,
            "Cure 4 Essência.");
        Make(folder, "Card_FioCortado", "Fio Cortado", CardType.Rito, Faction.Destino, Rarity.Rara, 3, 0, 0,
            "Destrua um Ser inimigo com custo 3 ou menos.");
        Make(folder, "Card_ArmadilhaLabirinto", "Armadilha do Labirinto", CardType.Vigilia, Faction.Destino, Rarity.Comum, 2, 0, 0,
            "Quando um Ser inimigo atacar, o ataque é anulado.");
        Make(folder, "Card_EcoDestino", "Eco do Destino", CardType.Vigilia, Faction.Destino, Rarity.Rara, 3, 0, 0,
            "Quando um Ser seu for destruído, devolva-o à mão.");

        // Liga as Ascensões (a Centelha para a Nyx já foi feita na Etapa 2).
        Link(mudaGaia, gaia, 3);
        Link(titaColosso, titaTempestade, 3);
        Link(filhoteHidra, hidra, 3);
        Link(jovemHeroi, heroiLendario, 3);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string message = createdCount + " carta(s) nova(s) criada(s) em:\n" + folder;
        Debug.Log(message);
        EditorUtility.DisplayDialog("Cartas base", message, "OK");
    }

    private static CardData Make(string folder, string fileName, string displayName, CardType type,
        Faction faction, Rarity rarity, int cost, int attack, int health, string effectText)
    {
        string path = folder + "/" + fileName + ".asset";

        // Se a carta já existe, não mexe nela.
        CardData existing = AssetDatabase.LoadAssetAtPath<CardData>(path);
        if (existing != null)
        {
            return existing;
        }

        CardData card = ScriptableObject.CreateInstance<CardData>();
        card.EditorSetup(fileName, displayName, type, faction, rarity, cost, attack, health, effectText);
        AssetDatabase.CreateAsset(card, path);
        createdCount++;
        return card;
    }

    private static void Link(CardData from, CardData to, int cost)
    {
        from.EditorSetAscension(to, cost);
        EditorUtility.SetDirty(from);
    }
}