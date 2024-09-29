using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainSection : MonoBehaviour
{
    [NonReorderable]
    public List<Transform> tiles;

    [HideInInspector]
    public Bounds bounds;
    public Vector3 boundsSizeExtend = new Vector3(5f, 0, 5f);
    public Vector3 boundOffset = new Vector3(2.5f, 0, 2.5f);

    public TerrainSection leftSection = null;
    public TerrainSection rightSection = null;

    [HideInInspector]
    public int prefabIndex = -1;

    [ContextMenu("Update Bounds")]
    private void OnValidate() 
    {
        tiles = GetComponentsInChildren<Transform>().ToList();
        tiles.RemoveAll(tile => tile.TryGetComponent<Prop>(out _));

        if (tiles.Count > 0) 
        {
            bounds = new Bounds(tiles[0].position, Vector3.zero);
            foreach (Transform tile in tiles)
            {
                bounds.Encapsulate(tile.position);
            }
            bounds.center += boundOffset;
            bounds.size += boundsSizeExtend;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + bounds.center, bounds.size);
    }
}
