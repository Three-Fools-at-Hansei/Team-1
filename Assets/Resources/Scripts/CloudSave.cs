using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.CloudSave.Models.Data.Player;
using SaveOptions = Unity.Services.CloudSave.Models.Data.Player.SaveOptions;
using System.Collections.Generic;


public class CloudSave
{


    public async void SaveData(string PlayerId)
    {
        var playerData = new Dictionary<string, object>{
          {"PlayerId", PlayerId},
        };
        //var playerData = new Dictionary<string, object>{
        //  {"firstKeyName", "a text value"},
        //  {"secondKeyName", 123}
        //};
        await CloudSaveService.Instance.Data.Player.SaveAsync(playerData);
        Debug.Log($"Saved data {string.Join(',', playerData)}");
    }

    public async void LoadData()
    {
        var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> {
          "firstKeyName", "secondKeyName"
        });

        if (playerData.TryGetValue("firstKeyName", out var firstKey)) {
            Debug.Log($"firstKeyName value: {firstKey.Value.GetAs<string>()}");
        }

        if (playerData.TryGetValue("secondKeyName", out var secondKey)) {
            Debug.Log($"secondKey value: {secondKey.Value.GetAs<int>()}");
        }
    }





    //// Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}
}











