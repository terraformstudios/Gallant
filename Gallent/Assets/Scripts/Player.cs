using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player : NetworkBehaviour {
    [SyncVar] public int side = 0;
    [SyncVar] public bool BLOCK = false; //  make this variable true, for the ability block, and make it false at end of the turn
    [SyncVar] public float HEALTH = 10;
    [SyncVar] public bool IS_STUNNED = false;
    [SyncVar] public bool BLOCK_USED_B_WEAPONIZE = false; //block used before weaponize
    [SyncVar] public bool ApplySecondPhase_OWT = false; // apply second phase of oblivion waits
    [SyncVar] public bool DODGE_APPLIED_KR = false; // turn this variable true, when you apply dodge ability, dodge applied from killer reflexes

    public Text HealthText;
    public GameObject Sheild;
    public GameObject StunVisual;
    public GameObject DodgeVisual;
    public override void OnStartClient()
    {
        base.OnStartClient();
        string netID = GetComponent<NetworkIdentity>().netId.ToString();
        GameManager.instance.RegisterPlayer(netID, this);
        HEALTH = 10;
    }
    private void OnDisable()
    {
        GameManager.instance.UnRegisterPlayer(GetComponent<NetworkIdentity>().netId.ToString());
    }
    [Command] //this function should only be called by the local player
    public void Cmd_I_have_completed_phase_I()
    {// This method will run on server side
        Rpc_Acknowledge_PlayersManager_Remote_Player();
    }
    [ClientRpc]
    public void Rpc_Acknowledge_PlayersManager_Remote_Player() {
        PlayersManager.instance.Check_Phase1_Completion();
    }
    [Command] //this function should only be called by the local player, when ever he completes his action
    public void Cmd_I_have_completed_Action()
    {// This method will run on server side
        Rpc_Acknowledge_PlayersManager_Remote_Player_ActionCompleted();
    }
    [ClientRpc]
    public void Rpc_Acknowledge_PlayersManager_Remote_Player_ActionCompleted()
    {
        PlayersManager.instance.Check_Action_Completion();
    }
    [Command] //this function should only be called by the local player
    public void Cmd_AssignMyActionsToOpponentsSlots(int Idx1, int Idx2, int Idx3, int Idx4)
    {// This method will run on server side
        Rpc_AssignActionsToOpponentSlots(Idx1, Idx2, Idx3, Idx4);
    }
    [ClientRpc]
    public void Rpc_AssignActionsToOpponentSlots(int Idx1, int Idx2, int Idx3, int Idx4)
    {
        if (!isLocalPlayer)
            AssignActionsToBothOpponents(Idx1, Idx2, Idx3, Idx4);
    }
    // check the spawn count
    [Command]
    public void Cmd_BothPlayersAreReady() {
        Rpc_BothPlayersAreReady();
    }
    //inform my remote player, and update the players count
    [ClientRpc]
    public void Rpc_BothPlayersAreReady()
    {
        PlayersManager.instance.OnBothPlayersReady();
    }
    // send my name to server
    [Command]
    public void Cmd_SendMyNameToServer(string _name)
    {
        Rpc_SendMyNameToRemotePlayer(_name);
    }
    //inform my remote player, and update my name in the player's manager script, assign it to the opponent's name
    [ClientRpc]
    public void Rpc_SendMyNameToRemotePlayer(string _name)
    {
        if (!isLocalPlayer)
        {
            PlayersManager.instance.SetOpponentName(_name);
        }
    }
    // Send my BLOCK Info Server
    [Command]
    public void Cmd_SendBlockValueServer(bool value)
    {
        Rpc_SendBlockValueToRemoteClient(value);
    }
    //Inform my remote player, and update his block value
    [ClientRpc]
    public void Rpc_SendBlockValueToRemoteClient(bool value)
    {
        BLOCK = value;
        Sheild.SetActive(value);
        if (!isLocalPlayer) {
            PlayerController.instance.OpponentIsBlock = value;
        }
    }
    [Command]
    public void Cmd_SendDodgeValueTOServer(bool _bool) {
        Rpc_SendDodgeValueTOClient(_bool);
    }
    [ClientRpc]
    public void Rpc_SendDodgeValueTOClient(bool _bool) {
        DODGE_APPLIED_KR = _bool;
        DodgeVisual.SetActive(_bool);
        // inform the main player on the client side that i'm enabling/disabling my dodge
        if (!isLocalPlayer) {
            PlayerController.instance.OpponentDodgeIsActive = _bool;
        }
       
    }
    // Send Reduce health command to server
    [Command]
    public void Cmd_ReduceHealth(float damage) {
        Rpc_ReduceHealth(damage);
    }
    [ClientRpc]
    public void Rpc_ReduceHealth(float damage) {
        if (!isLocalPlayer) {
            PlayersManager.instance.ApplyDamagetoOpponent(damage);
        }
    }

    [Command]
    public void Cmd_StunOpponent(bool value) {
        Rpc_StunOpponent(value);
    }
    [ClientRpc]
    public void Rpc_StunOpponent(bool stunvalue) {
        if (!isLocalPlayer)
        {
            PlayersManager.instance.ApplyStuntoOpponent(true);
        }
        else if (isLocalPlayer) {
            PlayerController.instance.OpponentisStunned = true;
            PlayersManager.instance.ChangeTheOpponentSlotsToStun();
        }
    }
    // sync health across the network
    [Command]
    public void Cmd_SyncHealth(float health) {
        Rpc_SyncHealth(health);
    }
    [ClientRpc]
    public void Rpc_SyncHealth(float health) {
        HEALTH = health;
        HealthText.text = HEALTH.ToString();
    }
    // sync Stun value
    [Command]
    public void Cmd_SyncStun(bool stunvalue) {
        Rpc_SyncStun(stunvalue);
    }
    [ClientRpc]
    public void Rpc_SyncStun(bool stunvalue) {
        IS_STUNNED = stunvalue;
        StunVisual.SetActive(stunvalue);
    }

    // sync block used_before_weaponize_bool
    [Command]
    public void Cmd_SyncBlockusedBeforeWeaponize(bool _value) {
        Rpc_SyncBlockusedBeforeWeaponize(_value);
    }

    [ClientRpc]
    public void Rpc_SyncBlockusedBeforeWeaponize(bool _bool) {
        BLOCK_USED_B_WEAPONIZE = _bool;
        if (_bool)
        {
            // check if this is local player, check weaponize ability in player's action slots and change its priorities
            if (isLocalPlayer)
            {
                // check weaponize in player's action slots
                PlayersManager.instance.CheckWeaponizeInPlayerSlots();
            }
            // check if this is not local player, check weaponize ability in Opponent's action slots and change its priorities
            else if (!isLocalPlayer)
            {
                // Check weaponize in the Opponent's slots
                PlayersManager.instance.CheckWeaponizeInOpponentSlots();
            }
        }
    }

    // sync oblivion waits second phase
    [Command]
    public void Cmd_SyncApplySecondPhase_OWT(bool _value)
    {
        Rpc_SyncApplySecondPhase_OWT(_value);
    }

    [ClientRpc]
    public void Rpc_SyncApplySecondPhase_OWT(bool _bool)
    {
        ApplySecondPhase_OWT = _bool;
        if (!isLocalPlayer) {
            PlayerController.instance.OpponentOblivionWaits = _bool;
        }
    }
    // use this funciton when 2nd phase of oblivion waits complets
    [Command]
    public void Cmd_MoveToActionSelection() {
        Rpc_MoveToActionSelection();
    }
    [ClientRpc]
    public void Rpc_MoveToActionSelection()
    {
       // if(isLocalPlayer)
        PlayersManager.instance.MovetoNextAction(); 
    }
    //Assigning actions to each Opponent here
    public void AssignActionsToBothOpponents(int A1Idx, int A2Idx, int A3Idx, int A4Idx)
    {
        // clear the previous actions if any
        PlayersManager.instance.ClearPreviousActionSlotsOpenent();
        GameObject Action1 = Instantiate(PlayersManager.instance.ActionPrefabs[A1Idx]);
        Action1.transform.SetParent(PlayersManager.instance.OpponentAction1Slot.transform);
        Action1.GetComponent<RectTransform>().localScale = Vector3.one;
        Action1.GetComponent<RectTransform>().rotation = new Quaternion(0, 0, 0, 0);
        Action1.GetComponent<ActionIdentity>().SetPlayerProperties(PlayersManager.instance.OpponentPlayer);
        // Action1.GetComponent<ActionIdentity>().MakeMeInvisible();
        GameObject Action2 = Instantiate(PlayersManager.instance.ActionPrefabs[A2Idx]);
        Action2.transform.SetParent(PlayersManager.instance.OpponentAction2Slot.transform);
        Action2.GetComponent<RectTransform>().localScale = Vector3.one;
        Action2.GetComponent<RectTransform>().rotation = new Quaternion(0, 0, 0, 0);
        Action2.GetComponent<ActionIdentity>().SetPlayerProperties(PlayersManager.instance.OpponentPlayer);
        // Action2.GetComponent<ActionIdentity>().MakeMeInvisible();
        GameObject Action3 = Instantiate(PlayersManager.instance.ActionPrefabs[A3Idx]);
        Action3.transform.SetParent(PlayersManager.instance.OpponentAction3Slot.transform);
        Action3.GetComponent<RectTransform>().localScale = Vector3.one;
        Action3.GetComponent<RectTransform>().rotation = new Quaternion(0, 0, 0, 0);
        Action3.GetComponent<ActionIdentity>().SetPlayerProperties(PlayersManager.instance.OpponentPlayer);
        //  Action3.GetComponent<ActionIdentity>().MakeMeInvisible();
        GameObject Action4 = Instantiate(PlayersManager.instance.ActionPrefabs[A4Idx]);
        Action4.transform.SetParent(PlayersManager.instance.OpponentAction4Slot.transform);
        Action4.GetComponent<RectTransform>().localScale = Vector3.one;
        Action4.GetComponent<RectTransform>().rotation = new Quaternion(0, 0, 0, 0);
        Action4.GetComponent<ActionIdentity>().SetPlayerProperties(PlayersManager.instance.OpponentPlayer);
        // Action4.GetComponent<ActionIdentity>().MakeMeInvisible();

    }
}
