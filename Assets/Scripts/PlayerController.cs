using UnityEngine;
using System.Collections;


//In this section, you have to edit OnPointerDown and OnPointerUp sections to make the game behave in a proper way using hJoint
//Hint: You may want to Destroy and recreate the hinge Joint on the object. For a beautiful gameplay experience, joint would created after a little while (0.2 seconds f.e.) to create mechanical challege for the player
//And also create fixed update to make score calculated real time properly.
//Update FindRelativePosForHingeJoint to calculate the position for you rope to connect dynamically
//You may add up new functions into this class to make it look more understandable and cosmetically great.

public class PlayerController : MonoBehaviour {

    [SerializeField]
    private GameObject blockPrefab;
    [SerializeField]
    private HingeJoint hJoint;
    [SerializeField]
    private LineRenderer lRenderer;
    [SerializeField]
    private Rigidbody playerRigidbody;
    [SerializeField]
    private PlayerFollower playerFollower;
    [SerializeField]
    private GameObject pointPrefab;

    [SerializeField]
    
    private GUIController guiController;
    private float score;
    private bool gameOver = false;

    [Header ("CustomizedVariables")]
    [SerializeField] Transform playerT;
    [SerializeField] GameObject WholeBlockPrefab;
    [SerializeField] float speed = 20;
    private bool m_bIsSwinging;
    private bool m_bHasStarted;
    private Vector3 m_RopeDest;
    private bool isPointerUp;

    

	void Start ()
    {
        BlockCreator.GetSingleton().Initialize(32, blockPrefab, pointPrefab,playerT);
        // BlockCreator.GetSingleton().Initialize(16, WholeBlockPrefab, pointPrefab,this.gameObject.transform);
        m_bHasStarted = false;//
        m_bIsSwinging = false;
        lRenderer.enabled = true;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.up, out hit))
        {
            m_RopeDest = hit.point;
        }
        // Start swinging
        StartCoroutine(InitialSwing());

        FindRelativePosForHingeJoint(new Vector3(0,10,0),null);
	}
	
    public void FindRelativePosForHingeJoint(Vector3 blockPosition, Rigidbody blockRigidbody)
    {
        //Update the block position on this line in a proper way to Find Relative position for our blockPosition
        hJoint = gameObject.AddComponent<HingeJoint>();
        hJoint.anchor = transform.InverseTransformPoint(blockPosition);
        hJoint.connectedBody = blockRigidbody;
        lRenderer.SetPosition(1, hJoint.anchor);
        lRenderer.enabled = true;
        playerRigidbody.velocity = Vector3.Cross(-transform.right, (transform.position - m_RopeDest).normalized) * speed;
    }

    
    public void PointerDown()
    {
        Debug.Log("Pointer Down");
        //This function works once when player holds on the screen
        //FILL the behaviour here when player holds on the screen. You may or not call other functions you create here or just fill it here

        m_bIsSwinging = true;
        lRenderer.enabled = true;
        playerRigidbody.useGravity = false;


        if (!m_bHasStarted)
        {
            // InitialSwing coroutine is watching this.
            // mark HasStarted to discontinue swing
            m_bHasStarted = true;
            Destroy(hJoint);
        }
        else
        {
            // Cast a ray to forward/up
            RaycastHit hit;
            
            if (Physics.Raycast(transform.position, (Vector3.forward + Vector3.up).normalized , out hit))
            {

                if(hit.collider.CompareTag("Block")){
                    Debug.Log(hit.collider.gameObject);

                    m_RopeDest = hit.collider.gameObject.GetComponent<BlockController>().GetAnchorPosition();
                    Rigidbody blockRigidbody = hit.collider.gameObject.GetComponent<Rigidbody>();
                    
                    FindRelativePosForHingeJoint(m_RopeDest,blockRigidbody);

                }
                // Record where to shoot line, our latest pivot point
                
            }

            
        }
        isPointerUp = false;
        
        // if (m_bIsSwinging)
        // {
            
        // // Draw the line
        //     lRenderer.positionCount = 2;
        //     RaycastHit hit;
            
        //     if (Physics.Raycast(transform.position, transform.forward + transform.up, out hit))
        //     {   
        //         // Record where to shoot line, our latest pivot point
        //         m_RopeDest = hit.point;
        //         FindRelativePosForHingeJoint(m_RopeDest);
        //     }
        //     // lRenderer.SetPosition(0, transform.position);
        //     // lRenderer.SetPosition(1, m_RopeDest);

        //     // Push the rigidbody around the pivot point
        //     // playerRigidbody.velocity = Vector3.Cross(-transform.right, (transform.position - m_RopeDest).normalized) * speed;
        //     // playerRigidbody.AddForce(Vector3.forward * speed);
        // }

        
        
    }

    public void PointerUp()
    {
        Debug.Log("Pointer Up");
        //This function works once when player takes his/her finger off the screen
        //Fill the behaviour when player stops holding the finger on the screen.
        hJoint = GetComponent<HingeJoint>();
        m_bIsSwinging = false;
        playerRigidbody.useGravity = true;
        lRenderer.enabled = false;
        Destroy(hJoint);
        isPointerUp = true;
        

        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag.Equals("Block") && !gameOver)
        {
            PointerUp(); //Finishes the game here to stoping holding behaviour
            gameOver = true;
            guiController.scoreText.text = score.ToString("0.00");
            //If you know a more modular way to update UI, change the code below
            if(PlayerPrefs.HasKey("HighScore"))
            {
                float highestScore = PlayerPrefs.GetFloat("HighScore");
                if(score > highestScore)
                {
                    PlayerPrefs.SetFloat("HighScore", score);
                    guiController.highscoreText.text = "HighestScore: " + score.ToString("0.00");
                }
                else
                {
                    guiController.highscoreText.text = "HighestScore: " + highestScore.ToString("0.00");
                }
            }
            else
            {
                PlayerPrefs.SetFloat("HighScore", score);
                guiController.highscoreText.text = "HighestScore: " + score.ToString("0.00");
            }
            guiController.gameOverPanel.SetActive(true);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag.Equals("Point"))
        {
            if(Vector3.Distance(transform.position, other.gameObject.transform.position) < .5f)
            {
                score += 10f;
            }
            else
            {
                score += 5f;
            }
            other.gameObject.SetActive(false);
        }
    }
    private void FixedUpdate()
    {
       //Score doesn't set properly since it always tend to update the score. Make a proper way to update the score as player advances

       SetScore();
        
    }
    public void SetScore()
    {
        score += playerRigidbody.velocity.z * Time.fixedDeltaTime * 0.1f;
        guiController.realtimeScoreText.text = score.ToString("0.00");
    }

    private IEnumerator InitialSwing()
    {
        // Remember the original speed value
        float initialSpeed = speed;
        float elapsedTime = 0;
        while(!m_bHasStarted)
        {
            // Change speed value
            speed = Mathf.PingPong(elapsedTime * initialSpeed, initialSpeed * 2) - initialSpeed;
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        // Done swinging. Restore speed value
        speed = initialSpeed;
    }
}
