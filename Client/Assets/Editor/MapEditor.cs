using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapEditor
{
    #if UNITY_EDITOR

    [MenuItem("Tools/GenerateMap")]
    private static void HellowWorld()
    {
        GameObject go = GameObject.Find("Map");
        if(go == null) {
            return;
        }

        Tilemap tilemaps = Util.FindChild<Tilemap>(go, "Tilemap_Collision", true);
        if(tilemaps == null) {
            return;
        }

        List<Vector3Int> blocked = new List<Vector3Int>();

        // 블로킹 타일 추출
        foreach(Vector3Int pos in tilemaps.cellBounds.allPositionsWithin) {
            TileBase tile = tilemaps.GetTile(pos);
            if(tile != null) {
                blocked.Add(pos);
            }
        }

        // 파일 생성
        // TODO : 바이너리로 할지(압축) / TXT 형태로 할지(편리하게 볼 수 있도록) 고민해봐야 함 
        string mapFileName = "default";
        using (var writer = File.CreateText($"Assets/Resources/Map/output_{mapFileName}.txt")) {
            writer.WriteLine(tilemaps.cellBounds.xMin);
            writer.WriteLine(tilemaps.cellBounds.xMax);
            writer.WriteLine(tilemaps.cellBounds.yMin);
            writer.WriteLine(tilemaps.cellBounds.yMax);

            for (int y = tilemaps.cellBounds.yMax; y >= tilemaps.cellBounds.yMin; y--) {
                for (int x = tilemaps.cellBounds.xMin; x <= tilemaps.cellBounds.xMax; x++) {
                    TileBase tile = tilemaps.GetTile(new Vector3Int(x, y, 0));
                    if (tile != null) {
                        writer.Write("1");
                    } else {
                        writer.Write("0");
                    }
                }
                writer.WriteLine();
            }
        }
    }
    #endif
}
