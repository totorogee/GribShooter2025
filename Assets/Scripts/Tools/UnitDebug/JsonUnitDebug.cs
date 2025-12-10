using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameTool;
using Newtonsoft.Json;

[System.Serializable]
public class JsonDataTest
{
    public string EventName;
    public int Int;
    public float Float = 123.2f;
    public bool Bool;
    public Vector2 Vector2;
    public Vector2Int Vector2Int;
    public Vector3 Vector3;
    public Vector3Int Vector3Int;
}

public class JsonUnitDebug : MonoBehaviour
{
    [SerializeField] public JsonDataTest test;

    // Start is called before the first frame update
    void Start()
    {
        // Apply custom JSON settings for Unity Vector types
        JsonHelper.ApplyCustomSetting();
        
        JsonDataTest jsonData = new JsonDataTest
        {
            Int = 1,
            Bool = true,
            Vector2Int = new Vector2Int(123, 135),
            Vector3 = new Vector3(1, 1, 1),
        };

        string temp = JsonConvert.SerializeObject(jsonData);

        Debug.LogWarning(temp);
        jsonData = JsonConvert.DeserializeObject<JsonDataTest>(temp);
        Debug.LogWarning(JsonConvert.SerializeObject(jsonData));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
