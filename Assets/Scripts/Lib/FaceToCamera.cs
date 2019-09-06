using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class FaceToCamera : MonoBehaviour
{
    public Transform camTrans;
    private Transform _trans;

    void Start()
    {
        _trans = this.transform;
        if (Camera.main != null)
            camTrans = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (camTrans != null)
        {
            _trans.LookAt(_trans.position + camTrans.rotation * Vector3.forward,
                camTrans.rotation * Vector3.up);
        }
    }
}