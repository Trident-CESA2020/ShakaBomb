﻿//==============================================================================================
/// File Name	: 
/// Summary		: 
//==============================================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Common;
using UnityEngine.UI;
//==============================================================================================
public partial class Floor : MonoBehaviour
{
    //------------------------------------------------------------------------------------------
    // member variable
    //------------------------------------------------------------------------------------------
    
    // 最初・最後のポジション
    [SerializeField]
    protected Vector3 startPosition;
    [SerializeField]
    protected Vector3 endPosition;
    [SerializeField]
    protected float speed;

    [SerializeField]
    private float radius;
    [SerializeField]
    private bool rightOrLeft;
    [SerializeField]
    private float second;
    [SerializeField]
    FloorStatus floorStatus;

    protected GameObject thisObj;

    private Floor currentObj;
    private NormalFloor normalFloor;
    private MoveFloor moveFloor;
    private RotationFloor rotationFloor;
    private CircleRotationFloor circleRotationFloor;
    private GenerateFloor generateFloor;
    private RideOnFloor rideOnFloor;
    private FallFloor fallFloor;

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
        thisObj = this.gameObject;

        switch (floorStatus)
        {
            case FloorStatus.Normal:
                normalFloor = new NormalFloor();
                currentObj = normalFloor;
                break;
            case FloorStatus.Move:
                moveFloor = new MoveFloor(this.gameObject, startPosition, endPosition);
                moveFloor.speed = speed;
                currentObj = moveFloor;
                break;
            case FloorStatus.RidoOn:
                rideOnFloor = new RideOnFloor(this.gameObject, startPosition, endPosition,second);
                currentObj = rideOnFloor;
                break;
            case FloorStatus.CircleRotation:
                circleRotationFloor = new CircleRotationFloor(this.gameObject, radius, speed, rightOrLeft);
                currentObj = circleRotationFloor;
                break;
            case FloorStatus.Fall:
                fallFloor = new FallFloor(this.gameObject, startPosition, endPosition,second);
                currentObj = fallFloor;
                break;
            case FloorStatus.Rotation:
                rotationFloor = new RotationFloor(this.gameObject, second);
                currentObj = rotationFloor;
                break;
            case FloorStatus.Generate:
                generateFloor = new GenerateFloor(this.gameObject);
                currentObj = generateFloor;
                break;
            default:
                break;
        }
    }

    //------------------------------------------------------------------------------------------
    // Update
    //------------------------------------------------------------------------------------------
    private void Update()
    {
        currentObj.Execute();
    }
}