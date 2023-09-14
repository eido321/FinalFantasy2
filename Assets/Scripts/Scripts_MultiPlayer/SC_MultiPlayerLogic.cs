using AssemblyCSharp;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SC_MultiPlayerLogic : MonoBehaviour
{
    #region AppWrap Keys
    private string apiKey = "1bcec2061d9aee7a55d392d777d29915703ae87f4e395cdf748ad00a892f2331";
    private string secretKey = "f7ea560f993f68e7f0acb767428cbc202a9363d861a42a748aa1d8d06f97bbd1";
    private Listener listener;
    #endregion

    #region Variables

    public Dictionary<string, GameObject> unityObjects;
    private Dictionary<string, object> passedParams;
    private List<string> roomIds;
    private int roomIndex = 0;

    private int maxRoomUsers = 2;
    private string roomName = "ShenkarRoom";
    public string roomId;

    public SC_BattleLogic battleLogic;
    public static string _SetPass;
    #endregion

    #region Singleton

    private static SC_MultiPlayerLogic instance;
    public static SC_MultiPlayerLogic Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.Find("SC_MultiPlayerLogic").GetComponent<SC_MultiPlayerLogic>();

            return instance;
        }
    }

    #endregion

    #region MonoBehaviour

    private void OnEnable()
    {
        Listener.OnConnect += OnConnect;
        Listener.OnRoomsInRange += OnRoomsInRange;
        Listener.OnCreateRoom += OnCreateRoom;
        Listener.OnJoinRoom += OnJoinRoom;
        Listener.OnGetLiveRoomInfo += OnGetLiveRoomInfo;
        Listener.OnUserJoinRoom += OnUserJoinRoom;
        Listener.OnGameStarted += OnGameStarted;
    }

    private void OnDisable()
    {
        Listener.OnConnect -= OnConnect;
        Listener.OnRoomsInRange -= OnRoomsInRange;
        Listener.OnCreateRoom -= OnCreateRoom;
        Listener.OnJoinRoom -= OnJoinRoom;
        Listener.OnGetLiveRoomInfo -= OnGetLiveRoomInfo;
        Listener.OnUserJoinRoom -= OnUserJoinRoom;
        Listener.OnGameStarted -= OnGameStarted;
    }

    void Awake()
    {
        InitAwake();
    }

    #endregion

    #region Inititiation
    private void InitAwake()
    {
        unityObjects = new Dictionary<string, GameObject>();
        GameObject[] _obj = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (GameObject g in _obj)
            unityObjects.Add(g.name, g);

        passedParams = new Dictionary<string, object>()
        {{"Password",_SetPass}};

        listener = new Listener();

        WarpClient.initialize(apiKey, secretKey);
        WarpClient.GetInstance().AddConnectionRequestListener(listener);
        WarpClient.GetInstance().AddChatRequestListener(listener);
        WarpClient.GetInstance().AddUpdateRequestListener(listener);
        WarpClient.GetInstance().AddLobbyRequestListener(listener);
        WarpClient.GetInstance().AddNotificationListener(listener);
        WarpClient.GetInstance().AddRoomRequestListener(listener);
        WarpClient.GetInstance().AddTurnBasedRoomRequestListener(listener);
        WarpClient.GetInstance().AddZoneRequestListener(listener);

        GlobalVariables.userId = System.Guid.NewGuid().ToString();
        Debug.Log("UserId: " + GlobalVariables.userId);

        WarpClient.GetInstance().Connect(GlobalVariables.userId);
        Debug.Log("Open Connection...");
    }

    private void DoRoomSearchLogic()
    {
        if (roomIndex < roomIds.Count)
        {
            Debug.Log("Bring room info (" + roomIds[roomIndex] + ")");
            WarpClient.GetInstance().GetLiveRoomInfo(roomIds[roomIndex]);
        }
        else
        {
            Debug.Log("Creating Room...");
            int _randNumber = UnityEngine.Random.Range(100000, 999999);
            WarpClient.GetInstance().CreateTurnRoom(roomName + _randNumber, GlobalVariables.userId, maxRoomUsers, passedParams, GlobalVariables.turnTime);
        }
    }
    #endregion

    #region Server Callbacks

    private void OnConnect(bool _IsSuccess)
    {
        Debug.Log("OnConnect " + _IsSuccess);
        if (_IsSuccess)
        {
            Debug.Log("Connected.");
            unityObjects["Selection_box_start"].GetComponent<Button>().interactable = true;
        }
        else
        {
            Debug.Log("Failed to Connect.");
        }
    }

    private void OnRoomsInRange(bool _IsSuccess, MatchedRoomsEvent eventObj)
    {
        Debug.Log("OnRoomsInRange " + _IsSuccess);
        if (_IsSuccess)
        {
            Debug.Log("Parsing Rooms...");
            roomIds = new List<string>();
            foreach (var RoomData in eventObj.getRoomsData())
            {
                Debug.Log("Room Id: " + RoomData.getId());
                Debug.Log("Room Owner: " + RoomData.getRoomOwner());
                roomIds.Add(RoomData.getId());
            }

            roomIndex = 0;
            DoRoomSearchLogic();
        }
    }

    private void OnCreateRoom(bool _IsSuccess, string _RoomId)
    {
        Debug.Log("OnCreateRoom " + _IsSuccess + " " + _RoomId);
        if (_IsSuccess)
        {
            roomId = _RoomId;
            Debug.Log("Room have been created, RoomId: " + _RoomId);
            WarpClient.GetInstance().JoinRoom(roomId);
            WarpClient.GetInstance().SubscribeRoom(roomId);
        }
        else
        {
            Debug.Log("Cant Create room...");
        }
    }

    private void OnJoinRoom(bool _IsSuccess, string _RoomId)
    {
        if (_IsSuccess)
        {
            Debug.Log("Joined Room: " + _RoomId);
        }
        else Debug.Log("Failed to join Room: " + _RoomId);
    }

    private void OnGetLiveRoomInfo(LiveRoomInfoEvent eventObj)
    {
        Debug.Log("OnGetLiveRoomInfo ");
        if (eventObj != null && eventObj.getProperties() != null)
        {
            Dictionary<string, object> _properties = eventObj.getProperties();
            if (_properties.ContainsKey("Password") &&
                _properties["Password"].ToString() == passedParams["Password"].ToString())
            {
                roomId = eventObj.getData().getId();
                Debug.Log("Received Room Info, joining room: " + roomId);
                WarpClient.GetInstance().JoinRoom(roomId);
                WarpClient.GetInstance().SubscribeRoom(roomId);
            }
            else
            {
                roomIndex++;
                DoRoomSearchLogic();
            }
        }
    }

    private void OnUserJoinRoom(RoomData eventObj, string _UserName)
    {
        Debug.Log("User Joined Room " + _UserName);
        if (eventObj.getRoomOwner() == GlobalVariables.userId && GlobalVariables.userId != _UserName)
        {
            Debug.Log("Starting Game...");
            WarpClient.GetInstance().startGame();
        }
    }

    private void OnGameStarted(string _Sender, string _RoomId, string _NextTurn)
    {
        Debug.Log("Game Started, Next Turn: " + _NextTurn);
        unityObjects["SelectionUI"].SetActive(false);
        unityObjects["GameUI"].SetActive(true);
        battleLogic.StartGameLogic();

    }

    #endregion

    #region Controller

    public void Btn_PlayLogic()
    {
        Debug.Log("Btn_PlayLogic");
        WarpClient.GetInstance().GetRoomsInRange(1, 2);
        Debug.Log("Searching for an available rooms...");
        unityObjects["Selection_box_start"].GetComponent<Button>().interactable = false;
    }

    #endregion


}
