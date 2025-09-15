using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Bson;
using UnityEngine;

namespace GameTool.Hex
{
    public class HexEnvController : MonoBehaviour
	{
        [SerializeField]
        public HexSystem HexSystem = new HexSystem();
        public HexTiles PrefabsHexagonTilesFlat;
        public HexTiles PrefabsHexagonTilesPointy;
        public Dictionary<HexInt, HexTiles> TilesDic = new Dictionary<HexInt, HexTiles>();

        public int TileColorID = 0;
        public List<Color> TileColors = new List<Color>
        {
            new Color(0.5f,0.5f,0.5f,0.5f),
            new Color(0f,0.5f,0.5f,0.5f),
            new Color(0.5f,0f,0.5f,0.5f),
            new Color(0.5f,0.5f,0f,0.5f),
        };

        private float currentZ = 0f;
        private Vector3 StartingScale = new Vector3();

        // Debug
        public int ChildCount = 0; 

        private void Awake()
		{
			InitTiles();
		}

		private void OnEnable()
		{
			EventManager.StartListening(EventName.InitHexAtMouse, GenerateTileAtMouse);
            EventManager.StartListening(EventName.ClearTileAtMouse, ClearTileAtMouse);
            EventManager.StartListening<HexInt>(EventName.InitHexByHexInt, GenerateTileByHexInt);
			EventManager.StartListening<int>(EventName.InitTilesHexRing, GenerateTilesHexRing);
			EventManager.StartListening<int>(EventName.InitTilesHexPlane, GenerateTilesHexPlane);
            EventManager.StartListening(EventName.ClearAllTiles, ClearAllTiles);
            EventManager.StartListening<float>(EventName.InitTilesCirPlane, GenerateTilesCirPlane);
            EventManager.StartListening<HexInt>(EventName.DisplayDataByHexInt, DisplayDataByHexInt);
            StartingScale = PrefabsHexagonTilesFlat.transform.localScale;
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                GenerateTileAtMouse();
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                ClearTileAtMouse();
            }
            ChildCount = this.transform.childCount-2;
        }
        private void InitTiles()
		{
            PrefabsHexagonTilesFlat.gameObject.SetActive(false);
            PrefabsHexagonTilesPointy.gameObject.SetActive(false);
		}

        // GenerateTile
        private void GenerateTileByPixel(Cart cart, HexInt hexInt)
        {
            var PrefabsHexagonTiles = HexSystem.Orientation == Orientations.Flat ?
                PrefabsHexagonTilesFlat : PrefabsHexagonTilesPointy;

            PrefabsHexagonTiles.gameObject.SetActive(true);
            HexTiles item;
            if (!TilesDic.TryGetValue(hexInt, out item))
            {
                item = Instantiate(PrefabsHexagonTiles, this.transform);
                TilesDic.Add(hexInt, item);
            }

            item.transform.localPosition = (Vector3)cart + new Vector3(0, 0, currentZ);
            currentZ += float.Epsilon;
            item.transform.localPosition += (Vector3)HexSystem.GetOriginCart();

            item.SetColor(TileColors[TileColorID % 4]);
            item.transform.localScale = StartingScale * HexSystem.Scale;
            
            PrefabsHexagonTiles.gameObject.SetActive(false);
        }

        public void GenerateTileAtMouse()
		{
			Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(
				new Vector3((Input.mousePosition).x, (Input.mousePosition).y, Camera.main.nearClipPlane)
				);
            Cart cart = new Cart(mouseWorld.x, mouseWorld.y);
            GenerateTileByHexInt(cart.ToHex(HexSystem.Scale , HexSystem.Orientation));
        }

        public void ClearTileAtMouse()
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(
				new Vector3((Input.mousePosition).x, (Input.mousePosition).y, Camera.main.nearClipPlane)
				);
            Cart cart = new Cart(mouseWorld.x, mouseWorld.y);
            HexInt hexInt = cart.ToHex(HexSystem.Scale, HexSystem.Orientation);
            if (TilesDic.ContainsKey(hexInt))
            {
                Destroy(TilesDic[hexInt].gameObject);
                TilesDic.Remove(hexInt);
            }
        }

        public void GenerateTilesHexPlane(int size)
        {
            for (int i = 0; i <= size; i++)
            {
                GenerateTilesHexRing(i);
            }
        }

        public void GenerateTilesHexRing(int size)
        {
            if (size == 0) { GenerateTileByHexInt( new HexInt()); }

            HexInt hex = new HexInt();
            int start = 0;
            hex += size * Hex.UnitRing[start];
            for (int j = start + 2; j < start + 8; j++)
            {
                for (int i = 0; i < size; i++)
                {
                    hex += Hex.UnitRing[ j % 6];
                    GenerateTileByHexInt(hex);
                }
            }
        }

        public void GenerateTilesCirPlane(float sizeInHex)
        {
            GenerateTileByHexInt(new HexInt());
            for (int i = 1; i <= sizeInHex * 2f; i++)
            {
                if (!TryGenerateTilesRing(i , sizeInHex)) { break; }
            }
        }

        public bool TryGenerateTilesRing(int hexSize , float targetSize)
        {
            bool found = false;
            HexInt hex = new HexInt();
            int start = 0;
            hex += hexSize * Hex.UnitRing[start];
            for (int j = start + 2; j < start + 8; j++)
            {
                for (int i = 0; i < hexSize; i++)
                {
                    hex += Hex.UnitRing[j % 6];
                    if ( Hex.GetHexDistance(hex , HexSystem) - Hex.EPS < targetSize)
                    {
                        GenerateTileByHexInt(hex);
                        found = true;
                    }
                }
            }
            return found;
        }

        public void GenerateTileByHexInt(HexInt hex)
		{
            HexEntity ent = new HexEntity(hex, HexSystem);
			GenerateTileByPixel(ent.ToCart() , hex);
		}



        public void ClearAllTiles()
        {
            foreach (var item in TilesDic)
            {
                Destroy(item.Value.gameObject);
            }
            TilesDic.Clear();
        }

        public void DisplayDataByHexInt(HexInt hexInt)
        {
            Debug.LogWarning( Hex.GetData(hexInt).ToString());
            Debug.LogWarning(Hex.HexDataDic.Count.ToString());

        }


    }
}
