﻿/*--------------------------------------*/
/*--ファイル名：StageDataController.cs--*/
/*--概要：ステージの詳細を描画する------*/
/*--------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   //UI

public class StageDataController : MonoBehaviour
{
    /*----------*/
    /*--public--*/
    /*----------*/
    //浄化率のテキストオブジェクト
    ////public GameObject PurificationRateTextPrefab;
    /*-----------*/
    /*--private--*/
    /*-----------*/
    //ドアオブジェクト
    private GameObject[] door_obj;
    ////テキストオブジェクト
    //private GameObject[] text_obj;
    //プレイヤー
    private PlayerController playerController;

    //ステージ番号のロゴ
    private Image image_stage_number_logo;
    //ステージ番号のテキスト
    private Text text_stage_number;
    //ステージ番号の汚れ
    private Image image_dirty_logo;
    //操作説明のロゴ
    private Image image_operation_logo;
    //ロゴの回転角度
    private float number_logo_angle;
    //操作説明の回転角度
    private float operation_logo_angle;
    //ロゴの回転向き
    private bool logo_angle_direction;

    /*------------------------------*/
    /*--関数名：Awake---------------*/
    /*--概要：Startより早い初期化---*/
    /*--引数：なし------------------*/
    /*--戻り値：なし----------------*/
    /*------------------------------*/
    private void Awake()
    {
        //**ステージのドア**//*************************************************************
        //シーン内にあるドアオブジェクトを全部探す
        door_obj = GameObject.FindGameObjectsWithTag("StageDoor");
        //ドアオブジェクトのリストを並べ変える(要素番号とステージ番号が合うように)
        //door_obj = SortDoorObjectList();
        //ステージデータの初期化をする(初期化していたら何もしない)
        SharedData.instance.SetStageDataSize(door_obj.Length);    //ドアオブジェクトの数だけ
    }

    /*-----------------*/
    /*--関数名：Start--*/
    /*--概要：初期化---*/
    /*--引数：なし-----*/
    /*--戻り値：なし---*/
    /*-----------------*/
    void Start()
    {
        //自身のCanvasの設定をする
        SharedData.instance.SetCanvasOption(GetComponent<Canvas>());
        //自身のCanvasScalerの設定をする
        SharedData.instance.SetCanvasScaleOption(GetComponent<CanvasScaler>());

        //**ステージ番号のロゴ**//*********************************************************
        //ステージ番号のロゴを探す
        image_stage_number_logo = GameObject.Find("StageNumberLogo").GetComponent<Image>();
        //ステージ番号のロゴの回転角度を設定する(最初は見えない)
        number_logo_angle = 90.0f;
        logo_angle_direction = false;   //プレイヤーがどのドアにも当たっていない
        image_stage_number_logo.GetComponent<RectTransform>().localRotation = Quaternion.Euler(number_logo_angle, 0.0f, 0.0f);

        //**ステージ番号のテキスト**//*****************************************************
        //ステージ番号のテキストを探す
        text_stage_number = GameObject.Find("StageNumber").GetComponent<Text>();
        //ステージ番号のテキストを初期化
        text_stage_number.text = "ステージ??";

        //**ステージ番号の汚れ**//*****************************************************
        //汚れを探す
        image_dirty_logo = GameObject.Find("LogoDirty").GetComponent<Image>();

        //**操作説明のロゴ**//*****************************************************
        image_operation_logo = GameObject.Find("SmallUILogo").GetComponent<Image>();
        //操作説明のロゴの回転角度を設定する(最初は見えない)
        operation_logo_angle = 110.0f;
        image_operation_logo.GetComponent<RectTransform>().localRotation = Quaternion.Euler(operation_logo_angle, 0.0f, 0.0f);


        playerController = GameObject.Find(Common.Player.NAME).GetComponent<PlayerController>();

        ////テキストオブジェクトのサイズを決める
        //text_obj = new GameObject[door_obj.Length];

        //Debug.Log(door_obj.Length);

        ////ドアオブジェクトの数だけテキストオブジェクトを作る
        //for (int i = 0; i < door_obj.Length; i++)
        //{
        //    //プレファブを元にテキストオブジェクトを作る
        //    GameObject text = Instantiate(PurificationRateTextPrefab) as GameObject;
        //    //ドアオブジェクトからステージ番号をもらう
        //    int stage_number = door_obj[i].GetComponent<DoorToStage>().GetStageNumber();
        //    //ステージ番号を使って浄化率をもらう
        //    int purification = SharedData.instance.GetPurification(stage_number);
        //    //テキスト内容を変える
        //    text.GetComponent<Text>().text = purification.ToString() + "%";
        //    //テキストオブジェクトを覚える
        //    text_obj[i] = text;
        //    //作ったテキストオブジェクトを子オブジェクトに登録する
        //    text.transform.parent = transform;
        //    //拡大率
        //    text_obj[i].GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
        //}

        ////ドアの色を変える
        //for (int i = 0; i < door_obj.Length; i++)
        //{
        //    //プレイできるものは赤色にできないものは黒色に
        //    if (SharedData.instance.GetCanPlay(door_obj[i].GetComponent<DoorToStage>().GetStageNumber()))
        //    {
        //        door_obj[i].GetComponent<SpriteRenderer>().color = Color.red;
        //    }
        //    else
        //    {
        //        door_obj[i].GetComponent<SpriteRenderer>().color = Color.black;
        //    }
        //}
    }
    /*--終わり：Start--*/

    /*------------------*/
    /*--関数名：Update--*/
    /*--概要：更新------*/
    /*--引数：なし------*/
    /*--戻り値：なし----*/
    /*------------------*/
    void Update()
    {
        logo_angle_direction = false;
        bool enableJump = true;
        //ドアの数だけ繰り返す
        for (int i = 0; i < door_obj.Length; i++)
        {
            //ドアにプレイヤーが当たっていたら
            if (door_obj[i].GetComponent<DoorToStage>().GetTouchPlayer())
            {
                logo_angle_direction = true;
                int stage_num = door_obj[i].GetComponent<DoorToStage>().GetStageNumber();
                text_stage_number.text = "第" + (stage_num).ToString() + "区画";
                //ステージ番号が二桁だったら
                //if (i>=10)
                //{
                //    text_stage_number.text = "ステージ" + door_obj[i].GetComponent<DoorToStage>().GetStageNumber().ToString();
                //}
                //ステージ番号が一桁だったら
                //else
                //{
                //    text_stage_number.text = "ステージ0" + door_obj[i].GetComponent<DoorToStage>().GetStageNumber().ToString();
                //}

                Color color = image_dirty_logo.color;
                //プレイできないステージだったら
                if (SharedData.instance.GetCanPlay(stage_num - 1) == false)
                {
                    //汚れを出す
                    image_dirty_logo.color = new Color(color.r, color.g, color.b, 1.0f);
                }
                else
                {
                    //汚れを出さない
                    image_dirty_logo.color = new Color(color.r, color.g, color.b, 0.0f);
                    enableJump = false;
                }

                break;
            }
        }

        //playerController.EnableJump(enableJump);

        //見えるように回転する時
        if (logo_angle_direction)
        {
            number_logo_angle -= 3.0f;
            //汚れを出していなかったら
            if (image_dirty_logo.color.a != 1.0f)
            {
                operation_logo_angle -= 3.0f;
            }
            //0.0度まで
            if (number_logo_angle < 0.0f)
            {
                number_logo_angle = 0.0f;
            }
            //0.0度まで
            if (operation_logo_angle < 0.0f)
            {
                operation_logo_angle = 0.0f;
            }
        }
        else
        {
            number_logo_angle += 3.0f;
            //汚れを出していなかったら
            if (image_dirty_logo.color.a != 1.0f)
            {
                //少しずらして戻す
                if (number_logo_angle > 20.0f)
                {
                    operation_logo_angle += 3.0f;
                }
            }
            //90.0度まで
            if (number_logo_angle > 90.0f)
            {
                number_logo_angle = 90.0f;
            }
            //110.0度まで
            if (operation_logo_angle > 110.0f)
            {
                operation_logo_angle = 110.0f;
            }
        }
        //回転更新
        image_stage_number_logo.GetComponent<RectTransform>().localRotation = Quaternion.Euler(number_logo_angle, 0.0f, 0.0f);
        image_operation_logo.GetComponent<RectTransform>().localRotation = Quaternion.Euler(operation_logo_angle, 0.0f, 0.0f);

        ////移動するドアのために毎回座標更新する
        //for (int i = 0; i < door_obj.Length; i++)
        //{
        //    text_obj[i].GetComponent<RectTransform>().position = new Vector3(door_obj[i].transform.position.x, door_obj[i].transform.position.y, text_obj[i].GetComponent<RectTransform>().position.z);
        //    text_obj[i].GetComponent<RectTransform>().position += new Vector3(0.0f, -2.0f, 0.0f);
        //}
    }
    /*--終わり：Update--*/

    /*--------------------------------------------------------*/
    /*--関数名：SortDoorObjectList(private)-------------------*/
    /*--概要：ドアオブジェクトをステージ番号の順に並べ変える--*/
    /*--引数：なし--------------------------------------------*/
    /*--戻り値：なし------------------------------------------*/
    /*--------------------------------------------------------*/
    private GameObject[] SortDoorObjectList()
    {
        //ドアの数分の配列を作る
        GameObject[] obj_list = new GameObject[door_obj.Length];

        //用意した配列の数だけ繰り返す
        for (int i = 0; i < obj_list.Length; i++)
        {
            //ドアの数だけ繰り返す
            for (int j = 0; j < door_obj.Length; j++)
            {
                //配列の要素数がドアのステージ番号とあっていたら
                if (i + 1 == door_obj[j].GetComponent<DoorToStage>().GetStageNumber())
                {
                    //配列に入れる
                    obj_list[i] = door_obj[j];
                    break;
                }
            }
        }

        return obj_list;
    }
    /*--終わり：SortDoorObjectList--*/
}