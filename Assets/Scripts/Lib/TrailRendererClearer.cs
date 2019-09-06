using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailRendererClearer : MonoBehaviour
{
    private TrailRenderer[] rds;

    void Awake()
    {
        rds = this.GetComponentsInChildren<TrailRenderer>();
    }

    void OnEnable()
    {
        Clear();
    }

    [ContextMenu("UpdateScaleByWorldScale")]
    public void UpdateScaleByWorldScale()
    {
        UpdateScale(this.transform.lossyScale.x);
    }

    public void UpdateScale(float scale)
    {
        if (rds != null)
        {
            foreach (var trailRenderer in rds)
            {
                trailRenderer.widthMultiplier *= scale;
            }
        }
    }

    public void Clear()
    {
        if (rds != null)
        {
            foreach (var trailRenderer in rds)
            {
                trailRenderer.Clear();
            }
        }
    }
}
