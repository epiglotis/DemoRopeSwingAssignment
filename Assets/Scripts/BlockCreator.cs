using System.Collections.Generic;
using UnityEngine;


//In this class, the map has been created.
//You have to edit GetRelativeBlock section to calculate current relative block to cast player rope to hold on
//Update Block Position section to make infinite map.
public class BlockCreator : MonoBehaviour {

    private static BlockCreator singleton = null;
    [SerializeField] GameObject blockPrefab;
    [SerializeField] Transform playerTransform;
    [SerializeField] GameObject pointPrefab;
    public float difficulty = 2f;
    public int blockCount = 16;
    public int lastHeightUpperBlock = 20;

    private List<GameObject> blockPool = new List<GameObject>();
    private int oldestBlockIndex;
    private int newestBlockIndex;
    private float lastPlayerZ;
    private float colorChangeValue;
    private int blockCounter;
    private float blockWidth;

    public static BlockCreator GetSingleton(){

        if(singleton == null){

            singleton = new GameObject("_BlockCreator").AddComponent<BlockCreator>();

        }

        return singleton;

    }

    public void Initialize(int bCount, GameObject bPrefab, GameObject pPrefab, Transform playerT)
    {
        blockCount = bCount;
        blockPrefab = bPrefab;
        pointPrefab = pPrefab;
        playerTransform = playerT;
        InstantiateBlocks();
    }

    private void InstantiateBlocks()
    {
        //Instantiate block here

        // blockWidth will be needed for a proper positioning
        blockWidth = blockPrefab.transform.localScale.z;

        for (int i = 0; i < blockCount; i++)
        {
            // We have a block on the start so we will just add it to the list and continue creating others
            if (i == 0)
            {
                blockPool.Add(blockPrefab);
                SetColor(0);
            }

            else
            {
                GameObject newColumn = Instantiate(blockPrefab);
                newColumn.transform.parent = blockPrefab.transform.parent;
                blockPool.Add(newColumn);
                UpdateBlockPosition(blockPool.Count - 1, blockPool[blockPool.Count - 2].transform.position, 1);
            }
        }
        // colorChangeValue will be increased for each block
        colorChangeValue = 0;

        // We have to store oldest and newest blocks to move oldest to right next to newest
        oldestBlockIndex = 0;
        newestBlockIndex = blockPool.Count - 1;
    }
	
	void Update (){
        // If player moved past a block put the oldest block right next to newest. (-6 stands for moving those block offscreen)
		if (playerTransform.position.z - 6 > lastPlayerZ + blockWidth)
        {
            UpdateBlockPosition(oldestBlockIndex, blockPool[newestBlockIndex].transform.position, difficulty);
            
            // Update newest and oldest blocks.
            newestBlockIndex = oldestBlockIndex;
            oldestBlockIndex = (oldestBlockIndex + 1) % blockPool.Count;
            lastPlayerZ += blockWidth;
        }
	}

    private void UpdateBlockPosition(int columnIndex, Vector3 prevColumnPos, float maximumVariance )
    {
        // Get the new position of the block with random height
        Vector3 movePos = new Vector3(prevColumnPos.x, Random.Range(prevColumnPos.y - maximumVariance, prevColumnPos.y + maximumVariance), prevColumnPos.z + blockWidth);
        
        blockPool[columnIndex].transform.position = movePos;
        // Set new color
        SetColor(columnIndex);
        // How many blocks has been instantiated
        blockCounter++;
        // Clear the point until its time to add it.
        if (blockPool[columnIndex].transform.Find("Goal"))
        {
            Destroy(blockPool[columnIndex].transform.Find("Goal").gameObject);
        }
        // Add the point here
        if (blockCounter >= lastHeightUpperBlock)
        {
            GameObject point = Instantiate(pointPrefab);
            point.transform.parent = blockPool[columnIndex].transform;
            // Name the instantiated point
            point.name = "Goal";
            // Randomize height
            point.transform.localPosition = new Vector3(0, Random.Range(-3f, 3f), 0);
            // Reset counter
            blockCounter = 0;
        }
    }

    private void SetColor(int columnIndex)
    {
        // Increase the color change value
        colorChangeValue += (2f / 250f);
        if (colorChangeValue > 1f)
            colorChangeValue = 0;
        
        float saturation = .7f;
        // If it is an even number, change saturation
        if (columnIndex % 2 == 0)
        {
            saturation = .5f;
        }
        // Set each renderer with that color
        foreach (MeshRenderer renderer in blockPool[columnIndex].GetComponentsInChildren<MeshRenderer>())
        {
            renderer.material.color = Color.HSVToRGB(colorChangeValue, saturation, .7f);
        }
    }
}
