using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController : MonoBehaviour
{
    [SerializeField] Transform anchorTransform;

    public Vector3 FindRelativeJointPos()
    {
        return anchorTransform.position;
    }
}
