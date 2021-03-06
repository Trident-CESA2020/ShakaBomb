﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFindRangeController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        //プレイヤーだったら
        if(collision.tag=="Player")
        {
            transform.GetComponentInParent<EnemyController>().SetFindPlayer(true);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        //プレイヤーだったら
        if (collision.tag == "Player")
        {
            transform.GetComponentInParent<EnemyController>().SetFindPlayer(false);
        }
    }
}
