using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreTextController : MonoBehaviour
{

    [SerializeField] FloatReference scoreCounter;
    Text scoreText;

    private void Start() {
        
        scoreText = GetComponent<Text>();

    }
    

    void Update()
    {
        
        scoreText.text = scoreCounter.Value.ToString("0.00");

    }
}
