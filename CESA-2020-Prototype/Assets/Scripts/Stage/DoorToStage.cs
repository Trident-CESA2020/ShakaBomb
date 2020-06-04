﻿//==============================================================================================
/// File Name	: 
/// Summary		: 
//==============================================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Common;
using UnityEngine.SceneManagement;
//==============================================================================================
public class DoorToStage : MonoBehaviour
{
    //------------------------------------------------------------------------------------------
    // member variable
    //------------------------------------------------------------------------------------------

    // 対応するステージ番号
    [SerializeField]
    int numStage;

    float goStage = Common.Decimal.ZERO;

	//------------------------------------------------------------------------------------------
    // Awake
	//------------------------------------------------------------------------------------------
    private void Awake()
    {
        
    }

	//------------------------------------------------------------------------------------------
    // Start
	//------------------------------------------------------------------------------------------
    private void Start()
    {
        
    }

	//------------------------------------------------------------------------------------------
    // Update
	//------------------------------------------------------------------------------------------
	private void Update()
    {
        goStage = Input.GetAxis(GamePad.BUTTON_A);
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == Player.NAME && goStage > 0.0f)
        {
            // ToDo:静的な変数に代入
            Data.stage_number = numStage;
            SceneManager.LoadScene("PlayScene");
        }
    }

    //扉のステージ番号を渡す
    public int GetStageNumber()
    {
        return numStage;
    }
}
