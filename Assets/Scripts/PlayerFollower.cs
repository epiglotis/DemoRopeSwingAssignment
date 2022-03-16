//This class is on the main camera to follow player.
//You may optimize it on SetPosition section and
//Write a proper way to update blocks positions on the map to make it an infite gameplay.

using UnityEngine;

public class PlayerFollower : MonoBehaviour {

    [SerializeField] private Transform player;
    private float zDifference;
    private float yDifference;
    Vector3 velocity = Vector3.zero;

    private void Start() {
        
        SetPosition(player);
    }

    public void SetPosition(Transform p)
    {
        //Optimize this portion
        player = p;
        zDifference = player.transform.position.z - transform.position.z;
        yDifference = player.transform.position.y - transform.position.y;
        
    }
    private void Update()
    {
        if(player != null)
        {
            Vector3 targetPosition = player.position - new Vector3(0,yDifference,zDifference);
            Vector3 currentPos = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 0);
            transform.position = new Vector3(transform.position.x, currentPos.y, currentPos.z);
        }
    }

}
