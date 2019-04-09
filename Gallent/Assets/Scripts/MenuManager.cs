using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    public void SelectPlayerBtn(string _name) {
        SelectedPlayerManager.instance.SelectPlayer(_name);
        SceneManager.LoadScene("scene1");
    }
}
