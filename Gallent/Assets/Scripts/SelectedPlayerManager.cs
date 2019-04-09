using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedPlayerManager : MonoBehaviour {
    public static SelectedPlayerManager instance;
    public string SelectedPlayerName;
  
    // Use this for initialization
    void Awake () {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }
    public void SelectPlayer(string _playername) {
        SelectedPlayerName = _playername;
    }
}
