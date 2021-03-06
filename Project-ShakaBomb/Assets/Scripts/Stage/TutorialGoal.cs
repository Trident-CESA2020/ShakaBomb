﻿//==============================================================================================
/// File Name	: TutorialGoal.cs
/// Summary		: 
//==============================================================================================
using UnityEngine;
using Common;
//==============================================================================================
public class TutorialGoal : MonoBehaviour
{
    //------------------------------------------------------------------------------------------
    // member variable
    //------------------------------------------------------------------------------------------
    [SerializeField]
    PlayDirector playDirector = null;
    private bool once = false;



    //------------------------------------------------------------------------------------------
    // summary : ゴールにプレイヤーが触れた
    // remarks : none
    // param   : none
    // return  : none
    //------------------------------------------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (once)
        {
            return;
        }

        if (col.tag == ConstPlayer.NAME)
        {
            playDirector.TutorialGoal();
        }
    }
}
