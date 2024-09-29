using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;

public class TerrainGenerator : MonoBehaviour
{
    public GameObject[] sectionPrefabs;
    public Transform player;

    private Vector3 nextPosition;
    private List<TerrainSection> sections = new List<TerrainSection>();

    private void Start()
    {
        nextPosition = transform.position;
    }

    private TerrainSection PlaceNextSection() 
    {
        int sectionIndex = UnityEngine.Random.Range(0, sectionPrefabs.Length);

        GameObject instance = Instantiate(sectionPrefabs[sectionIndex], nextPosition, Quaternion.identity);
        instance.transform.parent = transform;
        TerrainSection section = instance.GetComponent<TerrainSection>();
        section.prefabIndex = sectionIndex;

        Vector3 sectionSize = section.bounds.size;
        nextPosition += new Vector3(0, -sectionSize.y, sectionSize.z);

        return section;
    }

    private TerrainSection PlaceNeighborSection(TerrainSection origin, NeighborSide side) {
        Vector3 position = origin.transform.position;
        position += Vector3.right * origin.bounds.size.x * (side == NeighborSide.Left ? 1 : -1);

        int prefabIndex = origin.prefabIndex; 
        GameObject instance = Instantiate(sectionPrefabs[prefabIndex], position, Quaternion.identity);
        instance.transform.parent = transform;
        TerrainSection section = instance.GetComponent<TerrainSection>();
        section.prefabIndex = prefabIndex;
        if (side == NeighborSide.Left) {
            origin.leftSection = section;
            section.rightSection = origin;
        }
        else {
            origin.rightSection = section;
            section.leftSection = origin;
        }

        var props = section.GetComponentsInChildren<Prop>();
        foreach(Prop prop in props)
            prop.Init();

        return section;
    }

    private void Update()
    {
        nextPosition.x = player.position.x;

        if (nextPosition.z - player.position.z < 500) 
        {
            var origin = PlaceNextSection();
            sections.Add(origin);
            sections.Add(PlaceNeighborSection(origin, NeighborSide.Left));
            sections.Add(PlaceNeighborSection(origin, NeighborSide.Right));
        }

        List<TerrainSection> newInstances = new List<TerrainSection>();
        foreach (TerrainSection section in sections) {
            if (!section)
                continue;

            Vector3 sectionCenter = section.transform.position + section.bounds.center;
            if (sectionCenter.z - player.position.z < -200)
            {
                Destroy(section.gameObject);
                continue;
            }

            float sectionXSize = section.bounds.size.x;
            if (section.rightSection == null) {
                Vector3 rightSideSection = sectionCenter - Vector3.right * sectionXSize;
                if (Mathf.Abs(rightSideSection.x - player.position.x) < 600) {
                    //Mathf.Abs(sectionCenter.x - player.position.x) + sectionXSize) {

                    var newInstance = PlaceNeighborSection(section, NeighborSide.Right);
                    section.rightSection = newInstance;
                    newInstances.Add(newInstance);
                }
            }
            if (section.leftSection == null) {
                Vector3 leftSideSection = sectionCenter - Vector3.left * sectionXSize;
                if (Mathf.Abs(leftSideSection.x - player.position.x) < 600) {
                    //Mathf.Abs(sectionCenter.x - player.position.x) + sectionXSize) {

                    var newInstance = PlaceNeighborSection(section, NeighborSide.Left);
                    section.leftSection = newInstance;
                    newInstances.Add(newInstance);
                }
            }
        }
        sections.RemoveAll(s => s == null);
        sections.AddRange(newInstances);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        foreach (TerrainSection section in sections) {
            Vector3 sectionCenter = section.transform.position + section.bounds.center;
            Gizmos.DrawSphere(sectionCenter, 5);
        }
    }

    public enum NeighborSide { Left, Right }
}
