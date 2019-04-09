using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	public static GameManager instance;
    public List<Vector3> Terrain_Coordinates;
    public GameObject Action1Slot, Action2Slot, Action3Slot, Action4Slot;// assign actions slots from the action selection panal
    public GameObject ActionSelectionPanal;
	public Text InfoText;
	public Dictionary<string, Player> Players = new Dictionary<string, Player>();
	public Transform spawnPoint1, spawnPoint2;
	public GameObject[] Player1StartPositions, Player2StartPositions;
    public Color OrignalColor, HighlightColor; // initial positions orignal colors
    public GameObject PlayerActions, OpponentActions;
    // use this delegate and event when both players spawned
    void Awake(){
		instance = this;
		Dictionary<string, Player> Players = new Dictionary<string, Player>();
	}
	// Use this for initialization
	void Start () {
		Initialize ();
	}
	void Initialize(){
        // assign white color to the initial position boxes
        for (int i = 0; i < Player1StartPositions.Length; i++){
			Player1StartPositions [i].GetComponent<MeshRenderer> ().material.color = OrignalColor;
			Player2StartPositions [i].GetComponent<MeshRenderer> ().material.color = OrignalColor;
		}
	}
	// Register new Player, add him to the dictionary and give him a proper name
	public void RegisterPlayer(string _netID, Player _player)
	{
		if(Players.Count>= 2){
			Destroy (_player.gameObject);
			return;
		}
		Players.Add(_netID, _player);
		_player.transform.name = "Player" + _netID;
		Debug.Log(Players.Count);
		if (_player.transform.name == "Player1") {
			_player.side = 1;
            
            if (_player.GetComponent<PlayerController> () != null)
				for(int i = 0; i < Player1StartPositions.Length;i++){
					_player.GetComponent<PlayerController> ().InitialPositions[i] = Player1StartPositions[i];
				//}
			}
			_player.transform.position = spawnPoint1.position;
			
			_player.GetComponentInChildren<MeshRenderer> ().materials [0].color = Color.red;
		} else {
			_player.side = -1;
            if (_player.GetComponent<PlayerController> () != null)
				for(int i = 0; i < Player2StartPositions.Length;i++){
					_player.GetComponent<PlayerController> ().InitialPositions[i] = Player2StartPositions[i];
				}
            _player.transform.rotation = Quaternion.Euler(0, 180, 0);
            _player.transform.position = spawnPoint2.position;
			_player.GetComponentInChildren<MeshRenderer> ().materials [0].color = Color.blue;
		}
		if(Players.Count == 2){
			SelectInitialPosition (); // Phase-1 select initial position
		}
	}
	public void UnRegisterPlayer(string _playerID)
	{
		Players.Remove(_playerID);
	}
	// Phase-1 Initial point selection
	void SelectInitialPosition(){
		foreach(Player P in Players.Values){
			if(P.GetComponent<PlayerController>() != null){
				P.GetComponent<PlayerController> ().SelectInitialPosition ();
			}
		}
		
	}
    // Open Action selection Panel
    public void OpenActionSelectionPanel() {
        ActionSelectionPanal.SetActive(true);
    }
    // check if other player exists at the next location
    public bool OtherPlayerExists(Vector3 target) {
        foreach (Player _player in Players.Values) {
            if (_player.transform.position == target) {
                return true;
            }
        }
        return false;
    }
    // check if both players moved to same location, if yes cancel the movement
    public bool DeadLockOccured() {
        Player[] bothplayers = new Player[2];
        int i = 0;
        foreach (Player _player in Players.Values) {
            bothplayers[i] = _player;
            if (i < 1) {
                i++;
            }
        }
        if (bothplayers[0].transform.position == bothplayers[1].transform.position)
        {
            return true;
        }
        else {
            return false;
        }

    }
    // change ui in case of left side player, side -1
    public void SwapActionPositions() {
        for (int i = 0; i < PlayerActions.transform.childCount; i++) {
            PlayerActions.transform.GetChild(i).transform.rotation = Quaternion.Euler(0,0,-90);
            OpponentActions.transform.GetChild(i).transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        Vector3 Temp;
        Temp = PlayerActions.transform.position;
        PlayerActions.transform.position = OpponentActions.transform.position;
        OpponentActions.transform.position = Temp;
    }
    // Make the players Face each other
    public void ChecKRotationofPlayers() {
        // Geting both the palyers and storing them into array
        GameObject[] players;
        players = new GameObject[Players.Count];
        int i = 0;
        foreach (Player _player in Players.Values)
        {
            players[i] = _player.gameObject;
            i++;
        }
        //chek if both are located in the same row
        if (PlayersLiesInSameRow(players[0],players[1])) {
            if (players[0].transform.position.z < players[1].transform.position.z)
            { // check which one lies in the lower value colum
                players[0].transform.rotation = Quaternion.Euler(0,180,0);
                players[1].transform.rotation = Quaternion.Euler(0,0,0);
            }else if (players[0].transform.position.z > players[1].transform.position.z)
            { 
                players[1].transform.rotation = Quaternion.Euler(0, 180, 0);
                players[0].transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            return;
        }
        // check if both lies in the same column
        if (PlayersLiesInSameColumn(players[0], players[1]))
        {
            if (players[0].transform.position.x < players[1].transform.position.x)
            { // check which one lies in the lower value colum
                players[0].transform.rotation = Quaternion.Euler(0, 270, 0);
                players[1].transform.rotation = Quaternion.Euler(0, 90, 0);
            }
            else if (players[0].transform.position.x > players[1].transform.position.x)
            {
                players[0].transform.rotation = Quaternion.Euler(0, 90, 0);
                players[1].transform.rotation = Quaternion.Euler(0, 270, 0);
            }
            return;
        }

        // if not the above two cases, then use only front and back direction: hilal
        if (players[0].transform.position.z < players[1].transform.position.z)
        { // check which one lies in the lower value colum
            players[0].transform.rotation = Quaternion.Euler(0, 180, 0);
            players[1].transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (players[0].transform.position.z > players[1].transform.position.z)
        {
            players[1].transform.rotation = Quaternion.Euler(0, 180, 0);
            players[0].transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        return;

    }
    // This funciton will return true if both the players lies in the same row
    public bool PlayersLiesInSameRow(GameObject Player1, GameObject Player2) {
        if (Player1.transform.position.x == Player2.transform.position.x)
        {
            return true;
        }
        else {
            return false;
        }
    }
    // This funciton will return true if both the players lies in the same colum
    public bool PlayersLiesInSameColumn(GameObject Player1, GameObject Player2)
    {
        if (Player1.transform.position.z == Player2.transform.position.z)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
