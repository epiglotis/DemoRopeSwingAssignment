using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    [SerializeField] Transform anchorTransform;
    
    public Vector3 GetAnchorPosition(){

        return anchorTransform.position;

    }

}
