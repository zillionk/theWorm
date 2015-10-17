﻿using UnityEngine;
using System.Collections;

public class OngroundAlter : MonoBehaviour
{

    private bool findPlayer = false;
    public GameObject player;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //int thisZ = this.transform.position.z;
        float playerDepth = player.GetComponent<testPlayer>().depth;
        //Debug.Log("playerDepth");
        if (findPlayer && playerDepth == 0) {
            Debug.Log("ongrond alert");
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {

        // If it hits an enemy...
        if (col.tag == "Player")
        {
            //Debug.Log("found player");
            findPlayer = true;
            //this.die();
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {

        // If it hits an enemy...
        if (col.tag == "Player")
        {
            //Debug.Log("player left");
            findPlayer = false;
            //this.die();
        }
    }
}
