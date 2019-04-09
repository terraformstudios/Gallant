using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
     
// this script will only be used for the local player, and should be destroyed for RPC's
public class PlayerController : NetworkBehaviour {
    public static PlayerController instance;
    #region Global Variables
    //Game states
    enum GameState {
        normal,
        Phase1_initialposition_selection,
        Phase2_Select_Actions,
        Phase3_Apply_Actions,// apply all the actions those are choosen by each player
        phase4
    }
    public Text NameText;
    // Actions states these will be used in phase-3(Action playing State)
    public enum Action_States {
        Play_Action1,
        Play_Action2,
        Play_Action3,
        Play_Action4
    }
    public GameObject[] InitialPositions;
    private bool Select_Initial_Position_Bool = false;
    private bool Waiting = false;
    public bool Phase2_once_bool = true; // make this bool true whenever move to phase-2
    /// <summary>
    /// use these bools if you want the task to be done once during the specified action phase
    public bool playaction4Once = true;
    public bool playaction1Once = true;
    public bool playaction2Once = true;
    public bool playaction3Once = true;
    public Player myPlayer;
    /// </summary>
    private bool MovementInProgress = false;
    private ActionIdentity CurrentActionPlayer; // store the current action for the waiting player
    public bool OpponentisStunned = false;
    public bool OpponentOblivionWaits = false;
    public bool OpponentDodgeIsActive = false;
    public bool OpponentIsBlock = false;
    private void OnEnable()
    {
        PlayersManager.instance.Phase1Completed += OnPhase1Completion;
 
       
    }
    private void OnDisable()
    {
        PlayersManager.instance.Phase1Completed -= OnPhase1Completion;
       
    }
    GameState CurrentState; // this will store the current state of the player
    public Action_States CurrentActionState;
    private bool CheckPlayersCount = true; // check how many players instantiated on the server
    #endregion
    void Start() {
        myPlayer = this.GetComponent<Player>();
        if (!isLocalPlayer) {
            NameText.text = "OPPONENT";
            myPlayer.HealthText = PlayersManager.instance.OpponentHealthText;
            myPlayer.HealthText.text = myPlayer.HEALTH.ToString();
            Destroy(this);
        }
        else {
            instance = this;
            myPlayer.HealthText = PlayersManager.instance.PlayerHealthText;
            myPlayer.HealthText.text = myPlayer.HEALTH.ToString();
            NameText.text = "YOU";
        }
        CurrentActionState = Action_States.Play_Action1;// initialize from the first action always
    }
    // Update is called once per frame
    void Update() {
        // use this code to ensure that both the players are instantiated
        if (CheckPlayersCount) {
            if (isServer) {
                //Debug.Log("Total players: "+NetworkServer.connections.Count);
                if (NetworkServer.connections.Count == 2) {
                   Invoke( "BothPlayersAreReady",2);
                    CheckPlayersCount = false;
                }
            }
        }
        //hadling different states of the game
        switch (CurrentState)
        {
            case GameState.Phase1_initialposition_selection:
                if (Select_Initial_Position_Bool) {
                    Ray myray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Input.GetMouseButtonDown(0)) {
                        if (Physics.Raycast(myray, out hit, 1000)) {
                            for (int i = 0; i < InitialPositions.Length; i++) {
                                Debug.Log(InitialPositions[i].name);
                                if (hit.collider.name == InitialPositions[i].transform.name) {
                                    Debug.Log("Initial Phase done");
                                    StartCoroutine(MoveToTargetPosition(5, this.gameObject, InitialPositions[i].gameObject));
                                    myPlayer.Cmd_I_have_completed_phase_I();//:Tell PlayersManager that i'have completed the first phase
                                    Select_Initial_Position_Bool = false; // Avoid clicking on the other box
                                    break;
                                }
                            }

                        }

                    }

                }
                break;
            case GameState.Phase2_Select_Actions:
                // these tasks to be done once in phase-2
                if (Phase2_once_bool) {
                    StartCoroutine(OpenActionPanalCorutine());// open the action selection panal
                    Phase2_once_bool = false;
                }
                break;
            case GameState.Phase3_Apply_Actions:
                switch (CurrentActionState)
                {
                    case Action_States.Play_Action1:
                        if (playaction1Once)
                        {
                            //Debug.Log("Action1 started");
                            // check current action of player and opponent
                            CheckCurrentAction(PlayersManager.instance.PlayerAction1Slot.GetComponentInChildren<ActionIdentity>(), PlayersManager.instance.OpponentAction1Slot.GetComponentInChildren<ActionIdentity>());
                            PlayersManager.instance.OpponentAction1Slot.GetComponentInChildren<ActionIdentity>().MakeMeVisible();// make the opponent's action visible

                            playaction1Once = false;
                        }
                        break;
                    case Action_States.Play_Action2:
                        if (playaction2Once)
                        {
                            //Debug.Log("Action2 started");
                            // check current action of player and opponent
                            CheckCurrentAction(PlayersManager.instance.PlayerAction2Slot.GetComponentInChildren<ActionIdentity>(), PlayersManager.instance.OpponentAction2Slot.GetComponentInChildren<ActionIdentity>());
                            PlayersManager.instance.OpponentAction2Slot.GetComponentInChildren<ActionIdentity>().MakeMeVisible();// make the opponent's action visible
                            playaction2Once = false;
                        }
                        break;
                    case Action_States.Play_Action3:
                        if (playaction3Once)
                        {
                           // Debug.Log("Action3 started");
                            // check current action of player and opponent
                            CheckCurrentAction(PlayersManager.instance.PlayerAction3Slot.GetComponentInChildren<ActionIdentity>(), PlayersManager.instance.OpponentAction3Slot.GetComponentInChildren<ActionIdentity>());
                            PlayersManager.instance.OpponentAction3Slot.GetComponentInChildren<ActionIdentity>().MakeMeVisible();// make the opponent's action visible
                            playaction3Once = false;
                        }
                        break;
                    case Action_States.Play_Action4:
                        if (playaction4Once)
                        {
                           // Debug.Log("Action4 started");
                            // check current action of player and opponent
                            CheckCurrentAction(PlayersManager.instance.PlayerAction4Slot.GetComponentInChildren<ActionIdentity>(), PlayersManager.instance.OpponentAction4Slot.GetComponentInChildren<ActionIdentity>());
                            PlayersManager.instance.OpponentAction4Slot.GetComponentInChildren<ActionIdentity>().MakeMeVisible();// make the opponent's action visible
                            playaction4Once = false;
                        }
                        break;
                }
                break;


        }
    }
    // Phase-1 this activity goes here, selection of initital position
    public void SelectInitialPosition() {
        CurrentState = GameState.Phase1_initialposition_selection; // change Player state to initial state
        StartCoroutine(SelectInitialPosition_Corutine());
    }
    // Start the first phase actions
    IEnumerator SelectInitialPosition_Corutine() {
        GameManager.instance.InfoText.text = "Select your initial position in the highlighted squares";
        yield return new WaitForSeconds(1f);
        Highlight_Initial_Positions();

    }
    // Highlight the inition position boxes for both the players
    void Highlight_Initial_Positions() {
        Color C,H;
        C = GameManager.instance.OrignalColor;
        H = GameManager.instance.HighlightColor;
        Select_Initial_Position_Bool = true;
        for (int i = 0; i < InitialPositions.Length; i++) {
            StartCoroutine(LerpColor(5f, C, H, InitialPositions[i].GetComponent<MeshRenderer>().material));
        }
    }
    // use this funcion for smoothly transition of color
    IEnumerator LerpColor(float lerpDuration, Color startColor, Color endColor, Material Mat)
    {
        float StartTime = 0f;
        while (StartTime < lerpDuration) {
            Mat.color = Color.Lerp(startColor, endColor, StartTime / lerpDuration);
            StartTime += Time.deltaTime * 4f;
            yield return null;
        }
    }
    // Move the current game object to the selected target position
    IEnumerator MoveToTargetPosition(float lerpDuration, GameObject source, GameObject Target)
    {
        float StartTime = 0f;
        while (StartTime < lerpDuration)
        {
            source.transform.position = Vector3.MoveTowards(source.transform.position, Target.transform.position, StartTime / lerpDuration);
            StartTime += Time.deltaTime * 4f;
            yield return null;
        }
    }
    // Perform this action on phase1 completion
    public void OnPhase1Completion() {
        for (int i = 0; i < InitialPositions.Length; i++)
        {
            InitialPositions[i].GetComponent<MeshRenderer>().material.color = GameManager.instance.OrignalColor;
        }
        GameManager.instance.InfoText.text = "Phase 1 completed";
        CurrentState = GameState.Phase2_Select_Actions; // change Player state to phase2 action selection state

        // Set my actions UI
        if(myPlayer.side == -1)// if player is on the left side then shift my actions ui to the left
        {
            CheckActionsUI();
        }
        
        Phase2_once_bool = true; // :this bool is used for peroforming actions once in Phase-2
    }
    // use this function for swapping the ui of left side player : side -1
    public void CheckActionsUI()
    {
        GameManager.instance.SwapActionPositions();
    }
    // Perform this action on Choosing action Done
    public void OnChoosingActionDone() {
        AssignActionsToBothPlayers();
        //Change game state to Phase-3, Applying actions state
        // <always call these things combine when switing to phase3>
       
        playaction1Once = true;
        playaction2Once = true;
        playaction3Once = true;
        playaction4Once = true;
        Invoke("MoveToActionPlayingPhase_3",1f);
        // </ always call these things combine when switing to phase3>
    }
    // Move to phase 3 action playing state
    void MoveToActionPlayingPhase_3() {
        CurrentState = GameState.Phase3_Apply_Actions;
        CurrentActionState = Action_States.Play_Action1;
    }
    //Assigning actions to each player here
    public void AssignActionsToBothPlayers() {
        int A1Idx, A2Idx, A3Idx, A4Idx;
        if (GameManager.instance.Action1Slot.GetComponentInChildren<ActionIdentity>() != null) // if slot is not empty
        {
            A1Idx = GameManager.instance.Action1Slot.GetComponentInChildren<ActionIdentity>().Action_Id;
        }
        else {
            A1Idx = 7;// if slot is empty assign it no action
        }

        if (GameManager.instance.Action2Slot.GetComponentInChildren<ActionIdentity>() != null) // if slot is not empty
        {
            A2Idx = GameManager.instance.Action2Slot.GetComponentInChildren<ActionIdentity>().Action_Id;
        }
        else
        {
            A2Idx = 7;// if slot is empty assign it no action
        }

        if (GameManager.instance.Action3Slot.GetComponentInChildren<ActionIdentity>() != null) // if slot is not empty
        {
            A3Idx = GameManager.instance.Action3Slot.GetComponentInChildren<ActionIdentity>().Action_Id;
        }
        else
        {
            A3Idx = 7;// if slot is empty assign it no action
        }
        if (GameManager.instance.Action4Slot.GetComponentInChildren<ActionIdentity>() != null) // if slot is not empty
        {
            A4Idx = GameManager.instance.Action4Slot.GetComponentInChildren<ActionIdentity>().Action_Id;
        }
        else
        {
            A4Idx = 7;// if slot is empty assign it no action
        }
        // clear the previous actions if any
        PlayersManager.instance.ClearPreviousActionSlotsPlayer();
        GameObject Action1 = Instantiate(PlayersManager.instance.ActionPrefabs[A1Idx]);
        Action1.transform.SetParent(PlayersManager.instance.PlayerAction1Slot.transform);
        Action1.GetComponent<RectTransform>().rotation = new Quaternion(0, 0, 0, 0);
        Action1.GetComponent<RectTransform>().localScale = Vector3.one;
        Action1.GetComponent<ActionIdentity>().SetPlayerProperties(PlayersManager.instance.SelectedPlayer);
        GameObject Action2 = Instantiate(PlayersManager.instance.ActionPrefabs[A2Idx]);
        Action2.transform.SetParent(PlayersManager.instance.PlayerAction2Slot.transform);
        Action2.GetComponent<RectTransform>().localScale = Vector3.one;
        Action2.GetComponent<RectTransform>().rotation = new Quaternion(0, 0, 0, 0);
        Action2.GetComponent<ActionIdentity>().SetPlayerProperties(PlayersManager.instance.SelectedPlayer);
        GameObject Action3 = Instantiate(PlayersManager.instance.ActionPrefabs[A3Idx]);
        Action3.transform.SetParent(PlayersManager.instance.PlayerAction3Slot.transform);
        Action3.GetComponent<RectTransform>().localScale = Vector3.one;
        Action3.GetComponent<RectTransform>().rotation = new Quaternion(0, 0, 0, 0);
        Action3.GetComponent<ActionIdentity>().SetPlayerProperties(PlayersManager.instance.SelectedPlayer);
        GameObject Action4 = Instantiate(PlayersManager.instance.ActionPrefabs[A4Idx]);
        Action4.transform.SetParent(PlayersManager.instance.PlayerAction4Slot.transform);
        Action4.GetComponent<RectTransform>().localScale = Vector3.one;
        Action4.GetComponent<RectTransform>().rotation = new Quaternion(0,0,0,0);
        Action4.GetComponent<ActionIdentity>().SetPlayerProperties(PlayersManager.instance.SelectedPlayer);
        myPlayer.Cmd_AssignMyActionsToOpponentsSlots(A1Idx, A2Idx, A3Idx, A4Idx);

    }
    // open action panel after a little wait
    IEnumerator OpenActionPanalCorutine() {
        GameManager.instance.InfoText.text = "Get ready for actions selection, and choose your actions ";
        yield return new WaitForSeconds(4);
        GameManager.instance.OpenActionSelectionPanel();// open action panal
        GameManager.instance.InfoText.text = "Choose your actions within 30 seconds";

    }
    // Check the current action of the player and Opponent
    public void CheckCurrentAction(ActionIdentity _playerAction, ActionIdentity _opponentAction) {
        PlayersManager.instance.ActionNameText.text = "Player Action: <color=red>" + _playerAction.Category.ToString() + "</color>" + "  Opponent Action: <color=green>" + _opponentAction.Category.ToString() + "</color>";
        if (!myPlayer.IS_STUNNED) {
            // if both have the movement action, both should be played at the same time
            if ((_playerAction.Category == ActionIdentity.ActionCategory.Movment)
                && (_opponentAction.Category == ActionIdentity.ActionCategory.Movment)) {
                PlayMovementAction(_playerAction.Action_name.ToString(), transform.rotation.eulerAngles.y);
                return;
            }
            // if both have no-action, then move to the next action phase directly
            if ((_playerAction.Category == ActionIdentity.ActionCategory.Noaction)
                && (_opponentAction.Category == ActionIdentity.ActionCategory.Noaction))
            {
                I_hv_Completed_my_Action();
                return;
            }

            // if the player chooses no action & opponent chooses movement, then move to the next action phase directly
            if ((_playerAction.Category == ActionIdentity.ActionCategory.Noaction)
                && (_opponentAction.Category == ActionIdentity.ActionCategory.Movment))
            {
                I_hv_Completed_my_Action();
                return;
            }

            // if the player chooses Movement and opponent no-action, enable movement for player 
            if ((_playerAction.Category == ActionIdentity.ActionCategory.Movment)
                && (_opponentAction.Category == ActionIdentity.ActionCategory.Noaction))
            {
                PlayMovementAction(_playerAction.Action_name.ToString(), transform.rotation.eulerAngles.y);
                return;
            }

            // if Player chooses ability and oppoenent chooses movement or no-action, then player should go first
            if ((_playerAction.Category == ActionIdentity.ActionCategory.Ability)
               && ((_opponentAction.Category == ActionIdentity.ActionCategory.Noaction)
               || (_opponentAction.Category == ActionIdentity.ActionCategory.Movment)))
            {
                // Calling the ability function by name, to play the current ability
                PlayAbilityByName(_playerAction.AbilityName);
                return;
            }

            // if Player movement or no-action, and opponent chooses an Ability then player should go first
            if ((_opponentAction.Category == ActionIdentity.ActionCategory.Ability)
               && ((_playerAction.Category == ActionIdentity.ActionCategory.Noaction)
               || (_playerAction.Category == ActionIdentity.ActionCategory.Movment)))
            {
                // Write code for player to Wait untill the opponent completes its action
                Waiting = true;// goto the waiting state, and wait for the other player to complete his action
                GameManager.instance.InfoText.text = "Wait for the other player to complete his action: (your priority is low)";
                // store the current action of the player
                CurrentActionPlayer = _playerAction;
                return;
            }
            // if the both chooses an ability, than check priority and play the action according to the priorities 
            if ((_playerAction.Category == ActionIdentity.ActionCategory.Ability)
                && (_opponentAction.Category == ActionIdentity.ActionCategory.Ability))
            {
                Debug.Log(_playerAction._Ability+",,,"+_opponentAction._Ability);
                string PriorityValue = CheckPriority(_playerAction, _opponentAction);// first parameter must be player's action and vice versa
                GameManager.instance.InfoText.text = PriorityValue;
                switch (PriorityValue) {
                    case "First":
                        // in this case player need to play his abilty, call playability by name
                        PlayAbilityByName(_playerAction.AbilityName);
                        break;
                    case "Second":
                        // in this case, player needs to wait for opponent
                        Waiting = true;// goto the waiting state, and wait for the other player to complete his action
                        GameManager.instance.InfoText.text = "Wait for the other player to complete his action: (your priority is low)";
                        // store the current action of the player
                        CurrentActionPlayer = _playerAction;
                        break;
                    case "Equal":
                        // in this case player need to play his abilty, NO one will go to the waiting state
                        PlayAbilityByName(_playerAction.AbilityName);
                        break;
                }

                return;
            }
        }
        else if (myPlayer.IS_STUNNED)
        {
            I_hv_Completed_my_Action();
        }
        else if(OpponentisStunned)
        { // ignor priority in this case, just play your actions
            //  movement action
            if (_playerAction.Category == ActionIdentity.ActionCategory.Movment)
            {
                PlayMovementAction(_playerAction.Action_name.ToString(), transform.rotation.eulerAngles.y);
                return;
            }
            //  have no-action
            if (_playerAction.Category == ActionIdentity.ActionCategory.Noaction)
            {
                I_hv_Completed_my_Action();
                return;
            }

            // if Player chooses ability and oppoenent chooses movement or no-action, then player should go first
            if (_playerAction.Category == ActionIdentity.ActionCategory.Ability)
            {
                // Calling the ability function by name, to play the current ability
                PlayAbilityByName(_playerAction.AbilityName);
                return;
            }
        }
    }
    // Perform the movement action here
    public void PlayMovementAction(string Movement_name,float angle) {               
        if (!MovementInProgress)
        {
            float row = transform.position.x;
            float col = transform.position.z;
            if (angle == 0)
            {
                switch (Movement_name)
                {
                    case "Left":
                        if (MovementPossible(new Vector3(row + 1, 0, col)))
                        {
                            StartCoroutine(DoMovement(5, new Vector3(row + 1, 0, col), new Vector3(row, 0, col)));
                        }
                        break;
                    case "Right":
                        if (MovementPossible(new Vector3(row - 1, 0, col)))
                        {
                            StartCoroutine(DoMovement(5, new Vector3(row - 1, 0, col), new Vector3(row, 0, col)));
                        }
                        break;
                    case "Forward":
                        if (MovementPossible(new Vector3(row, 0, col - 1)))
                        {
                            StartCoroutine(DoMovement(5, new Vector3(row, 0, col - 1), new Vector3(row, 0, col)));
                        }
                        break;
                    case "Back":
                        if (MovementPossible(new Vector3(row, 0, col + 1)))
                        {
                            StartCoroutine(DoMovement(5, new Vector3(row, 0, col + 1), new Vector3(row, 0, col)));
                        }
                        break;
                }
            }
            if (angle == 180)
            {
                switch (Movement_name)
                {
                    case "Left":
                        if (MovementPossible(new Vector3(row - 1, 0, col)))
                        {
                            StartCoroutine(DoMovement(5, new Vector3(row - 1, 0, col), new Vector3(row, 0, col)));
                        }
                        break;
                    case "Right":
                        if (MovementPossible(new Vector3(row + 1, 0, col)))
                        {
                            StartCoroutine(DoMovement(5, new Vector3(row + 1, 0, col), new Vector3(row, 0, col)));
                        }
                        break;
                    case "Forward":
                        if (MovementPossible(new Vector3(row, 0, col + 1)))
                        {
                            StartCoroutine(DoMovement(5, new Vector3(row, 0, col + 1), new Vector3(row, 0, col)));
                        }
                        break;
                    case "Back":
                        if (MovementPossible(new Vector3(row, 0, col - 1)))
                        {
                            StartCoroutine(DoMovement(5, new Vector3(row, 0, col - 1), new Vector3(row, 0, col)));
                        }
                        break;
                }
            }
            if (angle == 270 || angle == -90)
            {
                switch (Movement_name)
                {
                    case "Forward":
                        if (MovementPossible(new Vector3(row + 1, 0, col)))
                        {
                            StartCoroutine(DoMovement(5, new Vector3(row + 1, 0, col), new Vector3(row, 0, col)));
                        }
                        break;
                    case "Back":
                        if (MovementPossible(new Vector3(row - 1, 0, col)))
                        {
                            StartCoroutine(DoMovement(5, new Vector3(row - 1, 0, col), new Vector3(row, 0, col)));
                        }
                        break;
                    case "Right":
                        if (MovementPossible(new Vector3(row, 0, col - 1)))
                        {
                            StartCoroutine(DoMovement(5, new Vector3(row, 0, col - 1), new Vector3(row, 0, col)));
                        }
                        break;
                    case "Left":
                        if (MovementPossible(new Vector3(row, 0, col + 1)))
                        {
                            StartCoroutine(DoMovement(5, new Vector3(row, 0, col + 1), new Vector3(row, 0, col)));
                        }
                        break;
                }
            }
            if (angle == 90)
            {
                switch (Movement_name)
                {
                    case "Forward":
                        if (MovementPossible(new Vector3(row - 1, 0, col)))
                        {
                            StartCoroutine(DoMovement(5, new Vector3(row - 1, 0, col), new Vector3(row, 0, col)));
                        }
                        break;
                    case "Back":
                        if (MovementPossible(new Vector3(row + 1, 0, col)))
                        {
                            StartCoroutine(DoMovement(5, new Vector3(row + 1, 0, col), new Vector3(row, 0, col)));
                        }
                        break;
                    case "Right":
                        if (MovementPossible(new Vector3(row, 0, col + 1)))
                        {
                            StartCoroutine(DoMovement(5, new Vector3(row, 0, col + 1), new Vector3(row, 0, col)));
                        }
                        break;
                    case "Left":
                        if (MovementPossible(new Vector3(row, 0, col - 1)))
                        {
                            StartCoroutine(DoMovement(5, new Vector3(row, 0, col - 1), new Vector3(row, 0, col)));
                        }
                        break;
                }
            }

        }
        else {
            Debug.Log("Movement in progress");
            GameManager.instance.InfoText.text = "Movement in progress";
        }
       
    }
    // Move the current game object to the specified location
    IEnumerator DoMovement(float lerpDuration, Vector3 Target, Vector3 OrignalPosition)
    {
        MovementInProgress = true;
        float StartTime = 0f;
        while (StartTime < lerpDuration)
        {
            transform.position = Vector3.MoveTowards(transform.position, Target, StartTime / lerpDuration);
            StartTime += Time.deltaTime * 4f;
            yield return null;
        }
        if (GameManager.instance.DeadLockOccured()) ///  confirm it wheather this is righ or not
        { // in case both players moved to the same location
            transform.position = OrignalPosition;
        }
        MovementInProgress = false;
        I_hv_Completed_my_Action(); // send acknowledgment to server about the action completion

    }
    //Call this function when you want to send ack about the action completion
    public void I_hv_Completed_my_Action() {
        myPlayer.Cmd_I_have_completed_Action(); // tell the server that i have completed my action
    }
    // assossiat this function with the event of action completion 
    public void MovetoNextAction() {
    
        //yield return new WaitForSeconds(5);
        // check if dodge of killer reflexes was enabled
        if (myPlayer.DODGE_APPLIED_KR)
        {
             // if dodge of killer reflexe was enabled, move two steps back
            float row = transform.position.x;
            float col = transform.position.z;
            float angle = transform.rotation.eulerAngles.y;
            int Movesteps = 0;
            switch ((int)angle)
            {
                case 0:
                    for (int i = 1; i < 3; i++)
                    {
                        if (MovementPossible_Reason(new Vector3(row, 0, col + i)) == "Possible")
                        {
                            Movesteps = i;
                        }
                    }
                    for (int i = 1; i < 3; i++)
                    {
                        string _name = row.ToString() + (col + i).ToString();
                        GameObject obj = PlayersManager.instance.GetBoxByName(_name);
                        if (obj != null)
                        {
                            obj.GetComponent<HighlightBox>()._HighLightBox();
                        }

                    }
                    StartCoroutine(DoMovement(5, new Vector3(row, 0, col + Movesteps), new Vector3(row, 0, col)));
                    break;
                case 180:
                    for (int i = 1; i < 3; i++)
                    {
                        if (MovementPossible_Reason(new Vector3(row, 0, col - i)) == "Possible")
                        {
                            Movesteps = i;
                        }
                    }
                    for (int i = 1; i < 3; i++)
                    {
                        string _name = row.ToString() + (col - i).ToString();
                        GameObject obj = PlayersManager.instance.GetBoxByName(_name);
                        if (obj != null)
                        {
                            obj.GetComponent<HighlightBox>()._HighLightBox();
                        }

                    }
                    StartCoroutine(DoMovement(5, new Vector3(row, 0, col - Movesteps), new Vector3(row, 0, col)));
                    break;
                case 270:
                    for (int i = 1; i < 3; i++)
                    {
                        if (MovementPossible_Reason(new Vector3(row + i, 0, col)) == "Possible")
                        {
                            Movesteps = i;
                        }
                    }
                    for (int i = 1; i < 3; i++)
                    {
                        string _name = (row + i).ToString() + (col).ToString();
                        GameObject obj = PlayersManager.instance.GetBoxByName(_name);
                        if (obj != null)
                        {
                            obj.GetComponent<HighlightBox>()._HighLightBox();
                        }

                    }
                    StartCoroutine(DoMovement(5, new Vector3(row + Movesteps, 0, col), new Vector3(row, 0, col)));
                    break;
                case 90:
                    for (int i = 1; i < 3; i++)
                    {
                        if (MovementPossible_Reason(new Vector3(row - i, 0, col)) == "Possible")
                        {
                            Movesteps = i;
                        }
                    }
                    for (int i = 1; i < 3; i++)
                    {
                        string _name = (row - i).ToString() + (col).ToString();
                        GameObject obj = PlayersManager.instance.GetBoxByName(_name);
                        if (obj != null)
                        {
                            obj.GetComponent<HighlightBox>()._HighLightBox();
                        }

                    }
                    StartCoroutine(DoMovement(5, new Vector3(row - Movesteps, 0, col), new Vector3(row, 0, col)));
                    break;

            }
            myPlayer.Cmd_SendDodgeValueTOServer(false);

        }
        StartCoroutine(MovtoNextActionPhaseCoroutine());
     
    }
    IEnumerator MovtoNextActionPhaseCoroutine()
    {
       
        yield return new WaitForSeconds(3);
        // here the player will move to next action phase
        if ((int)CurrentActionState < 3)
        {
            GameManager.instance.InfoText.text = "Move to next action phase";
            CurrentActionState++;// switching to next action phase
            if (myPlayer.BLOCK)
            {
                myPlayer.Cmd_SendBlockValueServer(false);

            }

        }
        else
        {
            if (!myPlayer.ApplySecondPhase_OWT && !OpponentOblivionWaits)
            {
                
                    if (myPlayer.BLOCK)
                    {
                        myPlayer.Cmd_SendBlockValueServer(false);

                    }

                // check if Opponent was stunned make him not stun
                if (OpponentisStunned)
                {
                    OpponentisStunned = false;
                }
                // check if player was stunned make him not stun
                if (myPlayer.IS_STUNNED)
                {
                    myPlayer.Cmd_SyncStun(false); //  Make the stun false at the start of the new action phase
                }
                // check if block before weaponize was true, make it false for the next turn
                if (myPlayer.BLOCK_USED_B_WEAPONIZE)
                {
                    myPlayer.Cmd_SyncBlockusedBeforeWeaponize(false);
                }
                // 
                // if 4 actions completed move to next phase
                // PlayersManager.instance.ClearPreviousActionSlotsOpenent();
                // PlayersManager.instance.ClearPreviousActionSlotsPlayer();
                GameManager.instance.InfoText.text = "Prepare for next turn, select your actions";
                CurrentState = GameState.Phase2_Select_Actions;
                PlayersManager.instance.ActionAckcount = 0;
                Phase2_once_bool = true;
            }
            else if (myPlayer.ApplySecondPhase_OWT)
            {
                Play_Oblivionwaits();
                GameManager.instance.InfoText.text = "Playing second phase of oblivion waits";
            }
            else if (OpponentOblivionWaits)
            {
                GameManager.instance.InfoText.text = "Waiting for the other player to finish 2nd phase Oblivion Waits";
            }
        }


        CheckRotation();

    }
    // check if movement is possible, checking for Terrain, Opponent, Overflow
    public bool MovementPossible(Vector3 Target) {
        if (GameManager.instance.Terrain_Coordinates.Contains(Target))
        {
            Debug.Log("Terrain at the specified location");
            I_hv_Completed_my_Action(); // send acknowledgment to server about the action completion
            return false;
        }
        else if (GameManager.instance.OtherPlayerExists(Target)) {
            Debug.Log("Other Player at the specified location");
            I_hv_Completed_my_Action(); // send acknowledgment to server about the action completion
            return false;
        } else if (Target.x < 0 || Target.z < 0 || Target.x > 5 || Target.z > 9) {
            Debug.Log("Over flow: trying location axis out side the board");
            I_hv_Completed_my_Action(); // send acknowledgment to server about the action completion
            return false;
        }
        return true;
    }
    // check if movement is possible, this funciton will be used for the abilities (ReapersReach), checking for Terrain, Opponent, Overflow
    public string MovementPossible_Reason(Vector3 Target)
    {
        if (GameManager.instance.Terrain_Coordinates.Contains(Target))
        {
            Debug.Log("Terrain at the specified location");
            return "Terrain";
        }
        else if (GameManager.instance.OtherPlayerExists(Target))
        {
            Debug.Log("Other Player at the specified location");
            return "OtherPlayer";
        }
        else if (Target.x < 0 || Target.z < 0 || Target.x > 5 || Target.z > 9)
        {
            Debug.Log("Over flow: trying location axis out side the board");
            return "OverFlow";
        }
        else {
            return "Possible";
        }
    }
    // Check Rotation of both players
    public void CheckRotation() {
        GameManager.instance.ChecKRotationofPlayers();
    }
    // associate this functoin with the waiting event
    public void OnWaitingDone() {
        
        if (Waiting) {
            // play the current action if not stunned
            Debug.Log("On waiting done");
            PlayActionByName(CurrentActionPlayer.Action_name.ToString());
            GameManager.instance.InfoText.text = "Waiting Done! Playing current action";
            Waiting = false;
        }
    }
    // This function will return the the string "First" , "Second", "Equal", this will show us that who's action will go first
    public string CheckPriority(ActionIdentity PlayerAction, ActionIdentity OpponentAction) {
        //check if the opponent's ability lies in the player's priority list, than player will go first
        if (PlayerAction.PriorityList.Contains(OpponentAction._Ability.ToString()))
        {
            return "First"; // this case this player will go first               
        }
        //Else check if the players current ability lies in the priority list of the opponent
        else if (OpponentAction.PriorityList.Contains(PlayerAction._Ability.ToString()))
        {
            return "Second";// in this case the player will go second
        }
        //Else if no one's action is prior, then both actions will go at the same time
        else
        {
            return "Equal";
        }
        
    }
    // Play action by name, this function will be used for the waiting player
    public void PlayActionByName(string ActionName) {
        switch (ActionName) {
            case "Left":
                PlayMovementAction("Left", transform.rotation.eulerAngles.y);
                break;
            case "Right":
                PlayMovementAction("Right", transform.rotation.eulerAngles.y);
                break;
            case "Forward":
                PlayMovementAction("Forward", transform.rotation.eulerAngles.y);
                break;
            case "Back":
                PlayMovementAction("Back", transform.rotation.eulerAngles.y);
                break;
            case "Ability1":
                switch (CurrentActionPlayer.AbilityName) {
                    case "LightningArrows":
                        // play LightningArrows ability
                        Play_LightningArrows();
                        break;
                    case "Block":
                        // Play block ability
                        Play_Block();
                        break;
                    case "ReapersReach":
                        // Play ReapersReach ability
                        Play_ReapersReach();
                        break;
                }
                break;
            case "Ability2":
                switch (CurrentActionPlayer.AbilityName)
                {
                    case "KillerReflexes":
                        // play KillerReflexes ability
                        Play_KillerReflexes();
                        break;
                    case "Charge":
                        // Play Charge ability
                        Play_Charge();
                        break;
                    case "CripplingSlash":
                        // Play CripplingSlash ability
                        Play_CripplingSlash();
                        break;
                }
                break;
            case "Ability3":
                switch (CurrentActionPlayer.AbilityName)
                {
                    case "LightningStrikesTwice":
                        // play LightningStrikesTwice ability
                        Play_LightningStrikesTwice();
                        break;
                    case "Weaponize":
                        // Play Weaponize ability
                        Play_Weaponize();
                        break;
                    case "Oblivionwaits":
                        // Play CripplingSlash ability
                        Play_Oblivionwaits();
                        break;
                }
                break;
            case "Noaction":
                I_hv_Completed_my_Action();
                break;
        }
    }
    //Play ability by name, call this function whenever you want to play an ability by name e.g Block, Charge etc
    public void PlayAbilityByName(string AbilityName)
    {
        switch (AbilityName) {
            case "LightningArrows":
                // play LightningArrows ability
                Play_LightningArrows();
                break;
            case "Block":
                // Play block ability
                Play_Block();
                break;
            case "ReapersReach":
                // Play ReapersReach ability
                Play_ReapersReach();
                break;
            case "KillerReflexes":
                // play KillerReflexes ability
                Play_KillerReflexes();
                break;
            case "Charge":
                // Play Charge ability
                Play_Charge();
                break;
            case "CripplingSlash":
                // Play CripplingSlash ability
                Play_CripplingSlash();
                break;
            case "LightningStrikesTwice":
                // play LightningStrikesTwice ability
                Play_LightningStrikesTwice();
                break;
            case "Weaponize":
                // Play Weaponize ability
                Play_Weaponize();
                break;
            case "Oblivionwaits":
                // Play CripplingSlash ability
                Play_Oblivionwaits();
                break;
        }
    }
    #region Ability Functions
    // call this function whenever you want to play the lightning Arrows ability
    public void Play_LightningArrows() {
       
        GameManager.instance.InfoText.text = "Playing ability Lightning arrows";
        float row = transform.position.x;
        float col = transform.position.z;
        float angle = transform.rotation.eulerAngles.y;
        GameObject[] AffectedBoxes = new GameObject[7];
        Vector3[] AffectedPoints = new Vector3[7];
        string _name;
        switch ((int)angle)
        {
            case 0:
                // first point 3-Damage
                AffectedPoints[0] = new Vector3(row, 0, col -1);
                _name = row.ToString() + (col - 1).ToString();
                AffectedBoxes[0] = PlayersManager.instance.GetBoxByName(_name);
                // second point 1-Damage
                AffectedPoints[1] = new Vector3(row, 0, col - 2);
                _name = row.ToString() + (col - 2).ToString();
                AffectedBoxes[1] = PlayersManager.instance.GetBoxByName(_name);
                // 3rd point 1-Damage
                AffectedPoints[2] = new Vector3(row, 0, col - 3);
                _name = row.ToString() + (col - 3).ToString();
                AffectedBoxes[2] = PlayersManager.instance.GetBoxByName(_name);
                // 4rth point 1-Damage
                AffectedPoints[3] = new Vector3(row-1, 0, col - 2);
                _name = (row-1).ToString() + (col - 2).ToString();
                AffectedBoxes[3] = PlayersManager.instance.GetBoxByName(_name);
                // 5th point 1-Damage
                AffectedPoints[4] = new Vector3(row+1, 0, col - 2);
                _name = (row+1).ToString() + (col - 2).ToString();
                AffectedBoxes[4] = PlayersManager.instance.GetBoxByName(_name);
                // 6th point 1-Damage
                AffectedPoints[5] = new Vector3(row + 2, 0, col - 3);
                _name = (row + 2).ToString() + (col - 3).ToString();
                AffectedBoxes[5] = PlayersManager.instance.GetBoxByName(_name);
                // 7th point 1-Damage
                AffectedPoints[6] = new Vector3(row-2, 0, col-3);
                _name = (row-2).ToString() + (col-3).ToString();
                AffectedBoxes[6] = PlayersManager.instance.GetBoxByName(_name);
                break;
            case 180:
                // first point 3-Damage
                AffectedPoints[0] = new Vector3(row, 0, col + 1);
                _name = row.ToString() + (col + 1).ToString();
                AffectedBoxes[0] = PlayersManager.instance.GetBoxByName(_name);
                // second point 1-Damage
                AffectedPoints[1] = new Vector3(row, 0, col + 2);
                _name = row.ToString() + (col + 2).ToString();
                AffectedBoxes[1] = PlayersManager.instance.GetBoxByName(_name);
                // 3rd point 1-Damage
                AffectedPoints[2] = new Vector3(row, 0, col + 3);
                _name = row.ToString() + (col + 3).ToString();
                AffectedBoxes[2] = PlayersManager.instance.GetBoxByName(_name);
                // 4rth point 1-Damage
                AffectedPoints[3] = new Vector3(row - 1, 0, col + 2);
                _name = (row - 1).ToString() + (col + 2).ToString();
                AffectedBoxes[3] = PlayersManager.instance.GetBoxByName(_name);
                // 5th point 1-Damage
                AffectedPoints[4] = new Vector3(row + 1, 0, col + 2);
                _name = (row + 1).ToString() + (col + 2).ToString();
                AffectedBoxes[4] = PlayersManager.instance.GetBoxByName(_name);
                // 6th point 1-Damage
                AffectedPoints[5] = new Vector3(row + 2, 0, col + 3);
                _name = (row + 2).ToString() + (col + 3).ToString();
                AffectedBoxes[5] = PlayersManager.instance.GetBoxByName(_name);
                // 7th point 1-Damage
                AffectedPoints[6] = new Vector3(row - 2, 0, col + 3);
                _name = (row - 2).ToString() + (col + 3).ToString();
                AffectedBoxes[6] = PlayersManager.instance.GetBoxByName(_name);
                break;
            case 90:
                // first point 3-Damage
                AffectedPoints[0] = new Vector3(row-1, 0, col);
                _name = (row-1).ToString() + (col).ToString();
                AffectedBoxes[0] = PlayersManager.instance.GetBoxByName(_name);
                // second point 1-Damage
                AffectedPoints[1] = new Vector3(row-2, 0, col);
                _name = (row-2).ToString() + (col).ToString();
                AffectedBoxes[1] = PlayersManager.instance.GetBoxByName(_name);
                // 3rd point 1-Damage
                AffectedPoints[2] = new Vector3(row-2, 0, col - 1);
                _name = (row-2).ToString() + (col - 1).ToString();
                AffectedBoxes[2] = PlayersManager.instance.GetBoxByName(_name);
                // 4rth point 1-Damage
                AffectedPoints[3] = new Vector3(row - 2, 0, col + 1);
                _name = (row - 2).ToString() + (col + 1).ToString();
                AffectedBoxes[3] = PlayersManager.instance.GetBoxByName(_name);
                // 5th point 1-Damage
                AffectedPoints[4] = new Vector3(row -3, 0, col);
                _name = (row - 3).ToString() + (col).ToString();
                AffectedBoxes[4] = PlayersManager.instance.GetBoxByName(_name);
                // 6th point 1-Damage
                AffectedPoints[5] = new Vector3(row -3, 0, col -2);
                _name = (row -3).ToString() + (col -2).ToString();
                AffectedBoxes[5] = PlayersManager.instance.GetBoxByName(_name);
                // 7th point 1-Damage
                AffectedPoints[6] = new Vector3(row - 3, 0, col + 2);
                _name = (row - 3).ToString() + (col + 2).ToString();
                AffectedBoxes[6] = PlayersManager.instance.GetBoxByName(_name);
                break;
            case 270:
                // first point 3-Damage
                AffectedPoints[0] = new Vector3(row + 1, 0, col);
                _name = (row + 1).ToString() + (col).ToString();
                AffectedBoxes[0] = PlayersManager.instance.GetBoxByName(_name);
                // second point 1-Damage
                AffectedPoints[1] = new Vector3(row + 2, 0, col);
                _name = (row + 2).ToString() + (col).ToString();
                AffectedBoxes[1] = PlayersManager.instance.GetBoxByName(_name);
                // 3rd point 1-Damage
                AffectedPoints[2] = new Vector3(row + 2, 0, col + 1);
                _name = (row + 2).ToString() + (col + 1).ToString();
                AffectedBoxes[2] = PlayersManager.instance.GetBoxByName(_name);
                // 4rth point 1-Damage
                AffectedPoints[3] = new Vector3(row + 2, 0, col - 1);
                _name = (row + 2).ToString() + (col - 1).ToString();
                AffectedBoxes[3] = PlayersManager.instance.GetBoxByName(_name);
                // 5th point 1-Damage
                AffectedPoints[4] = new Vector3(row + 3, 0, col);
                _name = (row + 3).ToString() + (col).ToString();
                AffectedBoxes[4] = PlayersManager.instance.GetBoxByName(_name);
                // 6th point 1-Damage
                AffectedPoints[5] = new Vector3(row + 3, 0, col - 2);
                _name = (row + 3).ToString() + (col - 2).ToString();
                AffectedBoxes[5] = PlayersManager.instance.GetBoxByName(_name);
                // 7th point 1-Damage
                AffectedPoints[6] = new Vector3(row + 3, 0, col + 2);
                _name = (row + 3).ToString() + (col + 2).ToString();
                AffectedBoxes[6] = PlayersManager.instance.GetBoxByName(_name);
                break;
        }

        for (int i = 0; i < AffectedBoxes.Length; i++)
        {
            if (AffectedBoxes[i] != null)
                AffectedBoxes[i].GetComponent<HighlightBox>()._HighLightBox();
        }
        for (int i = 0; i < AffectedPoints.Length; i++)
        {
            if (MovementPossible_Reason(AffectedPoints[i]) == "OtherPlayer")
            {
                // Other player ahead, apply damage to other player
                // put the condition if not blocked
                if (!OpponentDodgeIsActive)
                {
                    if (!OpponentIsBlock) { 
                        if (i == 0)
                        {
                            myPlayer.Cmd_ReduceHealth(3);
                        }
                        else
                        {
                            myPlayer.Cmd_ReduceHealth(1);
                        }
                    }
                }
                else if (OpponentDodgeIsActive)
                {
                    Debug.Log("oh no! you dodged my attack");
                    // reduce my health by one
                    if (myPlayer.HEALTH > 1)
                    {
                        float health = myPlayer.HEALTH - 1;
                        myPlayer.Cmd_SyncHealth(health);
                    }

                }

                Debug.Log("Apply damage to  player");
                break;
            }
        }
        I_hv_Completed_my_Action();
    }
    // call this funciton whenever you want to play Block ability
    public void Play_Block() {
        GameManager.instance.InfoText.text = "Playing ability block";
        myPlayer.Cmd_SendBlockValueServer(true);
        myPlayer.Cmd_SyncBlockusedBeforeWeaponize(true);
        I_hv_Completed_my_Action();
    }
    // call this function whenever you want to play ReapersReach ability
    public void Play_ReapersReach() {
        GameManager.instance.InfoText.text = "Playing ability ReapersReach ";
        if (!myPlayer.IS_STUNNED)
        {
            float row = transform.position.x;
            float col = transform.position.z;
            float angle = transform.rotation.eulerAngles.y;
            int Movesteps = 0;
            // only forward movement (Check 4 spaces in front)
            switch ((int)angle)
            {
                case 0:
                    // check the front 4 spaces and check if the movement is possible
                    for (int i = 1; i < 5; i++)
                    {
                        if (MovementPossible_Reason(new Vector3(row, 0, col - i)) == "Possible")
                        {
                            Movesteps = i;
                        }
                        else if (MovementPossible_Reason(new Vector3(row, 0, col - i)) == "Terrain")
                        {
                            // Terrain ahead, just move the player by "MoveSteps"
                            break;
                        }
                        else if (MovementPossible_Reason(new Vector3(row, 0, col - i)) == "OverFlow")
                        {
                            // over Flow
                            break;
                        }
                        else if (MovementPossible_Reason(new Vector3(row, 0, col - i)) == "OtherPlayer")
                        {
                            if (!OpponentDodgeIsActive)
                            {
                                // Other player ahead, apply damage to other player (damage +1 if OW used before this ability)
                                if (myPlayer.ApplySecondPhase_OWT)
                                {
                                    myPlayer.Cmd_ReduceHealth(2);
                                }
                                else
                                {
                                    myPlayer.Cmd_ReduceHealth(1);
                                }
                            }
                            else if (OpponentDodgeIsActive)
                            {
                                // successfull dodge
                                Debug.Log("oh no! you dodged my attack");
                                // reduce my health by one
                                if (myPlayer.HEALTH > 1)
                                {
                                    float health = myPlayer.HEALTH - 1;
                                    myPlayer.Cmd_SyncHealth(health);
                                }

                            }
                            Debug.Log("Apply damage to  player");
                            break;
                        }
                    }
                    for (int i = 1; i < 5; i++)
                    {
                        string _name = row.ToString() + (col - i).ToString();
                        GameObject obj = PlayersManager.instance.GetBoxByName(_name);
                        if (obj != null)
                        {
                            obj.GetComponent<HighlightBox>()._HighLightBox();
                        }

                    }
                    StartCoroutine(DoMovement(5, new Vector3(row, 0, col - Movesteps), new Vector3(row, 0, col)));
                    break;
                case 180:
                    // check the front 4 spaces and check if the movement is possible
                    for (int i = 1; i < 5; i++)
                    {
                        if (MovementPossible_Reason(new Vector3(row, 0, col + i)) == "Possible")
                        {
                            Movesteps = i;
                        }
                        else if (MovementPossible_Reason(new Vector3(row, 0, col + i)) == "Terrain")
                        {
                            // Terrain ahead, just move the player by "MoveSteps"
                            break;
                        }
                        else if (MovementPossible_Reason(new Vector3(row, 0, col + i)) == "OverFlow")
                        {
                            // over Flow
                            break;
                        }
                        else if (MovementPossible_Reason(new Vector3(row, 0, col + i)) == "OtherPlayer")
                        {
                            // Other player ahead, apply damage to other player
                            if (!OpponentDodgeIsActive)
                            {
                                // Other player ahead, apply damage to other player (damage +1 if OW used before this ability)
                                if (myPlayer.ApplySecondPhase_OWT)
                                {
                                    myPlayer.Cmd_ReduceHealth(2);
                                }
                                else
                                {
                                    myPlayer.Cmd_ReduceHealth(1);
                                }
                            }
                            else if (OpponentDodgeIsActive)
                            {
                                // successfull dodge
                                Debug.Log("oh no! you dodged my attack");
                                // reduce my health by one
                                if (myPlayer.HEALTH > 1)
                                {
                                    float health = myPlayer.HEALTH - 1;
                                    myPlayer.Cmd_SyncHealth(health);
                                }

                            }
                            Debug.Log("Apply damage to  player");
                            break;
                        }
                    }
                    for (int i = 1; i < 5; i++)
                    {
                        string _name = row.ToString() + (col + i).ToString();
                        GameObject obj = PlayersManager.instance.GetBoxByName(_name);
                        if (obj != null)
                        {
                            obj.GetComponent<HighlightBox>()._HighLightBox();
                        }

                    }
                    StartCoroutine(DoMovement(5, new Vector3(row, 0, col + Movesteps), new Vector3(row, 0, col)));
                    break;
                case 270:
                    // check the front 4 spaces and check if the movement is possible
                    for (int i = 1; i < 5; i++)
                    {
                        if (MovementPossible_Reason(new Vector3(row + i, 0, col)) == "Possible")
                        {
                            Movesteps = i;
                        }
                        else if (MovementPossible_Reason(new Vector3(row + i, 0, col)) == "Terrain")
                        {
                            // Terrain ahead, just move the player by "MoveSteps"
                            break;
                        }
                        else if (MovementPossible_Reason(new Vector3(row + i, 0, col)) == "OverFlow")
                        {
                            // over Flow
                            break;
                        }
                        else if (MovementPossible_Reason(new Vector3(row + i, 0, col)) == "OtherPlayer")
                        {
                            // Other player ahead, apply damage to other player
                            if (!OpponentDodgeIsActive)
                            {
                                // Other player ahead, apply damage to other player (damage +1 if OW used before this ability)
                                if (myPlayer.ApplySecondPhase_OWT)
                                {
                                    myPlayer.Cmd_ReduceHealth(2);
                                }
                                else
                                {
                                    myPlayer.Cmd_ReduceHealth(1);
                                }
                            }
                            else if (OpponentDodgeIsActive)
                            {
                                // successfull dodge
                                Debug.Log("oh no! you dodged my attack");
                                // reduce my health by one
                                if (myPlayer.HEALTH > 1)
                                {
                                    float health = myPlayer.HEALTH - 1;
                                    myPlayer.Cmd_SyncHealth(health);
                                }

                            }
                            Debug.Log("Apply damage to  player");
                            break;
                        }
                    }
                    for (int i = 1; i < 5; i++)
                    {
                        string _name = (row + i).ToString() + (col).ToString();
                        GameObject obj = PlayersManager.instance.GetBoxByName(_name);
                        if (obj != null)
                        {
                            obj.GetComponent<HighlightBox>()._HighLightBox();
                        }

                    }
                    StartCoroutine(DoMovement(5, new Vector3(row + Movesteps, 0, col), new Vector3(row, 0, col)));
                    break;
                case 90:
                    // check the front 4 spaces and check if the movement is possible
                    for (int i = 1; i < 5; i++)
                    {
                        if (MovementPossible_Reason(new Vector3(row - i, 0, col)) == "Possible")
                        {
                            Movesteps = i;
                        }
                        else if (MovementPossible_Reason(new Vector3(row - i, 0, col)) == "Terrain")
                        {
                            // Terrain ahead, just move the player by "MoveSteps"
                            break;
                        }
                        else if (MovementPossible_Reason(new Vector3(row - i, 0, col)) == "OverFlow")
                        {
                            // over Flow
                            break;
                        }
                        else if (MovementPossible_Reason(new Vector3(row - i, 0, col)) == "OtherPlayer")
                        {
                            // Other player ahead, apply damage to other player
                            if (!OpponentDodgeIsActive)
                            {
                                // Other player ahead, apply damage to other player (damage +1 if OW used before this ability)
                                if (myPlayer.ApplySecondPhase_OWT)
                                {
                                    myPlayer.Cmd_ReduceHealth(2);
                                }
                                else
                                {
                                    myPlayer.Cmd_ReduceHealth(1);
                                }
                            }
                            else if (OpponentDodgeIsActive)
                            {
                                // successfull dodge
                                Debug.Log("oh no! you dodged my attack");
                                // reduce my health by one
                                if (myPlayer.HEALTH > 1)
                                {
                                    float health = myPlayer.HEALTH - 1;
                                    myPlayer.Cmd_SyncHealth(health);
                                }

                            }
                            Debug.Log("Apply damage to  player");
                            break;
                        }
                    }
                    for (int i = 1; i < 5; i++)
                    {
                        string _name = (row - i).ToString() + (col).ToString();
                        GameObject obj = PlayersManager.instance.GetBoxByName(_name);
                        if (obj != null)
                        {
                            obj.GetComponent<HighlightBox>()._HighLightBox();
                        }

                    }
                    StartCoroutine(DoMovement(5, new Vector3(row - Movesteps, 0, col), new Vector3(row, 0, col)));
                    break;
            }
        }
        else if (myPlayer.IS_STUNNED) {
            I_hv_Completed_my_Action();
        }
    }
    // call this function whenever you want to play KillerReflexes ability
    public void Play_KillerReflexes()
    {
        GameManager.instance.InfoText.text = "Playing ability Killer Reflexes";
        myPlayer.Cmd_SendDodgeValueTOServer(true);
        Invoke("I_hv_Completed_my_Action", 5);
    }
    // call this function whenever you want to play Charge ability
    public void Play_Charge()
    {
        GameManager.instance.InfoText.text = "Playing ability Charge";
        float row = transform.position.x;
        float col = transform.position.z;
        float angle = transform.rotation.eulerAngles.y;
        int Movesteps = 0;
        // only forward movement (Check 4 spaces in front)
        switch ((int)angle)
        {
            case 0:
                // check the front 4 spaces and check if the movement is possible
                for (int i = 1; i < 4; i++)
                {
                    if (MovementPossible_Reason(new Vector3(row, 0, col - i)) == "Possible")
                    {
                        Movesteps = i;
                    }
                    else if (MovementPossible_Reason(new Vector3(row, 0, col - i)) == "Terrain")
                    {
                        // Terrain ahead, just move the player by "MoveSteps"
                        break;
                    }
                    else if (MovementPossible_Reason(new Vector3(row, 0, col - i)) == "OverFlow")
                    {
                        // over Flow
                        break;
                    }
                    else if (MovementPossible_Reason(new Vector3(row, 0, col - i)) == "OtherPlayer")
                    {
                        // Other player ahead, apply damage to other player
                        // Apply Stun the opponent
                        

                        if (!OpponentDodgeIsActive)
                        {
                            myPlayer.Cmd_StunOpponent(true);
                            myPlayer.Cmd_ReduceHealth(1);
                        }
                        else if (OpponentDodgeIsActive)
                        {
                            // successfull dodge
                            Debug.Log("oh no! you dodged my attack");
                            // reduce my health by one
                            if (myPlayer.HEALTH > 1)
                            {
                                float health = myPlayer.HEALTH - 1;
                                myPlayer.Cmd_SyncHealth(health);
                            }

                        }
                        Debug.Log("Apply damage to  player");
                        break;
                    }
                }
                for (int i = 1; i < 4; i++)
                {
                    string _name = row.ToString() + (col - i).ToString();
                    GameObject obj = PlayersManager.instance.GetBoxByName(_name);
                    if (obj != null)
                    {
                        obj.GetComponent<HighlightBox>()._HighLightBox();
                    }

                }
                StartCoroutine(DoMovement(5, new Vector3(row, 0, col - Movesteps), new Vector3(row, 0, col)));
                break;
            case 180:
                // check the front 4 spaces and check if the movement is possible
                for (int i = 1; i < 4; i++)
                {
                    if (MovementPossible_Reason(new Vector3(row, 0, col + i)) == "Possible")
                    {
                        Movesteps = i;
                    }
                    else if (MovementPossible_Reason(new Vector3(row, 0, col + i)) == "Terrain")
                    {
                        // Terrain ahead, just move the player by "MoveSteps"
                        break;
                    }
                    else if (MovementPossible_Reason(new Vector3(row, 0, col + i)) == "OverFlow")
                    {
                        // over Flow
                        break;
                    }
                    else if (MovementPossible_Reason(new Vector3(row, 0, col + i)) == "OtherPlayer")
                    {
                        // Other player ahead, apply damage to other player
                        if (!OpponentDodgeIsActive)
                        {
                            myPlayer.Cmd_StunOpponent(true);
                            myPlayer.Cmd_ReduceHealth(1);
                        }
                        else if (OpponentDodgeIsActive)
                        {
                            // successfull dodge
                            Debug.Log("oh no! you dodged my attack");
                            // reduce my health by one
                            if (myPlayer.HEALTH > 1)
                            {
                                float health = myPlayer.HEALTH - 1;
                                myPlayer.Cmd_SyncHealth(health);
                            }

                        }
                        Debug.Log("Apply damage to  player");
                        break;
                    }
                }
                for (int i = 1; i < 4; i++)
                {
                    string _name = row.ToString() + (col + i).ToString();
                    GameObject obj = PlayersManager.instance.GetBoxByName(_name);
                    if (obj != null)
                    {
                        obj.GetComponent<HighlightBox>()._HighLightBox();
                    }

                }
                StartCoroutine(DoMovement(5, new Vector3(row, 0, col + Movesteps), new Vector3(row, 0, col)));
                break;
            case 270:
                // check the front 4 spaces and check if the movement is possible
                for (int i = 1; i < 4; i++)
                {
                    if (MovementPossible_Reason(new Vector3(row + i, 0, col)) == "Possible")
                    {
                        Movesteps = i;
                    }
                    else if (MovementPossible_Reason(new Vector3(row + i, 0, col)) == "Terrain")
                    {
                        // Terrain ahead, just move the player by "MoveSteps"
                        break;
                    }
                    else if (MovementPossible_Reason(new Vector3(row + i, 0, col)) == "OverFlow")
                    {
                        // over Flow
                        break;
                    }
                    else if (MovementPossible_Reason(new Vector3(row + i, 0, col)) == "OtherPlayer")
                    {
                        // Other player ahead, apply damage to other player
                        if (!OpponentDodgeIsActive)
                        {
                            myPlayer.Cmd_StunOpponent(true);
                            myPlayer.Cmd_ReduceHealth(1);
                        }
                        else if (OpponentDodgeIsActive)
                        {
                            // successfull dodge
                            Debug.Log("oh no! you dodged my attack");
                            // reduce my health by one
                            if (myPlayer.HEALTH > 1)
                            {
                                float health = myPlayer.HEALTH - 1;
                                myPlayer.Cmd_SyncHealth(health);
                            }

                        }
                        Debug.Log("Apply damage to  player");
                        break;
                    }
                }
                for (int i = 1; i < 4; i++)
                {
                    string _name = (row + i).ToString() + (col).ToString();
                    GameObject obj = PlayersManager.instance.GetBoxByName(_name);
                    if (obj != null)
                    {
                        obj.GetComponent<HighlightBox>()._HighLightBox();
                    }

                }
                StartCoroutine(DoMovement(5, new Vector3(row + Movesteps, 0, col), new Vector3(row, 0, col)));
                break;
            case 90:
                // check the front 4 spaces and check if the movement is possible
                for (int i = 1; i < 4; i++)
                {
                    if (MovementPossible_Reason(new Vector3(row - i, 0, col)) == "Possible")
                    {
                        Movesteps = i;
                    }
                    else if (MovementPossible_Reason(new Vector3(row - i, 0, col)) == "Terrain")
                    {
                        // Terrain ahead, just move the player by "MoveSteps"
                        break;
                    }
                    else if (MovementPossible_Reason(new Vector3(row - i, 0, col)) == "OverFlow")
                    {
                        // over Flow
                        break;
                    }
                    else if (MovementPossible_Reason(new Vector3(row - i, 0, col)) == "OtherPlayer")
                    {
                        // Other player ahead, apply damage to other player
                        if (!OpponentDodgeIsActive)
                        {
                            myPlayer.Cmd_StunOpponent(true);
                            myPlayer.Cmd_ReduceHealth(1);
                        }
                        else if (OpponentDodgeIsActive)
                        {
                            // successfull dodge
                            Debug.Log("oh no! you dodged my attack");
                            // reduce my health by one
                            if (myPlayer.HEALTH > 1)
                            {
                                float health = myPlayer.HEALTH - 1;
                                myPlayer.Cmd_SyncHealth(health);
                            }

                        }
                        Debug.Log("Apply damage to  player");
                        break;
                    }
                }
                for (int i = 1; i < 4; i++)
                {
                    string _name = (row - i).ToString() + (col).ToString();
                    GameObject obj = PlayersManager.instance.GetBoxByName(_name);
                    if (obj != null)
                    {
                        obj.GetComponent<HighlightBox>()._HighLightBox();
                    }

                }
                StartCoroutine(DoMovement(5, new Vector3(row - Movesteps, 0, col), new Vector3(row, 0, col)));
                break;
        }
    }
    // call this function whenever you want to play CripplingSlash ability
    public void Play_CripplingSlash()
    {
        if (!myPlayer.IS_STUNNED)
        {
            GameManager.instance.InfoText.text = "Playing Crippling slash";
            float row = transform.position.x;
            float col = transform.position.z;
            string _name;

            GameObject[] AffectedBoxes = new GameObject[4];
            Vector3[] AffectedPoints = new Vector3[4];

            // first point 2-Damage
            AffectedPoints[0] = new Vector3(row, 0, col - 1);
            _name = row.ToString() + (col - 1).ToString();
            AffectedBoxes[0] = PlayersManager.instance.GetBoxByName(_name);
            // second point 1-Damage
            AffectedPoints[1] = new Vector3(row + 1, 0, col);
            _name = (row + 1).ToString() + (col).ToString();
            AffectedBoxes[1] = PlayersManager.instance.GetBoxByName(_name);
            // 3rd point 1-Damage
            AffectedPoints[2] = new Vector3(row - 1, 0, col);
            _name = (row - 1).ToString() + (col).ToString();
            AffectedBoxes[2] = PlayersManager.instance.GetBoxByName(_name);
            // 4rth point
            AffectedPoints[3] = new Vector3(row, 0, col + 1);
            _name = row.ToString() + (col + 1).ToString();
            AffectedBoxes[3] = PlayersManager.instance.GetBoxByName(_name);
            // highlight the effected boxes and check if Opponent is there or not
            for (int i = 0; i < AffectedBoxes.Length; i++)
            {
                if (AffectedBoxes[i] != null)
                    AffectedBoxes[i].GetComponent<HighlightBox>()._HighLightBox();
            }
            for (int i = 0; i < AffectedPoints.Length; i++)
            {
                if (MovementPossible_Reason(AffectedPoints[i]) == "OtherPlayer")
                {
                    // Other player ahead, apply damage to other player
                    // put the condition if not blocked


                    if (!OpponentDodgeIsActive)
                    {
                        if (!OpponentIsBlock)
                        {
                            if (myPlayer.ApplySecondPhase_OWT)
                            {
                                myPlayer.Cmd_ReduceHealth(2);
                            }
                            else
                            {
                                myPlayer.Cmd_ReduceHealth(1);
                            }
                            // heal me  for 1 hp
                            if (myPlayer.HEALTH < 10)
                            {
                                float health = myPlayer.HEALTH + 1;
                                myPlayer.Cmd_SyncHealth(health);
                            }
                        }
                        else if (OpponentIsBlock)
                        {
                            // exectute successfull block
                            Debug.Log("Successfull block");
                        }
                    }
                    else if (OpponentDodgeIsActive)
                    {
                        // successfull dodge
                        Debug.Log("oh no! you dodged my attack");
                        // reduce my health by one
                        if (myPlayer.HEALTH > 1)
                        {
                            float health = myPlayer.HEALTH - 1;
                            myPlayer.Cmd_SyncHealth(health);
                        }

                    }
                    Debug.Log("Apply damage to  player");
                    break;
                }
            }
            I_hv_Completed_my_Action();
        }
        else if(myPlayer.IS_STUNNED) {
            I_hv_Completed_my_Action();
        }
    }
    // call this function whenever you want to play LightningStrikesTwice ability
    public void Play_LightningStrikesTwice()
    {
        //<Temprory
        GameManager.instance.InfoText.text = "Playing Lightning strikes twice";
        Invoke("I_hv_Completed_my_Action", 10);
        //Temprory>
    }
    // call this function whenever you want to play Weaponize ability
    public void Play_Weaponize()
    {
        GameManager.instance.InfoText.text = "Playing Weaponize";
        float row = transform.position.x;
        float col = transform.position.z;
        float angle = transform.rotation.eulerAngles.y;
        string _name;
        int damage = 1;
        if (myPlayer.BLOCK_USED_B_WEAPONIZE) {
            damage = 2;
        }
        GameObject[] AffectedBoxes = new GameObject[3];
        Vector3[] AffectedPoints = new Vector3[3];
        switch ((int)angle) {
            case 0:
                // first point 2-Damage
                AffectedPoints[0] = new Vector3(row, 0, col - 1);
                _name = row.ToString() + (col - 1).ToString();
                AffectedBoxes[0] = PlayersManager.instance.GetBoxByName(_name);
                // second point 1-Damage
                AffectedPoints[1] = new Vector3(row+1, 0, col - 1);
                _name = (row+1).ToString() + (col - 1).ToString();
                AffectedBoxes[1] = PlayersManager.instance.GetBoxByName(_name);
                // 3rd point 1-Damage
                AffectedPoints[2] = new Vector3(row-1, 0, col - 1);
                _name = (row-1).ToString() + (col - 1).ToString();
                AffectedBoxes[2] = PlayersManager.instance.GetBoxByName(_name);
                break;
            case 180:
                // first point 2-Damage
                AffectedPoints[0] = new Vector3(row, 0, col + 1);
                _name = row.ToString() + (col + 1).ToString();
                AffectedBoxes[0] = PlayersManager.instance.GetBoxByName(_name);
                // second point 1-Damage
                AffectedPoints[1] = new Vector3(row-1, 0, col +1);
                _name = (row-1).ToString() + (col+1).ToString();
                AffectedBoxes[1] = PlayersManager.instance.GetBoxByName(_name);
                // 3rd point 1-Damage
                AffectedPoints[2] = new Vector3(row+1, 0, col + 1);
                _name = (row+1).ToString() + (col + 1).ToString();
                AffectedBoxes[2] = PlayersManager.instance.GetBoxByName(_name);
                break;
            case 90:
                // first point 2-Damage
                AffectedPoints[0] = new Vector3(row-1, 0, col);
                _name = (row-1).ToString() + (col).ToString();
                AffectedBoxes[0] = PlayersManager.instance.GetBoxByName(_name);
                // second point 1-Damage
                AffectedPoints[1] = new Vector3(row - 1, 0, col - 1);
                _name = (row - 1).ToString() + (col - 1).ToString();
                AffectedBoxes[1] = PlayersManager.instance.GetBoxByName(_name);
                // 3rd point 1-Damage
                AffectedPoints[2] = new Vector3(row - 1, 0, col + 1);
                _name = (row - 1).ToString() + (col + 1).ToString();
                AffectedBoxes[2] = PlayersManager.instance.GetBoxByName(_name);
                break;
            case 270:
                // first point 2-Damage
                AffectedPoints[0] = new Vector3(row + 1, 0, col);
                _name = (row + 1).ToString() + (col).ToString();
                AffectedBoxes[0] = PlayersManager.instance.GetBoxByName(_name);
                // second point 1-Damage
                AffectedPoints[1] = new Vector3(row + 1, 0, col - 1);
                _name = (row + 1).ToString() + (col - 1).ToString();
                AffectedBoxes[1] = PlayersManager.instance.GetBoxByName(_name);
                // 3rd point 1-Damage
                AffectedPoints[2] = new Vector3(row + 1, 0, col + 1);
                _name = (row + 1).ToString() + (col + 1).ToString();
                AffectedBoxes[2] = PlayersManager.instance.GetBoxByName(_name);
                break;
        }
        for (int i = 0; i < AffectedBoxes.Length; i++)
        {
            if (AffectedBoxes[i] != null)
                AffectedBoxes[i].GetComponent<HighlightBox>()._HighLightBox();
        }
        for (int i = 0; i < AffectedPoints.Length; i++)
        {
            if (MovementPossible_Reason(AffectedPoints[i]) == "OtherPlayer")
            {
                // Other player ahead, apply damage to other player
                // put the condition if not blocked
                if (!OpponentDodgeIsActive) { 
                    if (i == 0)
                    {
                        myPlayer.Cmd_ReduceHealth(damage+1);
                    }
                    else
                    {
                        myPlayer.Cmd_ReduceHealth(damage);
                    }
                }
                else if (OpponentDodgeIsActive)
                {
                    // successfull dodge
                    Debug.Log("oh no! you dodged my attack");
                    // reduce my health by one
                    if (myPlayer.HEALTH > 1)
                    {
                        float health = myPlayer.HEALTH - 1;
                        myPlayer.Cmd_SyncHealth(health);
                    }

                }

                Debug.Log("Apply damage to  player");

                break;
            }
        }
        I_hv_Completed_my_Action();

    }
    // call this function whenever you want to play Oblivionwaits ability
    public void Play_Oblivionwaits()
    {
        if (!myPlayer.IS_STUNNED)
        {
            GameManager.instance.InfoText.text = "Playing Oblivionwaits";
            float row = transform.position.x;
            float col = transform.position.z;
            string _name;

            GameObject[] AffectedBoxes = new GameObject[8];
            Vector3[] AffectedPoints = new Vector3[8];

            // first point 2-Damage
            AffectedPoints[0] = new Vector3(row, 0, col - 1);
            _name = row.ToString() + (col - 1).ToString();
            AffectedBoxes[0] = PlayersManager.instance.GetBoxByName(_name);
            // second point 1-Damage
            AffectedPoints[1] = new Vector3(row + 1, 0, col);
            _name = (row + 1).ToString() + (col).ToString();
            AffectedBoxes[1] = PlayersManager.instance.GetBoxByName(_name);
            // 3rd point 1-Damage
            AffectedPoints[2] = new Vector3(row - 1, 0, col);
            _name = (row - 1).ToString() + (col).ToString();
            AffectedBoxes[2] = PlayersManager.instance.GetBoxByName(_name);
            // 4rth point
            AffectedPoints[3] = new Vector3(row, 0, col + 1);
            _name = row.ToString() + (col + 1).ToString();
            AffectedBoxes[3] = PlayersManager.instance.GetBoxByName(_name);
            // 5th point
            AffectedPoints[4] = new Vector3(row - 1, 0, col - 1);
            _name = (row - 1).ToString() + (col - 1).ToString();
            AffectedBoxes[4] = PlayersManager.instance.GetBoxByName(_name);
            // 6th point
            AffectedPoints[5] = new Vector3(row + 1, 0, col - 1);
            _name = (row + 1).ToString() + (col - 1).ToString();
            AffectedBoxes[5] = PlayersManager.instance.GetBoxByName(_name);
            // 7th point
            AffectedPoints[6] = new Vector3(row + 1, 0, col + 1);
            _name = (row + 1).ToString() + (col + 1).ToString();
            AffectedBoxes[6] = PlayersManager.instance.GetBoxByName(_name);
            // 8th point
            AffectedPoints[7] = new Vector3(row - 1, 0, col + 1);
            _name = (row - 1).ToString() + (col + 1).ToString();
            AffectedBoxes[7] = PlayersManager.instance.GetBoxByName(_name);

            // highlight the effected boxes and check if Opponent is there or not
            for (int i = 0; i < AffectedBoxes.Length; i++)
            {
                if (AffectedBoxes[i] != null)
                    AffectedBoxes[i].GetComponent<HighlightBox>()._HighLightBox();
            }
            if (!myPlayer.ApplySecondPhase_OWT)
            {
                for (int i = 0; i < AffectedPoints.Length; i++)
                {
                    if (MovementPossible_Reason(AffectedPoints[i]) == "OtherPlayer")
                    {
                        // Other player ahead, apply damage to other player
                        // put the condition if not blocked
                        if (!OpponentDodgeIsActive)
                        {
                            if (!OpponentIsBlock)
                            {
                                myPlayer.Cmd_ReduceHealth(1);
                                myPlayer.Cmd_SyncApplySecondPhase_OWT(true);
                            }
                            else if (OpponentIsBlock)
                            {
                                Debug.Log("Successful block");
                            }
                        }
                        else if (OpponentDodgeIsActive)
                        {
                            // successfull dodge
                            Debug.Log("oh no! you dodged my attack");
                            // reduce my health by one
                            if (myPlayer.HEALTH > 1)
                            {
                                float health = myPlayer.HEALTH - 1;
                                myPlayer.Cmd_SyncHealth(health);
                            }

                        }

                        Debug.Log("Apply damage to  player");
                        break;
                    }
                }
                I_hv_Completed_my_Action();
            }
            // apply second phase here
            else if (myPlayer.ApplySecondPhase_OWT)
            {
                for (int i = 0; i < AffectedPoints.Length; i++)
                {
                    if (MovementPossible_Reason(AffectedPoints[i]) == "OtherPlayer")
                    {
                        // Other player ahead, apply damage to other player
                        // put the condition if not blocked
                        myPlayer.Cmd_ReduceHealth(4);

                        Debug.Log("Apply damage to  player");
                        break;
                    }
                }
                StartCoroutine(SendOblivionWaitsCommands());
            }
        } else if (myPlayer.IS_STUNNED) {
            I_hv_Completed_my_Action();
        }
    }
    #endregion
    //Send my name to palyers manager, associate this funcion with on Player spawn
    public void BothPlayersAreReady() {
        // check the players count 
        myPlayer.Cmd_BothPlayersAreReady();
    }
    // if both the players got ready , set there opponents name
    public void SendMyNameToServer() {
        myPlayer.Cmd_SendMyNameToServer(PlayersManager.instance.SelectedPlayer);
    }
    // Apply damage to this player,and also sync health so that your remote player also update his health
    public void ApplyDamage(float damage) {
        myPlayer.HEALTH -= damage;
        myPlayer.HealthText.text = myPlayer.HEALTH.ToString();
        myPlayer.Cmd_SyncHealth(myPlayer.HEALTH);// syncHealth across the network
    }
    // Apply Stun to this player
    public void ApplySTUN(bool value) {
        myPlayer.IS_STUNNED = value;
        myPlayer.Cmd_SyncStun(value);
    }
    // use for oblivion wait to send commands after a little break
    IEnumerator SendOblivionWaitsCommands() {
        yield return new WaitForSeconds(1);
        myPlayer.Cmd_SyncApplySecondPhase_OWT(false);
        yield return new WaitForSeconds(1);
        myPlayer.Cmd_MoveToActionSelection();
    }
}
