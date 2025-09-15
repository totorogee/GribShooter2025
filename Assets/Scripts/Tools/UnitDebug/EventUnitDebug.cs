using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameTool;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using UnityEditor;
using GameTool.Hex;

public class EventUnitDebug : MonoBehaviour
{
    void Start()
    {
        JsonHelper.ApplyCustomSetting();
        EventManager.StartListening("Null" , TestNull) ;
        EventManager.StartListening<JsonDataTest , JsonDataTest>("TestJson", TestJsonData);
        EventManager.StartListening<string , int>("testST", TestST);
        EventManager.StartListening<int>("testint", TestInt);
        EventManager.StartListening<bool>("testbool", TestBool);
        EventManager.StartListening<bool,int>("testboolint", TestBoolInt);
        EventManager.StartListening<Vector2>("Vect", TestVect);
        EventManager.StartListening<Direction9>("TestDirection9", TestDirection9);
    }

    public void TestNull()
    {
        Debug.LogWarning("TestNull ");
    }

    public void TestJsonData(JsonDataTest input , JsonDataTest input2)
    {
        Debug.LogWarning("TinputJson " + input.Float + " " + input2.Float);
    }

    public void TestDirection9(Direction9 input)
    {
        Debug.LogWarning("TDirection9 " + input);
    }

    public void TestBool(bool input)
    {
        Debug.LogWarning("TB " + input);
    }

    public void TestInt(int input)
    {
        Debug.LogWarning("TI " + input);
    }

    public void TestBoolInt( bool input1 , int input2)
    {
        Debug.LogWarning("TBI " + input1 + " " + input2);
    }

    public void TestVect(Vector2 vector2)
    {
        Debug.LogWarning("TV " + vector2 );
    }

    public void TestST(string input1 , int input2)
    {
        Debug.LogWarning("TestST " + input1 + " " + input2);
    }
}