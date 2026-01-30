# Field Friends

A small, earnest, Game Boy / Game Boy Color-scale 2D monster-companion RPG set in **Bramble Vale**. Focused on walking, gentle encounters, and calm companionship.

## Project Structure

```
Assets/
  Scripts/
    Core/           Game manager, grid movement, camera controller
    Data/           Enums, creature/area definitions, type chart
    Battle/         Battle manager and UI
    World/          Area transitions, encounters, NPC dialogue
    Party/          Party management, creature instances, friendship
    UI/             Pause menu, friends list, map, dialogue box
    Save/           Save/load system
    Palette/        Color palette definitions
  Resources/
    Data/           ScriptableObject assets (creatures, areas, encounters)
  Scenes/           Unity scenes per area
  Sprites/          Pixel art assets
    Creatures/      16x16 and 24x24 creature sprites
    Tiles/          8x8 tileset art
    UI/             UI element sprites
    Characters/     Player and NPC sprites
  Audio/            Music and SFX
```

## Technical Specs

- **Tile size**: 8x8 pixels
- **Screen grid**: 20x18 tiles (Game Boy style)
- **Sprites**: 16x16 standard, 24x24 for large creatures (Bloomel, Still)
- **Palette**: 10-color pastel GBC palette (see `Assets/Sprites/FieldFriendsPastelGBC.gpl`)

## Core Systems

### Grid Movement
Discrete tile-to-tile walking. No fast travel. Walking is the point.

### Areas (7)
Willow End (home) > South Field / Creek Path > Hill Road > Stonebridge > North Meadow > Quiet Grove (end)

### Creatures (12)
Four types: Field, Wind, Water, Meadow. Field > Water > Wind > Field. Meadow neutral.

| Name     | Type   | HP | ATK | DEF | SPD | Ability        |
|----------|--------|----|-----|-----|-----|----------------|
| Mossbit  | Field  | 8  | 3   | 7   | 2   | Part the Grass |
| Bramblet | Field  | 6  | 4   | 5   | 3   | Snare Step     |
| Loamle   | Field  | 7  | 5   | 4   | 2   | Dig Cache      |
| Skirl    | Wind   | 4  | 5   | 2   | 8   | Hop Lift       |
| Flit     | Wind   | 5  | 3   | 3   | 7   | Scout Ahead    |
| Wispin   | Wind   | 3  | 6   | 2   | 9   | Gentle Gust    |
| Plen     | Water  | 6  | 4   | 4   | 4   | Stream Glide   |
| Ripplet  | Water  | 7  | 3   | 5   | 3   | Clear Pool     |
| Drift    | Water  | 4  | 5   | 3   | 7   | Slip Away      |
| Bloomel  | Meadow | 8  | 4   | 6   | 2   | Soft Light     |
| Petalyn  | Meadow | 5  | 4   | 5   | 5   | Calm Field     |
| Still    | Meadow | 9  | 2   | 8   | 1   | Wait           |

### Battle
Turn order by SPD. Three actions: MOVE (attack), WAIT (skip turn, builds friendship), BACK (flee). KO state is called "rest." Target duration: 4-6 turns.

### Friendship
Hidden counter that grows by walking together, not swapping, and choosing WAIT. Unlocks upgraded abilities at threshold (100).

### Save
Single slot. Autosave on area change. Manual save from pause menu. Save message: "You rest for a bit."

## Design Ethos

If a system starts explaining itself, remove it. Field Friends is allowed to be loved without interpretation.
