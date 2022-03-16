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
    private GameObject pointPrefab;

    [SerializeField]
    
    private GUIController guiController;
    private float score;
    private bool gameOver = false;

    [Header ("CustomizedVariables")]
    [SerializeField] Transform playerT;
    [SerializeField] float speed = 5;
    private bool isGameStarted;
    private Vector3 lineDestination;
    [Header ("GameVariables")]
    [SerializeField] FloatVariable scoreCounter;
    [SerializeField] FloatVariable highScoreCounter;

    

	void Start ()
    {
        BlockCreator.GetSingleton().Initialize(32, blockPrefab, pointPrefab,playerT);

        isGameStarted = false;
        lRenderer.enabled = true;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.up, out hit))
        {
            lineDestination = hit.point;
        }

        StartCoroutine(FirstSwing());               // We want the ball to start swinging just before the game starts

        FindRelativePosForHingeJoint(Vector3.up * 10);      //First attach the ball to the first upper block
	}
	
    public void FindRelativePosForHingeJoint(Vector3 blockPosition)
    {
        //Update the block position on this line in a proper way to Find Relative position for our blockPosition
        
        StartCoroutine(CreateHingeJoint(blockPosition));                 //Creates new joint
        
    }

    IEnumerator CreateHingeJoint(Vector3 blockPosition){
        
        hJoint = gameObject.AddComponent<HingeJoint>();
        hJoint.anchor = transform.InverseTransformPoint(blockPosition);         //gets blockPosition's local position
        lRenderer.SetPosition(1, hJoint.anchor);
        lRenderer.enabled = true;
        //Vector3.Cross makes possible for us to swing around the axis of the vectors.
        playerRigidbody.velocity = Vector3.Cross(-transform.right, (transform.position - lineDestination).normalized) * speed;
        yield return new WaitForSecondsRealtime(0.3f);
          

    }

    
    public void PointerDown()
    {
        Debug.Log("Pointer Down");
        //This function works once when player holds on the screen
        //FILL the behaviour here when player holds on the screen. You may or not call other functions you create here or just fill it here

        lRenderer.enabled = true;
        playerRigidbody.useGravity = false;


        if (!isGameStarted)
        {
            // When game starts FirstSwing Coroutine shouldn't be working
            isGameStarted = true;
            Destroy(hJoint);
        }
        else
        {
            // Ray the way we are going and up
            RaycastHit hit;
            
            if (Physics.Raycast(transform.position, (Vector3.forward + Vector3.up).normalized , out hit))
            {

                if(hit.collider.CompareTag("Block")){
                    
                    //If we hit a "Block" get that blocks JointPos and start creating our new hinge joint with a correct anchor
                    lineDestination = hit.collider.gameObject.GetComponent<BoxController>().FindRelativeJointPos();
                    FindRelativePosForHingeJoint(lineDestination);

                }
                
            }

            
        }
        
        
    }

    public void PointerUp()
    {
        Debug.Log("Pointer Up");
        //This function works once when player takes his/her finger off the screen
        //Fill the behaviour when player stops holding the finger on the screen.

        //When player stops touching the screen, destroy the joint and let the player fall
        hJoint = GetComponent<HingeJoint>();
        playerRigidbody.useGravity = true;
        lRenderer.enabled = false;
        foreach (HingeJoint item in GetComponents<HingeJoint>())
        {

            Destroy(item);                                              //BUGFIX: When touched quickly hingejoints are created but not destroyed
            
        }
        

        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag.Equals("Block") && !gameOver)
        {
            PointerUp(); //Finishes the game here to stoping holding behaviour
            gameOver = true;
            scoreCounter.SetValue(score);
            //If you know a more modular way to update UI, change the code below
            
            if(scoreCounter.Value > highScoreCounter.Value)
            {

                highScoreCounter.SetValue(scoreCounter.Value);
                    
            }
        
        }
            
            guiController.gameOverPanel.SetActive(true);
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag.Equals("Point"))
        {
            if(Vector3.Distance(transform.position, other.gameObject.transform.position) < .5f)
            {

                scoreCounter.Increase(10f);

            }
            else
            {

                scoreCounter.Increase(5f);

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
    {   if(gameOver) return;                    //BUGFIX: After game ends, score keeps going up or down
        score += playerRigidbody.velocity.z * Time.fixedDeltaTime * 0.1f;
        scoreCounter.SetValue(score);
    }

    private IEnumerator FirstSwing()
    {
        // Keep the speed value due to the fact that we will need it later
        float initialSpeed = speed;
        float elapsedTime = 0;
        while(!isGameStarted)
        {
            // Change speed value
            speed = Mathf.PingPong(elapsedTime * initialSpeed, initialSpeed * 2) - initialSpeed;
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        // This is the moment we need the speed value when we are done swinging
        speed = initialSpeed;
    }
}
