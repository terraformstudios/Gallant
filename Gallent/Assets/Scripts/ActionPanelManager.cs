using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionPanelManager : MonoBehaviour {
    public GameObject[] ActionSlots; // drag 4 slots of actions here
    public int ActionSelectionTimer = 30; // Total time for action selection
    public GameObject[] AllActionSlots;
    public Text TimerText;

    private void OnEnable()
    {
        TimerText.text = ActionSelectionTimer.ToString()+"Sec";
        StartCoroutine(StartTimer());
        ResetActionSlots();// shift all actions to the All actions slots and make the chosen action slots empty
    }
    private void OnDisable()
    {
        ActionSelectionTimer = 30;
    }
    // Associate this function with the Done Btn or with time
    public void ActionChoosingDone() {
        /* for (int i = 0; i < ActionSlots.Length; i++)
         {
             if (ActionSlots[i].transform.childCount == 0)
             {
                 GameManager.instance.InfoText.text = "You Must choose 4 actions";
                 return;
             }
         }*/
        //store all the chosen actions
        // Make this panal disappear and assign these actions
        PlayersManager.instance.TriggerActionChoosingDoneEvent(); // Trigger this event when actions are finalized
        this.gameObject.SetActive(false);
        GameManager.instance.InfoText.text = "Action selection done!";
    }
    IEnumerator StartTimer() {
        while (ActionSelectionTimer > -1) {
            ActionSelectionTimer -= 1;
            TimerText.text = ActionSelectionTimer.ToString()+"Sec";
            yield return new WaitForSeconds(1);
        }
        ActionChoosingDone();
        ActionSelectionTimer = 30;
    }
    public void ResetActionSlots() {
        for (int i=0; i<ActionSlots.Length; i++) {
            if (ActionSlots[i].transform.childCount > 0) { // check if the current slot is not empty
                for (int j=0; j < AllActionSlots.Length; j++) {
                    if (AllActionSlots[j].transform.tag == ActionSlots[i].transform.GetChild(0).name) {
                        ActionSlots[i].transform.GetChild(0).SetParent(AllActionSlots[j].transform);
                        for (int z = 0; z < AllActionSlots[j].transform.childCount; z++)
                        {
                            if (z == 0)
                            {
                                AllActionSlots[j].transform.GetChild(z).gameObject.SetActive(true);
                            }
                            else {
                                AllActionSlots[j].transform.GetChild(z).gameObject.SetActive(false);
                            }
                        }
                        break;
                    }
                }
            }
        }

    }
}
