using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionIdentity : MonoBehaviour {
    public int Action_Id;
    public enum ActionCategory {
        Movment,
        Ability,
        Noaction
    }

    public enum ActionName {
        Left,
        Right,
        Forward,
        Back,
        Ability1,
        Ability2,
        Ability3,
        Noaction
    }

    public enum Ability {
        NONABILITY,
        NORMAL,
        STUN,
        DODGE,
        BLOCK,
        COUNTER,
        GRAB
    }

    public ActionCategory Category;
    public ActionName Action_name;
    public Ability _Ability = Ability.NONABILITY;
    public string AbilityName;
    public int CoolDown = 0;
    public List<string> PriorityList = new List<string>();

    public void MakeMeInvisible() {
        this.GetComponent<Image>().enabled = false;
    }

    public void MakeMeVisible() {
        this.GetComponent<Image>().enabled = true;
    }

    public void SetPlayerProperties(string PlayerName) {
        switch (PlayerName)
        {
            case "ARCHER":
                if (Action_name == ActionName.Ability1)
                {
                    AbilityName = "LightningArrows";
                    _Ability = Ability.NORMAL;
                    CoolDown = 0;
                    return;
                }
                if (Action_name == ActionName.Ability2)
                {
                    AbilityName = "KillerReflexes";
                    _Ability = Ability.DODGE;
                    CoolDown = 1;
                    PriorityList.Add("NORMAL");
                    PriorityList.Add("STUN");
                    PriorityList.Add("GRAB");
                    Debug.Log("add items to list: "+PriorityList.Count);
                    return;
                }
                if (Action_name == ActionName.Ability3)
                {
                    AbilityName = "LightningStrikesTwice";
                    _Ability = Ability.DODGE;
                    CoolDown = 2;
                    PriorityList.Add("NORMAL");
                    PriorityList.Add("STUN");
                    PriorityList.Add("GRAB");
                    Debug.Log("add items to list: " + PriorityList.Count);
                    return;
                }
                break;
            case "WARRIOR":
                if (Action_name == ActionName.Ability1) {
                    AbilityName = "Block";
                    _Ability = Ability.BLOCK;
                    CoolDown = 1;
                    PriorityList.Add("NORMAL");
                    PriorityList.Add("COUNTER");
                    PriorityList.Add("DODGE");
                    Debug.Log("add items to list: " + PriorityList.Count);
                    return;
                }
                if (Action_name == ActionName.Ability2)
                {
                    AbilityName = "Charge";
                    _Ability = Ability.STUN;
                    CoolDown = 2;
                    PriorityList.Add("NORMAL");
                    PriorityList.Add("GRAB");
                    PriorityList.Add("BLOCK");
                    Debug.Log("add items to list: " + PriorityList.Count);
                    return;
                }
                if (Action_name == ActionName.Ability3)
                {
                    AbilityName = "Weaponize";
                    _Ability = Ability.NORMAL;
                    CoolDown = 0;
                    return;
                }
                break;
            case "REAPER":
                if (Action_name == ActionName.Ability1)
                {
                    AbilityName = "ReapersReach";
                    _Ability = Ability.GRAB;
                    CoolDown = 2;
                    PriorityList.Add("NORMAL");
                    PriorityList.Add("COUNTER");
                    PriorityList.Add("BLOCK");
                    Debug.Log("add items to list: " + PriorityList.Count);
                    return;
                }
                if (Action_name == ActionName.Ability2)
                {
                    AbilityName = "CripplingSlash";
                    _Ability = Ability.NORMAL;
                    CoolDown = 1;
                    return;
                }
                if (Action_name == ActionName.Ability3)
                {
                    AbilityName = "Oblivionwaits";
                    _Ability = Ability.NORMAL;
                    CoolDown = 2;
                    return;
                }
                break;
        }
    }

    public void OnStun() {
        this.GetComponent<Image>().color = Color.red;
    }
    // change weaponize Priority if block is used befor weaponize
    public void ChangeWeaponizePriority() {
        AbilityName = "Weaponize";
        _Ability = Ability.COUNTER;
        CoolDown = 0;
        PriorityList.Add("NORMAL");
        PriorityList.Add("STUN");
        PriorityList.Add("DODGE");
    }
}
