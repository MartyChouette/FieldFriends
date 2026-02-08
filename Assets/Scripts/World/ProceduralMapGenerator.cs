using UnityEngine;
using UnityEngine.Tilemaps;
using FieldFriends.Core;
using FieldFriends.Data;
using FieldFriends.Palette;

namespace FieldFriends.World
{
    /// <summary>
    /// Generates tile-based maps for all 7 areas at runtime.
    /// Each area is a 20x18 grid (one screen). Tiles are placed
    /// procedurally but follow a fixed layout per area for consistency.
    ///
    /// Call Generate(areaID) to build the tilemap for a given area.
    /// Clears and rebuilds the tilemap each time.
    /// </summary>
    public class ProceduralMapGenerator : MonoBehaviour
    {
        // Tile references (assigned at runtime from sprites)
        Tile _grass1, _grass2, _path, _tallGrass, _water1, _water2;
        Tile _tree, _trunk, _fence, _rocks, _flowers, _flowerDots;
        Tile _houseWall, _houseRoof, _houseWindow, _pondEdge;
        Tile _bridge, _stonePost, _stoneBank, _shallowWater, _deepWater;
        Tile _slopeDirt, _windGrass, _paleGrass, _roots, _canopy;
        Tile _flowerClusters, _reeds;

        Tilemap _groundMap;
        Tilemap _decorMap;
        Tilemap _collisionMap;
        Grid _grid;

        const int W = 20; // screen width in tiles
        const int H = 18; // screen height in tiles

        public Vector2Int PlayerSpawn { get; private set; }

        public void Initialize()
        {
            CreateTilemapHierarchy();
            LoadTiles();
        }

        void CreateTilemapHierarchy()
        {
            // Create grid
            var gridObj = new GameObject("Grid");
            _grid = gridObj.AddComponent<Grid>();
            _grid.cellSize = new Vector3(1, 1, 0);

            // Ground layer (walkable)
            var groundObj = new GameObject("Ground");
            groundObj.transform.SetParent(gridObj.transform);
            _groundMap = groundObj.AddComponent<Tilemap>();
            var groundRenderer = groundObj.AddComponent<TilemapRenderer>();
            groundRenderer.sortingOrder = 0;

            // Decoration layer (over ground)
            var decorObj = new GameObject("Decor");
            decorObj.transform.SetParent(gridObj.transform);
            _decorMap = decorObj.AddComponent<Tilemap>();
            var decorRenderer = decorObj.AddComponent<TilemapRenderer>();
            decorRenderer.sortingOrder = 1;

            // Collision layer (invisible, blocks movement)
            var collObj = new GameObject("Collision");
            collObj.transform.SetParent(gridObj.transform);
            _collisionMap = collObj.AddComponent<Tilemap>();
            var collRenderer = collObj.AddComponent<TilemapRenderer>();
            collRenderer.sortingOrder = -1;
            collRenderer.enabled = false; // invisible
            collObj.AddComponent<TilemapCollider2D>();
            collObj.layer = LayerMask.NameToLayer("Obstacles");
        }

        void LoadTiles()
        {
            // Create tiles from sprites loaded from Resources or generated
            _grass1 = CreateTileFromSprite("Tiles/grass_1");
            _grass2 = CreateTileFromSprite("Tiles/grass_2");
            _path = CreateTileFromSprite("Tiles/path_dirt");
            _tallGrass = CreateTileFromSprite("Tiles/tall_grass");
            _water1 = CreateTileFromSprite("Tiles/water_1");
            _water2 = CreateTileFromSprite("Tiles/water_2");
            _tree = CreateTileFromSprite("Tiles/tree_top");
            _trunk = CreateTileFromSprite("Tiles/tree_trunk");
            _fence = CreateTileFromSprite("Tiles/fence");
            _rocks = CreateTileFromSprite("Tiles/rocks");
            _flowers = CreateTileFromSprite("Tiles/flowers");
            _flowerDots = CreateTileFromSprite("Tiles/flower_dots");
            _houseWall = CreateTileFromSprite("Tiles/house_wall");
            _houseRoof = CreateTileFromSprite("Tiles/house_roof");
            _houseWindow = CreateTileFromSprite("Tiles/house_window");
            _pondEdge = CreateTileFromSprite("Tiles/pond_edge");
            _bridge = CreateTileFromSprite("Tiles/bridge");
            _stonePost = CreateTileFromSprite("Tiles/stone_post");
            _stoneBank = CreateTileFromSprite("Tiles/stone_bank");
            _shallowWater = CreateTileFromSprite("Tiles/shallow_water");
            _deepWater = CreateTileFromSprite("Tiles/deep_water");
            _slopeDirt = CreateTileFromSprite("Tiles/slope_dirt");
            _windGrass = CreateTileFromSprite("Tiles/wind_grass");
            _paleGrass = CreateTileFromSprite("Tiles/pale_grass");
            _roots = CreateTileFromSprite("Tiles/roots");
            _canopy = CreateTileFromSprite("Tiles/shadowed_canopy");
            _flowerClusters = CreateTileFromSprite("Tiles/flower_clusters");
            _reeds = CreateTileFromSprite("Tiles/reeds");
        }

        Tile CreateTileFromSprite(string path)
        {
            var sprite = Resources.Load<Sprite>(path);
            if (sprite == null)
            {
                // Create a colored placeholder tile
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.color = FieldFriendsPalette.PastelMint;
                return tile;
            }
            var t = ScriptableObject.CreateInstance<Tile>();
            t.sprite = sprite;
            return t;
        }

        public void Generate(AreaID area)
        {
            ClearAll();

            switch (area)
            {
                case AreaID.WillowEnd:    GenerateWillowEnd(); break;
                case AreaID.SouthField:   GenerateSouthField(); break;
                case AreaID.CreekPath:    GenerateCreekPath(); break;
                case AreaID.HillRoad:     GenerateHillRoad(); break;
                case AreaID.Stonebridge:  GenerateStonebridge(); break;
                case AreaID.NorthMeadow:  GenerateNorthMeadow(); break;
                case AreaID.QuietGrove:   GenerateQuietGrove(); break;
            }
        }

        void ClearAll()
        {
            if (_groundMap != null) _groundMap.ClearAllTiles();
            if (_decorMap != null) _decorMap.ClearAllTiles();
            if (_collisionMap != null) _collisionMap.ClearAllTiles();
        }

        // --- Helper methods ---

        void FillGround(Tile tile)
        {
            for (int x = 0; x < W; x++)
                for (int y = 0; y < H; y++)
                    _groundMap.SetTile(new Vector3Int(x, y, 0), tile);
        }

        void FillGrassVaried()
        {
            for (int x = 0; x < W; x++)
                for (int y = 0; y < H; y++)
                {
                    var tile = ((x + y) % 3 == 0) ? _grass2 : _grass1;
                    _groundMap.SetTile(new Vector3Int(x, y, 0), tile);
                }
        }

        void SetGround(int x, int y, Tile tile)
        {
            _groundMap.SetTile(new Vector3Int(x, y, 0), tile);
        }

        void SetDecor(int x, int y, Tile tile)
        {
            _decorMap.SetTile(new Vector3Int(x, y, 0), tile);
        }

        void SetCollision(int x, int y)
        {
            // Use any tile for collision (invisible)
            _collisionMap.SetTile(new Vector3Int(x, y, 0), _rocks);
        }

        void PlacePath(int x1, int y1, int x2, int y2)
        {
            // Horizontal then vertical path
            int cx = x1, cy = y1;
            while (cx != x2)
            {
                SetGround(cx, cy, _path);
                cx += (x2 > cx) ? 1 : -1;
            }
            while (cy != y2)
            {
                SetGround(cx, cy, _path);
                cy += (y2 > cy) ? 1 : -1;
            }
            SetGround(x2, y2, _path);
        }

        void PlaceTreeRow(int y, int startX, int endX, int spacing)
        {
            for (int x = startX; x <= endX; x += spacing)
            {
                SetDecor(x, y + 1, _tree);
                SetDecor(x, y, _trunk);
                SetCollision(x, y);
                SetCollision(x, y + 1);
            }
        }

        void PlaceBorderTrees()
        {
            // Top and bottom tree borders
            for (int x = 0; x < W; x += 3)
            {
                SetDecor(x, H - 1, _tree);
                SetCollision(x, H - 1);
                SetDecor(x, 0, _trunk);
                SetCollision(x, 0);
            }
        }

        // --- Area generators ---

        void GenerateWillowEnd()
        {
            FillGrassVaried();
            PlayerSpawn = new Vector2Int(10, 5);

            // Paths
            PlacePath(10, 1, 10, H - 2); // main vertical path
            PlacePath(1, 9, 19, 9);       // horizontal cross path

            // Player's house (top-left area)
            for (int x = 2; x <= 5; x++)
            {
                SetDecor(x, 14, _houseRoof);
                SetCollision(x, 14);
                SetDecor(x, 13, _houseWall);
                SetCollision(x, 13);
            }
            SetDecor(3, 13, _houseWindow);
            SetDecor(4, 12, _path); // doorstep

            // Pond (right side)
            for (int x = 14; x <= 17; x++)
                for (int y = 11; y <= 13; y++)
                {
                    SetGround(x, y, _pondEdge);
                    SetCollision(x, y);
                }
            // Pond NPC position: near (13, 12)

            // NPC house (center-right)
            for (int x = 14; x <= 17; x++)
            {
                SetDecor(x, 7, _houseRoof);
                SetCollision(x, 7);
                SetDecor(x, 6, _houseWall);
                SetCollision(x, 6);
            }
            SetDecor(15, 6, _houseWindow);

            // Fence around town
            for (int x = 0; x < W; x++)
            {
                if (x != 10) // leave gap for south exit
                {
                    SetDecor(x, 1, _fence);
                    SetCollision(x, 1);
                }
                if (x < 8 || x > 12) // leave gap for east exits
                {
                    SetDecor(x, H - 2, _fence);
                    SetCollision(x, H - 2);
                }
            }
            for (int y = 2; y < H - 2; y++)
            {
                if (y != 9) // leave gap for west exit
                {
                    SetDecor(0, y, _fence);
                    SetCollision(0, y);
                    SetDecor(W - 1, y, _fence);
                    SetCollision(W - 1, y);
                }
            }

            // Trees for decoration
            SetDecor(7, 12, _tree);
            SetDecor(7, 11, _trunk);
            SetCollision(7, 12);
            SetCollision(7, 11);
            SetDecor(12, 3, _tree);
            SetDecor(12, 2, _trunk);
            SetCollision(12, 3);
            SetCollision(12, 2);

            // Flower accents
            SetDecor(3, 4, _flowers);
            SetDecor(8, 7, _flowers);
            SetDecor(16, 3, _flowers);
        }

        void GenerateSouthField()
        {
            FillGrassVaried();
            PlayerSpawn = new Vector2Int(10, 2);

            // Tall grass patches (encounter zones)
            for (int x = 2; x <= 7; x++)
                for (int y = 5; y <= 10; y++)
                    SetGround(x, y, _tallGrass);

            for (int x = 12; x <= 18; x++)
                for (int y = 7; y <= 13; y++)
                    SetGround(x, y, _tallGrass);

            for (int x = 5; x <= 10; x++)
                for (int y = 12; y <= 15; y++)
                    SetGround(x, y, _tallGrass);

            // Path through
            PlacePath(10, 0, 10, H - 1);
            PlacePath(3, 3, 10, 3);

            // Flower dots scattered
            SetDecor(4, 8, _flowerDots);
            SetDecor(14, 10, _flowerDots);
            SetDecor(7, 14, _flowerDots);
            SetDecor(16, 5, _flowerDots);

            // Border trees
            PlaceTreeRow(H - 2, 0, W - 1, 3);

            // Some rocks
            SetDecor(1, 3, _rocks);
            SetCollision(1, 3);
            SetDecor(18, 3, _rocks);
            SetCollision(18, 3);

            // Side trees
            for (int y = 4; y < H - 3; y += 4)
            {
                SetDecor(0, y + 1, _tree);
                SetDecor(0, y, _trunk);
                SetCollision(0, y);
                SetCollision(0, y + 1);
                SetDecor(W - 1, y + 1, _tree);
                SetDecor(W - 1, y, _trunk);
                SetCollision(W - 1, y);
                SetCollision(W - 1, y + 1);
            }
        }

        void GenerateCreekPath()
        {
            FillGrassVaried();
            PlayerSpawn = new Vector2Int(3, 2);

            // Creek running vertically (center-right)
            for (int y = 0; y < H; y++)
            {
                int cx = 11 + (y % 3 == 0 ? 1 : 0); // slight meander
                SetGround(cx - 1, y, _stoneBank);
                SetCollision(cx - 1, y);
                SetGround(cx, y, _shallowWater);
                SetCollision(cx, y);
                SetGround(cx + 1, y, _shallowWater);
                SetCollision(cx + 1, y);
                SetGround(cx + 2, y, _stoneBank);
                SetCollision(cx + 2, y);
            }

            // Small bridges crossing the creek
            for (int bx = 10; bx <= 14; bx++)
            {
                SetGround(bx, 6, _bridge);
                _collisionMap.SetTile(new Vector3Int(bx, 6, 0), null); // walkable
                SetGround(bx, 13, _bridge);
                _collisionMap.SetTile(new Vector3Int(bx, 13, 0), null);
            }

            // Reeds along water
            SetDecor(9, 3, _reeds);
            SetDecor(9, 8, _reeds);
            SetDecor(15, 5, _reeds);
            SetDecor(15, 11, _reeds);

            // Path on left side
            PlacePath(3, 0, 3, H - 1);
            PlacePath(3, 6, 10, 6);
            PlacePath(3, 13, 10, 13);

            // Right side path
            PlacePath(16, 0, 16, H - 1);

            // Encounter zones (grass near water edges)
            for (int x = 5; x <= 8; x++)
                for (int y = 8; y <= 11; y++)
                    SetGround(x, y, _tallGrass);

            for (int x = 15; x <= 18; x++)
                for (int y = 2; y <= 5; y++)
                    SetGround(x, y, _tallGrass);

            // Trees along borders
            PlaceTreeRow(H - 2, 0, 8, 3);
            PlaceTreeRow(H - 2, 16, W - 1, 3);
        }

        void GenerateHillRoad()
        {
            FillGrassVaried();
            PlayerSpawn = new Vector2Int(10, 2);

            // Slope/hill feel: diagonal path
            for (int i = 0; i < H; i++)
            {
                int px = 5 + (i * 10 / H);
                SetGround(px, i, _path);
                SetGround(px + 1, i, _path);

                // Wind grass on sides
                if (px > 1) SetGround(px - 1, i, _windGrass);
                if (px + 2 < W) SetGround(px + 2, i, _windGrass);
            }

            // Slope dirt patches
            for (int x = 0; x <= 3; x++)
                for (int y = 0; y <= 4; y++)
                    SetGround(x, y, _slopeDirt);

            for (int x = 16; x < W; x++)
                for (int y = 13; y < H; y++)
                    SetGround(x, y, _slopeDirt);

            // Encounter areas (wind creatures in open areas)
            for (int x = 1; x <= 4; x++)
                for (int y = 8; y <= 12; y++)
                    SetGround(x, y, _tallGrass);

            for (int x = 14; x <= 18; x++)
                for (int y = 5; y <= 9; y++)
                    SetGround(x, y, _tallGrass);

            // Rocks and trees as obstacles
            SetDecor(3, 14, _rocks);
            SetCollision(3, 14);
            SetDecor(17, 3, _rocks);
            SetCollision(17, 3);

            PlaceTreeRow(H - 1, 0, 4, 2);
            PlaceTreeRow(H - 1, 16, W - 1, 2);

            // Wind grass accents
            SetDecor(8, 5, _windGrass);
            SetDecor(12, 10, _windGrass);
            SetDecor(2, 6, _windGrass);
        }

        void GenerateStonebridge()
        {
            FillGrassVaried();
            PlayerSpawn = new Vector2Int(10, 2);

            // River running horizontally
            for (int x = 0; x < W; x++)
            {
                SetGround(x, 8, _stoneBank);
                SetCollision(x, 8);
                SetGround(x, 9, _deepWater);
                SetCollision(x, 9);
                SetGround(x, 10, _deepWater);
                SetCollision(x, 10);
                SetGround(x, 11, _stoneBank);
                SetCollision(x, 11);
            }

            // The bridge itself (center)
            for (int y = 8; y <= 11; y++)
            {
                SetGround(9, y, _bridge);
                _collisionMap.SetTile(new Vector3Int(9, y, 0), null);
                SetGround(10, y, _bridge);
                _collisionMap.SetTile(new Vector3Int(10, y, 0), null);
                SetGround(11, y, _bridge);
                _collisionMap.SetTile(new Vector3Int(11, y, 0), null);
            }

            // Stone posts at bridge ends
            SetDecor(8, 8, _stonePost);
            SetCollision(8, 8);
            SetDecor(12, 8, _stonePost);
            SetCollision(12, 8);
            SetDecor(8, 11, _stonePost);
            SetCollision(8, 11);
            SetDecor(12, 11, _stonePost);
            SetCollision(12, 11);

            // Paths leading to bridge
            PlacePath(10, 0, 10, 8);
            PlacePath(10, 11, 10, H - 1);

            // Small encounter patches
            for (int x = 1; x <= 5; x++)
                for (int y = 2; y <= 5; y++)
                    SetGround(x, y, _tallGrass);

            for (int x = 14; x <= 18; x++)
                for (int y = 13; y <= 16; y++)
                    SetGround(x, y, _tallGrass);

            // NPC by the bridge
            // (NPC at position ~7, 7)

            // Trees along edges
            for (int y = 0; y < 7; y += 3)
            {
                SetDecor(0, y + 1, _tree);
                SetDecor(0, y, _trunk);
                SetCollision(0, y);
                SetCollision(0, y + 1);
                SetDecor(W - 1, y + 1, _tree);
                SetDecor(W - 1, y, _trunk);
                SetCollision(W - 1, y);
                SetCollision(W - 1, y + 1);
            }

            // Reeds near water
            SetDecor(3, 8, _reeds);
            SetDecor(16, 11, _reeds);
        }

        void GenerateNorthMeadow()
        {
            FillGrassVaried();
            PlayerSpawn = new Vector2Int(10, 2);

            // Open meadow feel - fewer obstacles, more flowers
            // Path through center
            PlacePath(10, 0, 10, H - 1);
            PlacePath(3, 9, 17, 9);

            // Flower clusters everywhere
            SetDecor(3, 4, _flowerClusters);
            SetDecor(6, 7, _flowerClusters);
            SetDecor(8, 13, _flowerClusters);
            SetDecor(13, 5, _flowerClusters);
            SetDecor(16, 11, _flowerClusters);
            SetDecor(5, 15, _flowerClusters);
            SetDecor(14, 14, _flowerClusters);
            SetDecor(2, 10, _flowerClusters);
            SetDecor(17, 7, _flowerClusters);

            // Sparse encounter zones (Meadow creatures are rare)
            for (int x = 2; x <= 6; x++)
                for (int y = 4; y <= 8; y++)
                    SetGround(x, y, _tallGrass);

            for (int x = 13; x <= 17; x++)
                for (int y = 10; y <= 14; y++)
                    SetGround(x, y, _tallGrass);

            // Very few trees (open feel)
            SetDecor(1, 2, _tree);
            SetDecor(1, 1, _trunk);
            SetCollision(1, 1);
            SetCollision(1, 2);
            SetDecor(18, 16, _tree);
            SetDecor(18, 15, _trunk);
            SetCollision(18, 15);
            SetCollision(18, 16);

            // NPC in the meadow
            // (NPC at ~12, 9)

            // Flowers on path edges
            SetDecor(11, 4, _flowers);
            SetDecor(9, 12, _flowers);
        }

        void GenerateQuietGrove()
        {
            // Pale, quiet palette
            for (int x = 0; x < W; x++)
                for (int y = 0; y < H; y++)
                    _groundMap.SetTile(new Vector3Int(x, y, 0), _paleGrass);

            PlayerSpawn = new Vector2Int(10, 2);

            // Gentle path
            PlacePath(10, 0, 10, 12);

            // Canopy overhead (decoration)
            for (int x = 0; x < W; x++)
            {
                SetDecor(x, H - 1, _canopy);
                SetDecor(x, H - 2, _canopy);
                if (x < 3 || x > 16)
                {
                    SetDecor(x, H - 3, _canopy);
                }
            }

            // Roots scattered
            SetDecor(3, 5, _roots);
            SetDecor(7, 8, _roots);
            SetDecor(14, 6, _roots);
            SetDecor(16, 10, _roots);
            SetDecor(5, 12, _roots);
            SetDecor(12, 3, _roots);

            // Trees (sparse, ancient feel)
            for (int x = 1; x < W - 1; x += 5)
            {
                SetDecor(x, 14, _tree);
                SetDecor(x, 13, _trunk);
                SetCollision(x, 13);
                SetCollision(x, 14);
            }

            // Still encounter position (center-ish clearing)
            // Still appears around (10, 10) - marked with pale space
            SetGround(9, 10, _paleGrass);
            SetGround(10, 10, _paleGrass);
            SetGround(11, 10, _paleGrass);
            SetGround(9, 11, _paleGrass);
            SetGround(10, 11, _paleGrass);
            SetGround(11, 11, _paleGrass);

            // Optional NPC near the end
            // (NPC at ~10, 14)

            // No encounter tiles - this is the no-battle zone
            // The only encounter is the scripted Still trigger at (10, 10)

            // Border: roots and canopy make natural walls
            for (int y = 0; y < H; y++)
            {
                SetCollision(0, y);
                SetCollision(W - 1, y);
                SetDecor(0, y, _roots);
                SetDecor(W - 1, y, _roots);
            }
        }
    }
}
