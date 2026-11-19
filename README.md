# Duel of Greek Mythology

Duel of Greek Mythology is a strategic card and duel game inspired by Greek mythology, developed in Unity 6 (URP) for browser play (WebGL) with gameplay inspired by Yu-Gi-Oh! Duel Links adapted to the theme.

---

## Play Online

- itch.io: https://rodrigo255.itch.io/greek-mythology-duel

---

## How to Play and Rules

The game follows fast-paced card duel mechanics in the style of Yu-Gi-Oh! Duel Links, featuring thematic adaptations and mechanics based on Greek mythology:

- Turn-Based Duels: Players take turns summoning cards, activating effects or abilities, and attacking the opponent.
- Duel Links Style: Dynamic card duel matches with life points or resource management, battle positions, and effect or support cards adapted to the Greek mythology theme.
- Win Condition: Reduce the opponent life points to zero (or meet the match victory condition) before your opponent wins.
- Controls: Use the left mouse button to select cards, choose targets, confirm actions, and navigate duel and campaign menus.

---

## Technologies Used

- Engine: Unity 6 (Universal Render Pipeline - URP)
- Language: C#
- Platform: WebGL (Modern web browsers)
- Game Hosting: itch.io
- Source Code: https://github.com/leiterodrigo840-lab/greek-mythology-duel

---

## Project Structure

- Assets/ : C# scripts, scenes, prefabs, artwork, and visual setups.
- Packages/ : Unity project dependencies and packages.
- ProjectSettings/ : General project and WebGL platform settings.
- Main scenes: Boot, Campanha, DuelUI, DuelTest, CardTest, BalanceTest.

---

## Running Locally (WebGL)

Due to browser security policies (CORS), WebGL builds should not be opened directly via file:/// in the browser. To run locally:

1. In Unity 6, open the build window and click Build And Run (starts a local server automatically); or
2. Serve the build folder contents (index.html, Build/, and TemplateData/) through a local HTTP server (http://localhost or http://127.0.0.1).

---

## Author

- Rodrigo Leite - Game Development
- itch.io: https://rodrigo255.itch.io/greek-mythology-duel
- Repository: https://github.com/leiterodrigo840-lab/greek-mythology-duel
