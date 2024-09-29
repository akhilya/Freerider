using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour
{
    public new Transform transform;
    public Vector2Int scaleRange = new Vector2Int(35, 50);
    public Vector2Int xzRotationRange = new Vector2Int(-5, 5);
    public Vector2 yOffsetRange = new Vector2(-0.5f, 0.0f);
    public int appearChance = 50;
    public Vector2 zScaleRange = new Vector2(1, 1);

    private void OnValidate() {
        transform = GetComponent<Transform>();
        transform.localScale = Vector3.one * scaleRange.x;
    }

    private void Start() {
        Init();
    }

    public void Init() {
        bool appear = Random.value * 100 < appearChance;

        if (!appear) {
            gameObject.SetActive(false);
            return;
        }

        float yRot = Random.Range(0, 360),
            xRot = -90 + Random.Range(xzRotationRange.x, xzRotationRange.y),
            zRot = -90 + Random.Range(xzRotationRange.x, xzRotationRange.y);

        transform.rotation = Quaternion.Euler(xRot, yRot, zRot);
        Vector3 scale = Vector3.one * Random.Range(scaleRange.x, scaleRange.y);
        scale.z *= Random.Range(zScaleRange.x, zScaleRange.y);
        transform.localScale = scale;
        transform.localPosition += Vector3.up * Random.Range(yOffsetRange.x, yOffsetRange.y);
    }
}
