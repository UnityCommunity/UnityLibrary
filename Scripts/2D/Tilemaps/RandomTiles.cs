// Requires: Unity 5.5.0a1 or later
// Fills tilemap with random tiles
// Usage: Attach this script to Tilemap layer, assign tiles, hit play

using UnityEngine;

public class RandomTiles : MonoBehaviour
{
    public Tile[] tiles;

    void Start()
    {
        RandomTileMap();
    }

    void RandomTileMap()
    {
        TileMap map = GetComponent<TileMap>();

        int sizeX = 32;
        int sizeY = 16;

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                var tilePos = new Vector3Int(x, y, 0);
                map.SetTile(tilePos, tiles[Random.Range(0, tiles.Length)]);
            }
        }
    }

}
