﻿using UnityEngine.Networking;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameTimer : NetworkBehaviour {
    [SerializeField]
    [SyncVar] public float gameTime; //The length of a game, in seconds.
    [SyncVar] public float timer; //How long the game has been running. -1=waiting for players, -2=game is done
    [SyncVar] public int minPlayers; //Number of players required for the game to start
    [SyncVar] public bool masterTimer = false; //Is this the master timer?
                                               //public ServerTimer timerObj;
    public Text TimerText;
    private bool once = true;
    GameTimer serverTimer;

    void Start()
    {
        once = true;
        if (isLocalPlayer) {
           // TimerText = PlayersManager.instance.timerText;
           
        }
       // ResultPanal.SetActive(false);
        if (isServer)
        { // For the host to do: use the timer and control the time.
            if (isLocalPlayer)
            {
                serverTimer = this;
                masterTimer = true;
            }
        }
        else if (isLocalPlayer)
        { //For all the boring old clients to do: get the host's timer.
            GameTimer[] timers = FindObjectsOfType<GameTimer>();
            for (int i = 0; i < timers.Length; i++)
            {
                if (timers[i].masterTimer)
                {
                    serverTimer = timers[i];
                }
            }
        }
    }
    void Update()
    {
       // TimerText.text = Mathf.Floor(timer).ToString();
        if (masterTimer)
        { //Only the MASTER timer controls the time
            if (timer >= gameTime)
            {
                timer = -2;
            }
            else if (timer == -1)
            {
                if (NetworkServer.connections.Count >= minPlayers)
                {
                    timer = 0;
                }
            }
            else if (timer == -2)
            {
                if(once)
                {
                   // GameplayControl.levelClear = true;
                    //for (int i = 0;i < PlayersManager.instance.Players.Count; i++) {
                    //    PlayersManager.instance.UIRankObjects[i].SetActive(true);
                    //}
                 //   PlayersManager.instance.OnWinAssignValues();
                    once = false;
                }
                
            }
            else
            {
                timer += Time.deltaTime;
            }
        }

        if (isLocalPlayer)
        { //EVERYBODY updates their own time accordingly.
            TimerText.text = Mathf.Floor(timer).ToString();
            if (serverTimer)
            {
                gameTime = serverTimer.gameTime;
                timer = serverTimer.timer;
                minPlayers = serverTimer.minPlayers;
            }
            else
            { //Maybe we don't have it yet?
                GameTimer[] timers = FindObjectsOfType<GameTimer>();
                for (int i = 0; i < timers.Length; i++)
                {
                    if (timers[i].masterTimer)
                    {
                        serverTimer = timers[i];
                    }
                }
            }
        }
    }
}
