using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Bson;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameTool.Hex
{
    /// <summary>
    /// Main controller for managing hexagonal tile generation and placement in the scene.
    /// Handles both flat-topped and pointy-topped hexagon orientations.
    /// Uses event-driven architecture for tile generation commands.
    /// </summary>
    public class HexEnvController : SceneSingleton<HexEnvController>
	{
        [Header("Hex System Configuration")]
        [SerializeField]
        public HexSystem HexSystem = new HexSystem();
        
        [Header("Hexagon Prefabs")]
        [Tooltip("Prefab for flat-topped hexagons (orientation = Flat)")]
        public HexTiles PrefabsHexagonTilesFlat;
        [Tooltip("Prefab for pointy-topped hexagons (orientation = Pointy)")]
        public HexTiles PrefabsHexagonTilesPointy;
        
        [Header("Tile Management")]
        [Tooltip("Dictionary storing all active tiles by their hex coordinates")]
        public Dictionary<HexInt, HexTiles> TilesDic = new Dictionary<HexInt, HexTiles>();

        [Header("Visual Settings")]
        [Tooltip("Current color index for new tiles (cycles through TileColors)")]
        public int TileColorID = 0;
        [Tooltip("Color palette for hexagon tiles (cycles through this list)")]
        public List<Color> TileColors = new List<Color>
        {
            new Color(0.5f,0.5f,0.5f,0.5f), // Gray
            new Color(0f,0.5f,0.5f,0.5f),   // Cyan
            new Color(0.5f,0f,0.5f,0.5f),   // Magenta
            new Color(0.5f,0.5f,0f,0.5f),   // Yellow
        };

        [Header("Internal State")]
        [Tooltip("Z-depth offset for proper tile layering")]
        private float currentZ = 0f;
        [Tooltip("Original scale of the prefab for proper scaling")]
        private Vector3 StartingScale = new Vector3();

        [Header("Debug Info")]
        [Tooltip("Current number of child tiles (excluding prefabs)")]
        public int ChildCount = 0;

        /// <summary>
        /// Initialize the hexagon tile system on awake
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            InitTiles();
		}

        /// <summary>
        /// Subscribe to all hex tile events when the controller becomes active
        /// </summary>
		private void OnEnable()
		{
			// Single tile generation events
			EventManager.StartListening(EventName.InitHexAtMouse, GenerateTileAtMouse);
            EventManager.StartListening(EventName.ClearTileAtMouse, ClearTileAtMouse);
            EventManager.StartListening<HexInt>(EventName.InitHexByHexInt, GenerateTileByHexInt);
            
            // Multiple tile generation events
			EventManager.StartListening<int>(EventName.InitTilesHexRing, GenerateTilesHexRing);
			EventManager.StartListening<int>(EventName.InitTilesHexPlane, GenerateTilesHexPlane);
            EventManager.StartListening<float>(EventName.InitTilesCirPlane, GenerateTilesCirPlane);
            
            // Utility events
            EventManager.StartListening(EventName.ClearAllTiles, ClearAllTiles);
            EventManager.StartListening<HexInt>(EventName.DisplayDataByHexInt, DisplayDataByHexInt);
            
            // Store original prefab scale for proper scaling
            StartingScale = PrefabsHexagonTilesFlat.transform.localScale;
        }


        /// <summary>
        /// Handle keyboard input for tile generation and update debug info
        /// </summary>
        private void Update()
        {
            // B key: Generate tile at mouse position
            if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
            {
                GenerateTileAtMouse();
            }
            // V key: Clear tile at mouse position
            if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
            {
                ClearTileAtMouse();
            }
            // Update debug counter (subtract 2 for prefab objects)
            ChildCount = this.transform.childCount-2;
        }
        
        /// <summary>
        /// Initialize tile prefabs by hiding them (they're used as templates)
        /// </summary>
        private void InitTiles()
		{
            PrefabsHexagonTilesFlat.gameObject.SetActive(false);
            PrefabsHexagonTilesPointy.gameObject.SetActive(false);
		}

        /// <summary>
        /// Core method for generating a single hex tile at a specific position
        /// </summary>
        /// <param name="cart">Cartesian coordinates for tile placement</param>
        /// <param name="hexInt">Hex coordinates for tile identification</param>
        private void GenerateTileByPixel(Cart cart, HexInt hexInt)
        {
            // Select appropriate prefab based on hex system orientation
            var PrefabsHexagonTiles = HexSystem.Orientation == Orientations.Flat ?
                PrefabsHexagonTilesFlat : PrefabsHexagonTilesPointy;

            // Temporarily activate prefab for instantiation
            PrefabsHexagonTiles.gameObject.SetActive(true);
            HexTiles item;
            
            // Check if tile already exists at this hex coordinate
            if (!TilesDic.TryGetValue(hexInt, out item))
            {
                // Create new tile instance
                item = Instantiate(PrefabsHexagonTiles, this.transform);
                TilesDic.Add(hexInt, item);
            }

            // Set the hex coordinates for this tile
            item.SetHexCoordinates(hexInt);

            // Position the tile with Z-depth layering
            item.transform.localPosition = (Vector3)cart + new Vector3(0, 0, currentZ);
            currentZ += float.Epsilon; // Increment Z for proper layering
            item.transform.localPosition += (Vector3)HexSystem.GetOriginCart();

            // Apply color and scale
            item.SetColor(TileColors[TileColorID % 4]);
            item.transform.localScale = StartingScale * HexSystem.Scale;
            
            // Hide the prefab template
            PrefabsHexagonTiles.gameObject.SetActive(false);
        }

        /// <summary>
        /// Generate a hex tile at the current mouse position
        /// Converts mouse screen coordinates to world coordinates, then to hex coordinates
        /// </summary>
        public void GenerateTileAtMouse()
		{
			// Convert mouse screen position to world coordinates
			Vector2 mouseScreenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
			Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(
				new Vector3(mouseScreenPos.x, mouseScreenPos.y, Camera.main.nearClipPlane)
				);
            Cart cart = new Cart(mouseWorld.x, mouseWorld.y);
            // Convert world coordinates to hex coordinates and generate tile
            GenerateTileByHexInt(cart.ToHex(HexSystem.Scale , HexSystem.Orientation));
        }

        /// <summary>
        /// Remove a hex tile at the current mouse position
        /// Finds the tile under the mouse and destroys it
        /// </summary>
        public void ClearTileAtMouse()
        {
            // Convert mouse screen position to world coordinates
            Vector2 mouseScreenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(
				new Vector3(mouseScreenPos.x, mouseScreenPos.y, Camera.main.nearClipPlane)
				);
            Cart cart = new Cart(mouseWorld.x, mouseWorld.y);
            // Convert to hex coordinates and find tile
            HexInt hexInt = cart.ToHex(HexSystem.Scale, HexSystem.Orientation);
            if (TilesDic.ContainsKey(hexInt))
            {
                // Destroy tile and remove from dictionary
                Destroy(TilesDic[hexInt].gameObject);
                TilesDic.Remove(hexInt);
            }
        }

        /// <summary>
        /// Generate a complete hexagonal plane (filled hexagon) of tiles
        /// Creates concentric rings from center (0,0) to the specified size
        /// </summary>
        /// <param name="size">Radius of the hex plane (number of rings from center)</param>
        public void GenerateTilesHexPlane(int size)
        {
            // Generate each ring from center (0) to outer edge (size)
            for (int i = 0; i <= size; i++)
            {
                GenerateTilesHexRing(i);
            }
        }

        /// <summary>
        /// Generate a single hexagonal ring of tiles at the specified distance from center
        /// Uses the UnitRing pattern to create a perfect hex ring
        /// </summary>
        /// <param name="size">Distance from center (0 = center tile only)</param>
        public void GenerateTilesHexRing(int size)
        {
            // Size 0 = center tile only
            if (size == 0) { GenerateTileByHexInt( new HexInt()); return; }

            // Start at the first position of the ring
            HexInt hex = new HexInt();
            int start = 0;
            hex += size * Hex.UnitRing[start];
            
            // Generate tiles around the ring using UnitRing pattern
            for (int j = start + 2; j < start + 8; j++)
            {
                for (int i = 0; i < size; i++)
                {
                    hex += Hex.UnitRing[ j % 6];
                    GenerateTileByHexInt(hex);
                }
            }
        }

        /// <summary>
        /// Generate a circular area of tiles (not a perfect hex, but circular)
        /// Creates tiles within a circular radius from the center
        /// </summary>
        /// <param name="sizeInHex">Radius of the circular area in hex units</param>
        public void GenerateTilesCirPlane(float sizeInHex)
        {
            // Start with center tile
            GenerateTileByHexInt(new HexInt());
            
            // Generate rings until we exceed the circular radius
            for (int i = 1; i <= sizeInHex * 2f; i++)
            {
                if (!TryGenerateTilesRing(i , sizeInHex)) { break; }
            }
        }

        /// <summary>
        /// Try to generate tiles in a ring, but only if they fall within the circular radius
        /// Used by GenerateTilesCirPlane to create circular rather than hexagonal patterns
        /// </summary>
        /// <param name="hexSize">Ring size to generate</param>
        /// <param name="targetSize">Maximum distance from center (circular radius)</param>
        /// <returns>True if any tiles were generated in this ring</returns>
        public bool TryGenerateTilesRing(int hexSize , float targetSize)
        {
            bool found = false;
            HexInt hex = new HexInt();
            int start = 0;
            hex += hexSize * Hex.UnitRing[start];
            
            // Check each position in the ring
            for (int j = start + 2; j < start + 8; j++)
            {
                for (int i = 0; i < hexSize; i++)
                {
                    hex += Hex.UnitRing[j % 6];
                    // Only generate tile if it's within the circular radius
                    if ( Hex.GetHexDistance(hex , HexSystem) - Hex.EPS < targetSize)
                    {
                        GenerateTileByHexInt(hex);
                        found = true;
                    }
                }
            }
            return found;
        }

        /// <summary>
        /// Generate a tile at specific hex coordinates
        /// Converts hex coordinates to world position and creates the tile
        /// </summary>
        /// <param name="hex">Hex coordinates where the tile should be placed</param>
        public void GenerateTileByHexInt(HexInt hex)
		{
            // Create hex entity and convert to world coordinates
            HexEntity ent = new HexEntity(hex, HexSystem);
			GenerateTileByPixel(ent.ToCart() , hex);
		}

        /// <summary>
        /// Remove all generated tiles from the scene
        /// Destroys all tile GameObjects and clears the dictionary
        /// </summary>
        public void ClearAllTiles()
        {
            // Destroy all tile GameObjects
            foreach (var item in TilesDic)
            {
                Destroy(item.Value.gameObject);
            }
            // Clear the dictionary
            TilesDic.Clear();
        }

        /// <summary>
        /// Display debug information for a specific hex coordinate
        /// Shows hex data and total number of cached hex data entries
        /// </summary>
        /// <param name="hexInt">Hex coordinates to display data for</param>
        public void DisplayDataByHexInt(HexInt hexInt)
        {
            Debug.LogWarning( Hex.GetData(hexInt).ToString());
            Debug.LogWarning(Hex.HexDataDic.Count.ToString());
        }


    }
}
