using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TestCollision : MonoBehaviour
{
    public Tilemap _tileMap;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        List<Vector3Int> blocked = new List<Vector3Int>();

        foreach (Vector3Int pos in _tileMap.cellBounds.allPositionsWithin) {
            TileBase tile = _tileMap.GetTile(pos);
            if(tile != null) {
                blocked.Add(pos);
            }
        }
    }
}
