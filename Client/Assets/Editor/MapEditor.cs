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
    private static void GenerateMap()
    {
        GenerateByPath("Assets/Resources/Map");
        GenerateByPath("../Common/MapData");
    }

    private static void GenerateByPath(string pathPrefix)
    {
        GameObject[] gameObjects = Resources.LoadAll<GameObject>("Prefabs/Map");

        foreach (GameObject go in gameObjects) {

            Tilemap tilemapBase = Util.FindChild<Tilemap>(go, "Tilemap_Base", true);
            Tilemap tilemaps = Util.FindChild<Tilemap>(go, "Tilemap_Collision", true);

            List<Vector3Int> blocked = new List<Vector3Int>();

            // 블로킹 타일 추출
            foreach (Vector3Int pos in tilemaps.cellBounds.allPositionsWithin) {
                TileBase tile = tilemaps.GetTile(pos);
                if (tile != null) {
                    blocked.Add(pos);
                }
            }

            // 파일 생성
            // TODO : 바이너리로 할지(압축) / TXT 형태로 할지(편리하게 볼 수 있도록) 고민해봐야 함 
            using (var writer = File.CreateText($"{pathPrefix}/{go.name}.txt")) {
                writer.WriteLine(tilemapBase.cellBounds.xMin);
                writer.WriteLine(tilemapBase.cellBounds.xMax);
                writer.WriteLine(tilemapBase.cellBounds.yMin);
                writer.WriteLine(tilemapBase.cellBounds.yMax);

                for (int y = tilemapBase.cellBounds.yMax; y >= tilemapBase.cellBounds.yMin; y--) {
                    for (int x = tilemapBase.cellBounds.xMin; x <= tilemapBase.cellBounds.xMax; x++) {
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
    }
    #endif
}
