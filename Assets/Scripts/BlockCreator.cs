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
        // Record lane width, which allows accurate positioning
        blockWidth = blockPrefab.transform.localScale.z;
        // Creat list of columns
        for (int i = 0; i < blockCount; i++)
        {
            // First one exists, just add it and set it's color
            if (i == 0)
            {
                blockPool.Add(blockPrefab);
                SetColor(0);
            }
            // Others are created & added. Color is set by UpdateBlockPosition
            else
            {
                GameObject newColumn = Instantiate(blockPrefab);
                newColumn.transform.parent = blockPrefab.transform.parent;
                blockPool.Add(newColumn);
                UpdateBlockPosition(blockPool.Count - 1, blockPool[blockPool.Count - 2].transform.position, 1);
            }
        }
        // Start hue value, increment for each new row
        colorChangeValue = 0;

        // Reference indexes for the newest and oldest columns. We move the oldest, and put it next to the newest.
        oldestBlockIndex = 0;
        newestBlockIndex = blockPool.Count - 1;
    }

    // Use this for initialization
    void Start () {
        // // Record lane width, which allows accurate positioning
        // blockWidth = blockPrefab.transform.localScale.z;
        // // Creat list of columns
        // for (int i = 0; i < blockCount; i++)
        // {
        //     // First one exists, just add it and set it's color
        //     if (i == 0)
        //     {
        //         blockPool.Add(blockPrefab);
        //         SetColor(0);
        //     }
        //     // Others are created & added. Color is set by UpdateBlockPosition
        //     else
        //     {
        //         GameObject newColumn = Instantiate(blockPrefab);
        //         newColumn.transform.parent = blockPrefab.transform.parent;
        //         blockPool.Add(newColumn);
        //         UpdateBlockPosition(blockPool.Count - 1, blockPool[blockPool.Count - 2].transform.position, 1);
        //     }
        // }
        // // Start hue value, increment for each new row
        // colorChangeValue = 0;

        // // Reference indexes for the newest and oldest columns. We move the oldest, and put it next to the newest.
        // oldestBlockIndex = 0;
        // newestBlockIndex = blockPool.Count - 1;
	}
	
	// Update is called once per frame
	void Update (){
        // Check if the ball position has move a full column unit. If so, move the oldest to the next available position. Happens off screen.
        
		if (playerTransform.position.z > lastPlayerZ + blockWidth)
        {
            UpdateBlockPosition(oldestBlockIndex, blockPool[newestBlockIndex].transform.position, difficulty);
            
            // Update newest and oldest rows.
            newestBlockIndex = oldestBlockIndex;
            oldestBlockIndex = (oldestBlockIndex + 1) % blockPool.Count;
            lastPlayerZ += blockWidth;
        }
	}

    private void UpdateBlockPosition(int columnIndex, Vector3 prevColumnPos, float maximumVariance )
    {
        // Get the desired position, one column width downstream from the latest column
        Vector3 movePos = new Vector3(prevColumnPos.x, Random.Range(prevColumnPos.y - maximumVariance, prevColumnPos.y + maximumVariance), prevColumnPos.z + blockWidth);
        // Move to that new position
        blockPool[columnIndex].transform.position = movePos;
        // Set new color (with increment hue)
        SetColor(columnIndex);
        // Keep track of how many rows are generated since the last target.
        blockCounter++;
        // If the row contains a target, remove it.
        if (blockPool[columnIndex].transform.Find("Goal"))
        {
            Destroy(blockPool[columnIndex].transform.Find("Goal").gameObject);
        }
        // If it's time to add a target, do so.
        if (blockCounter >= lastHeightUpperBlock)
        {
            GameObject target = Instantiate(pointPrefab);
            target.transform.parent = blockPool[columnIndex].transform;
            // Name it specifically so we can detect it for removal later.
            target.name = "Goal";
            // Randomize height
            target.transform.localPosition = new Vector3(0, Random.Range(-5f, 5f), 0);
            // Reset counter
            blockCounter = 0;
        }
    }

    private void SetColor(int columnIndex)
    {
        // increment the hue, looping back to 0 if necessary
        colorChangeValue += (1f / 250f);
        if (colorChangeValue > 1f)
            colorChangeValue = 0;
        // Default saturation
        float saturation = .7f;
        // Alternative saturation for even numbers
        if (columnIndex % 2 == 0)
        {
            saturation = .5f;
        }
        // Set color for each child's renderer
        foreach (MeshRenderer renderer in blockPool[columnIndex].GetComponentsInChildren<MeshRenderer>())
        {
            renderer.material.color = Color.HSVToRGB(colorChangeValue, saturation, .7f);
        }
    }
}
