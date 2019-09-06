using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticBatchingBaker : MonoBehaviour
{
    public GameObject staticRoot;

    public GameObject[] targetGos;

    // Use this for initialization
    void Start()
    {
        if (targetGos != null && targetGos.Length > 0)
        {
            StaticBatchingUtility.Combine(targetGos, staticRoot);
        }
        else
        {
            StaticBatchingUtility.Combine(staticRoot);
        }
    }
}
