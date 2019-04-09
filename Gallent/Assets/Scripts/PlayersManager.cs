using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayersManager : NetworkBehaviour {
    public static PlayersManager instance;
    public List<GameObject> AllBoxes;
    public Text PlayersNameTxt, OpponenetNameTxt;
    public Text PlayerHealthText, OpponentHealthText;
    public GameObject[] ActionPrefabs;
    public GameObject PlayerAction1Slot, PlayerAction2Slot, PlayerAction3Slot, PlayerAction4Slot;
    public GameObject OpponentAction1Slot, OpponentAction2Slot, OpponentAction3Slot, OpponentAction4Slot;
    [SyncVar] public int Phase1count = 0; // use this counter for acknowledgement of stat-1 completion
    [SyncVar] public int ActionAckcount = 0; // use this variable for acknowledgment of action completion by both the players
    public Text ActionNameText;
  
    public string SelectedPlayer = "",OpponentPlayer = "";// it can be ARCHER,WARRIOR,REAPER
    private void Awake()
    {
        instance = this;  
    }
    private void Start()
    {
        SelectedPlayer = SelectedPlayerManager.instance.SelectedPlayerName;
        PlayersNameTxt.text = SelectedPlayer;
    }
    private void Update()
    {
        //if(ActionAckcount >0)
        //AckCountText.text += ActionAckcount.ToString()+"\n";
    }
    public delegate void Phase1Delegate();
    public event Phase1Delegate Phase1Completed;// Trigger this event on phase-1 completion
    // call this function to check the status of phase-1 (completed or not), 
    public void Check_Phase1_Completion() {
        Phase1count++;
        if (Phase1count >= 2) {
            if (Phase1Completed != null) {
                Phase1Completed();
            }
        }
    }
    // call this function to check the status of Current action of both players (completed or not), 
    public void Check_Action_Completion()
    {
        ActionAckcount++;
        Debug.Log("Action ack count: "+ActionAckcount);
        if (ActionAckcount >= 2)
        {

            PlayerController.instance.MovetoNextAction();
//
                ActionAckcount = 0; //  re-initialize for next action
           
        }
        else {
            Invoke("OnWaitDone",1);
        }
    }
    // Trigger this event on oblivion waits complets
    public void MovetoNextAction() {
        PlayerController.instance.MovetoNextAction();
            ActionAckcount = 0; //  re-initialize for next action
        
    }
    // invoke this funciton after 1 seconds
    public void OnWaitDone() {
        PlayerController.instance.OnWaitingDone();
        GameManager.instance.InfoText.text = "inform waiting player";
    }
    // Trigger action choosing done event Here
    public void TriggerActionChoosingDoneEvent()
    {
        PlayerController.instance.OnChoosingActionDone();
    }
    // Clear all the action slots (Player's Slots)
    public void ClearPreviousActionSlotsPlayer() {
        if (PlayerAction1Slot.transform.childCount > 0) {
            Destroy(PlayerAction1Slot.transform.GetChild(0).gameObject);
        }
        if (PlayerAction2Slot.transform.childCount > 0)
        {
            Destroy(PlayerAction2Slot.transform.GetChild(0).gameObject);
        }
        if (PlayerAction3Slot.transform.childCount > 0)
        {
            Destroy(PlayerAction3Slot.transform.GetChild(0).gameObject);
        }
        if (PlayerAction4Slot.transform.childCount > 0)
        {
            Destroy(PlayerAction4Slot.transform.GetChild(0).gameObject);
        }
    }
    // Clear all the action slots (Opponent's Slots)
    public void ClearPreviousActionSlotsOpenent()
    {
        if (OpponentAction1Slot.transform.childCount > 0)
        {
            Destroy(OpponentAction1Slot.transform.GetChild(0).gameObject);
        }
        if (OpponentAction2Slot.transform.childCount > 0)
        {
            Destroy(OpponentAction2Slot.transform.GetChild(0).gameObject);
        }
        if (OpponentAction3Slot.transform.childCount > 0)
        {
            Destroy(OpponentAction3Slot.transform.GetChild(0).gameObject);
        }
        if (OpponentAction4Slot.transform.childCount > 0)
        {
            Destroy(OpponentAction4Slot.transform.GetChild(0).gameObject);
        }
    }
    // set the opponent name
    public void SetOpponentName(string _name) {
        OpponentPlayer = _name;
        OpponenetNameTxt.text = OpponentPlayer;
    }
    // call this function on player spawan
    public void OnBothPlayersReady() {
        PlayerController.instance.SendMyNameToServer();
    }
    // public void ApplyDamage to the opponent
    public void ApplyDamagetoOpponent(float damage) {
        PlayerController.instance.ApplyDamage(damage);
    }
    // apply stun to opponent here
    public void ApplyStuntoOpponent(bool value) {
        PlayerController.instance.ApplySTUN(value);
        ChangeThePlayerSlotsToStun();
    }
    // Get box by name
    public GameObject GetBoxByName(string _name) {
        for (int i = 0; i < AllBoxes.Count; i++)
        {
            if (AllBoxes[i].name == _name)
            {
                return AllBoxes[i];
            }

        }
        return null;
    }

    // make the opponent action slots look like stun (changing color for now)
    public void ChangeTheOpponentSlotsToStun() {
        OpponentAction1Slot.GetComponentInChildren<ActionIdentity>().OnStun();
        OpponentAction2Slot.GetComponentInChildren<ActionIdentity>().OnStun();
        OpponentAction3Slot.GetComponentInChildren<ActionIdentity>().OnStun();
        OpponentAction4Slot.GetComponentInChildren<ActionIdentity>().OnStun();
    }

    // make the Player action slots look like stun (changing color for now)
    public void ChangeThePlayerSlotsToStun()
    {
        PlayerAction1Slot.GetComponentInChildren<ActionIdentity>().OnStun();
        PlayerAction2Slot.GetComponentInChildren<ActionIdentity>().OnStun();
        PlayerAction3Slot.GetComponentInChildren<ActionIdentity>().OnStun();
        PlayerAction4Slot.GetComponentInChildren<ActionIdentity>().OnStun();
    }

    public void CheckWeaponizeInPlayerSlots() {
        if (PlayerAction2Slot.GetComponentInChildren<ActionIdentity>().AbilityName == "Weaponize")
        {
            PlayerAction2Slot.GetComponentInChildren<ActionIdentity>().ChangeWeaponizePriority();
            return;
        }
        else if (PlayerAction3Slot.GetComponentInChildren<ActionIdentity>().AbilityName == "Weaponize")
        {
            PlayerAction3Slot.GetComponentInChildren<ActionIdentity>().ChangeWeaponizePriority();
            return;
        }
        else if (PlayerAction4Slot.GetComponentInChildren<ActionIdentity>().AbilityName == "Weaponize")
        {
            PlayerAction4Slot.GetComponentInChildren<ActionIdentity>().ChangeWeaponizePriority();
            return;
        }
    }
    public void CheckWeaponizeInOpponentSlots()
    {
        if (OpponentAction2Slot.GetComponentInChildren<ActionIdentity>().AbilityName == "Weaponize")
        {
            OpponentAction2Slot.GetComponentInChildren<ActionIdentity>().ChangeWeaponizePriority();
            return;
        }
        else if (OpponentAction3Slot.GetComponentInChildren<ActionIdentity>().AbilityName == "Weaponize")
        {
            OpponentAction3Slot.GetComponentInChildren<ActionIdentity>().ChangeWeaponizePriority();
            return;
        }
        else if (OpponentAction4Slot.GetComponentInChildren<ActionIdentity>().AbilityName == "Weaponize")
        {
            OpponentAction4Slot.GetComponentInChildren<ActionIdentity>().ChangeWeaponizePriority();
            return;
        }
    }
}
