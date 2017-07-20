// tested with unity version: 2017.2.0b4
// info: Fills tilemap with random tiles
// usage: Attach this script to empty gameobject, assign some tiles, then press play

using UnityEngine;
using UnityEngine.Tilemaps;

namespace UnityLibary
{
    public class RandomTiles : MonoBehaviour
    {
        public int width = 32;
        public int height = 32;

        public Tile[] tiles;

        void Start()
        {
            RandomTileMap();
        }

        void RandomTileMap()
        {
            // validation
            if (tiles == null || tiles.Length < 1)
            {
                Debug.LogError("Tiles not assigned", gameObject);
                return;
            }

            var parent = transform.parent;
            if (parent == null)
            {
                var go = new GameObject("grid");
                go.AddComponent<Grid>();
                transform.SetParent(go.transform);
            } else
            {
                if (parent.GetComponent<Grid>() == null)
                {
                    parent.gameObject.AddComponent<Grid>();
                }
            }

            TilemapRenderer tr = GetComponent<TilemapRenderer>();
            if (tr == null)
            {
                tr = gameObject.AddComponent<TilemapRenderer>();
            }

            Tilemap map = GetComponent<Tilemap>();
            if (map == null)
            {
                map = gameObject.AddComponent<Tilemap>();
            }


            // random map generation
            Vector3Int tilePos = Vector3Int.zero;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tilePos.x = x;
                    tilePos.y = y;
                    map.SetTile(tilePos, tiles[Random.Range(0, tiles.Length)]);
                }
            }
        }
    }
}
