﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class WormController : MonoBehaviour {
    public float moveSpeed;
    private float verticalSpeed = 1;
    private float xSpeed;
    private float ySpeed;
    public float depth;   //1 is on the ground, 0 is in shllow underground, -1 is in deep underground
    private float initialDepth = -1;
    private float minDeapth = -1;
    private float maxDeapth = 1;
    private GameObject levelManager;
    private bool testAbility = false;
    private float maxHp = 200;
    // private float maxHpAfterReset = 102; //after reset, 
    private float hp;
    private Scrollbar healthBar;
    public bool onground=false;
    public bool isActive;
    private float eatDistance = 1;
    public float loseHpByTimeOnground = 2f;
    public float loseHpByTimeUnderGround = 1f;

    private bool isGamePlaying;
    public float eatNpcLost = 30;


    //worm body fields
    public float speed = 0.1f;
    public float elastifactor = 0.3f; // To stop it from scrunching up when it stops! Higher value = less scrunchy
    public Transform bod1;
    public Transform bod2;
    public Transform bod3;
    
    public float[] pos = { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f }; // *** SYNC VARIABLE OVER NETWORK **

    //skills cool down
    public List<Skill> skills;

    //private bool upCooldown;
    //private bool downCooldown;

    public void reset() {
        //hp = maxHp + loseHpByTime; //becasue after reset, the hp bar won't update before the HP change
        hp = maxHp;
        //set health bar value
        GameObject healthBarObject = GameObject.FindGameObjectWithTag("HealthBar");
        healthBar = healthBarObject.GetComponent<Scrollbar>();
        healthBar.size = 1f;
        //loseHP(loseHpByTime);
        depth = initialDepth;
        onground = false;

        isGamePlaying = true;

        //reset lost hp
        InvokeRepeating("hpLostByTime", 1, 1);

        //reset blur
        levelManager = GameObject.FindGameObjectWithTag("LevelManager");
        levelManager.GetComponent<LevelManager>().setBlur(this.depth);

        //reset cool down
        foreach (Skill s in skills)
        {
            //minus 0.1 so that currentCoolDown < coolDown, then in update it will change the icon cooldown
            s.currentCoolDown = s.cooldown - 0.1f;
        }
    }

    // Use this for initialization

    void Start()
    {
        
        isGamePlaying = true;
        hp = maxHp;
        GameObject healthBarObject = GameObject.FindGameObjectWithTag("HealthBar");
        healthBar = healthBarObject.GetComponent<Scrollbar>();
        levelManager = GameObject.FindGameObjectWithTag("LevelManager");

        GameObject skillDown= GameObject.FindGameObjectWithTag("skill_down");
        Image downImage = skillDown.GetComponent<Image>();
        skills[0].skillIcon = downImage;

        GameObject skillUp = GameObject.FindGameObjectWithTag("skill_up");
        Image upImage = skillUp.GetComponent<Image>();
        skills[1].skillIcon = upImage;

        GameObject testAbility = GameObject.FindGameObjectWithTag("locate");
        Image abilityImage = testAbility.GetComponent<Image>();
        skills[2].skillIcon = abilityImage;

        GameObject eatAbility = GameObject.FindGameObjectWithTag("eat");
        Image eatImage = eatAbility.GetComponent<Image>();
        skills[3].skillIcon = eatImage;

        setupSegPositions();

        isActive = gameObject.GetComponent<WormController>().isActiveAndEnabled;
        //InvokeRepeating("hpLostByTime", 1, 1);

        reset();
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        //transform.Translate (new Vector3 (h * speed, v * speed));

        transform.position = new Vector3((transform.position.x + (h * speed)), (transform.position.y + (v * speed)));

        if (Mathf.Abs(h) > elastifactor || Mathf.Abs(v) > elastifactor)
        {
            pos[6] = pos[4];
            pos[4] = pos[2];
            pos[2] = pos[0];
            //}

            //if (Mathf.Abs(v)>elastifactor) {
            pos[7] = pos[5];
            pos[5] = pos[3];
            pos[3] = pos[1];
        }

        pos[1] = transform.position.y;
        pos[0] = transform.position.x;

        updateWormSegPositions();

        /* if (v != 0.0f) {
			v = v / Mathf.Abs(v);
			Debug.Log (v);
			GetComponent<Rigidbody2D> ().AddForce (transform.up.normalized * speed * v);
		}
		transform.Rotate(0, 0, -h * speed * Time.deltaTime * 90); */

    }

    void setupSegPositions()
    {
        pos[0] = transform.position.x;
        pos[1] = transform.position.y;
        pos[2] = bod1.position.x;
        pos[3] = bod1.position.y;
        pos[4] = bod2.position.x;
        pos[5] = bod2.position.y;
        pos[6] = bod3.position.x;
        pos[7] = bod3.position.y;
    }

    public void gameOver() {
        isGamePlaying = false;
        //cancel lost hp when game over
        CancelInvoke("hpLostByTime");

    }

    void updateWormSegPositions()
    {
        bod1.position = new Vector3(pos[2], pos[3]);
        bod2.position = new Vector3(pos[4], pos[5]);
        bod3.position = new Vector3(pos[6], pos[7]);
    }

    // Update is called once per frame
    void Update()
    {
        if (depth < 1 || !onground)
        {
            onground = false;
            //CancelInvoke("hpLostByTime");
        }

        //down
        if (Input.GetButtonDown("Fire1"))
        {
            //if not cool down and not in min deapth
            if ((skills[0].currentCoolDown >= skills[0].cooldown) && (depth > minDeapth)) {

                depth -= verticalSpeed;                    
                //set blur
                levelManager.GetComponent<LevelManager>().setBlur(this.depth);

                //set skill cooldown
                skills[0].currentCoolDown = 0;

            }
        }

        //up
        if (Input.GetButtonDown("Fire2"))
        {

        //if not cool down and not < maxdeapth
            if ((skills[1].currentCoolDown >= skills[1].cooldown) && (depth < maxDeapth))
            {
                depth += verticalSpeed;
                //set blur
                levelManager.GetComponent<LevelManager>().setBlur(this.depth);


                //set skill cooldown
                skills[1].currentCoolDown = 0;
            }

            //if on the ground, lost health
            //only set when worm is under ground
            if (depth == 1 && !onground)
            {
                onground = true;
                
            }
        }

        if (Input.GetButtonDown("Jump"))
        {


            if ((skills[2].currentCoolDown >= skills[2].cooldown))
            {

                //test ability
                ability();

                //set skill cooldown
                skills[2].currentCoolDown = 0;

            }

        }

        if (Input.GetButtonDown("Fire3"))
        {
            bool eatSuccess = false;
            if ((skills[3].currentCoolDown >= skills[3].cooldown))
            {
                eatSuccess = eat();
                //set skill cooldown

                if (eatSuccess) {
                    skills[3].currentCoolDown = 0;
                }
                
            }         
        }


        
        foreach (Skill s in skills)
        {
            if (s.currentCoolDown < s.cooldown)
            {
                s.currentCoolDown += Time.deltaTime;

                s.skillIcon.fillAmount = s.currentCoolDown / s.cooldown;
            }
        }

    }

    public void ability()
    {
        //gameObject.GetComponent<PlayerSync>().testAbility();
        transform.parent.gameObject.GetComponent<PlayerSync>().testAbility();
    }

    //lose hp and set health bar
    public void loseHP(float damage) {
        if (isGamePlaying)
        {
            hp = hp - damage;
            if (hp < 0)
            {
                hp = 0;
            }

            if (hp <= 0)
            {
                groundPlayerWin();
            }

            //set health bar value
            healthBar.size = hp / maxHp;
        }
    }

    //worm's hp lose along with time
    private void hpLostByTime() {
        if (depth == 1)
        {
            //onground
            this.loseHP(loseHpByTimeOnground);
        }
        else if(depth == 0)
        {
            this.loseHP(loseHpByTimeUnderGround);
        }
        
    }

    private bool eat()
    {
        bool eatSuccess = false;
        // eat player
        float distanceWithPlayer = Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("GroundPlayer").transform.position);
        if (distanceWithPlayer < eatDistance)
        {
            eatSuccess = true;
            wormWin();            
        }

        //eat npc
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("EnemyAnim");
        foreach (GameObject npc in npcs)
        {
            float distance = Vector3.Distance(transform.position, npc.transform.position);
            if (distance < eatDistance)
            {
                npc.GetComponent<npcController>().kill();
                loseHP(eatNpcLost);
                eatSuccess = true;
                break;
            }
        }

        return eatSuccess;
    }

    public void wormWin()
    {
        //gameObject.GetComponent<PlayerSync>().testAbility();
        transform.parent.gameObject.GetComponent<PlayerSync>().setWormWin(true);
    }

    public void groundPlayerWin()
    {
        //gameObject.GetComponent<PlayerSync>().testAbility();
        transform.parent.gameObject.GetComponent<PlayerSync>().setWormWin(false);
    }
}


[System.Serializable]
public class Skill {
    public float cooldown;

    // [HideInInspector]
    public float currentCoolDown;


    [HideInInspector]
    public Image skillIcon;
}