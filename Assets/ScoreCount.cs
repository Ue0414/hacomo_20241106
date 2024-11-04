using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCount : MonoBehaviour
{

    public Text Scoretext;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        multiMarker multimarker = GetComponent<multiMarker>();
        string count = (string)multimarker.countList.Count.ToString();
        Scoretext.text = count;
    }
}
