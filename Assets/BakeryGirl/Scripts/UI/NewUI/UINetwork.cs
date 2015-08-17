using BakeryGirl.Chess;
using ExitGames.Client.Photon.LoadBalancing;
using UnityEngine;
using UnityEngine.UI;

public class UINetwork : MonoBehaviour {
    private const string kUserNamePlayerPref = "";
    private const string kDefaultUserName = "Jefuty";
    private const float kEnterGameDelay = 1f;

    public enum NetworkState {
        None, MainEntry, Connecting, WaitingForOpponent, Timeout, GamePlay
    };

    public Text timecostLabel;
    public Text infoLabel;
    public Button exitButton;

    public RectTransform matchPanel;
    public InputField usernameInput;
    public InputField passwordInput;

    public float timeLimit = 300;

    private float _passtime = 0;
    private NetworkState _state = NetworkState.None;

    private string Username {
        get {
            return usernameInput.text;
        }
        set {
            usernameInput.text = value;
        }
    }

    private string RoomPwd {
        get { return passwordInput.text; }
    }

    public NetworkState CurrentState {
        get { return _state; }
        private set {
            if (_state != value) {
                var oldState = _state;
                _state = value;
                OnSwitchState(_state, oldState);
            }
        }
    }

    public float PassTime {
        get { return _passtime; }
        set {
            _passtime = value;
            timecostLabel.text = string.Format("{0} : {1}", ((int)_passtime / 60).ToString("D2"), ((int)_passtime % 60).ToString("D2"));
        }
    }

    #region Private
    void RegisterClientCBs() {
        GameClientAgent.ClientInstance.onDisconnect += OnDisconnect;
        GameClientAgent.ClientInstance.onJoinLobby += OnJoinLobby;
        GameClientAgent.ClientInstance.onJoinRoom += OnJoinRoom;
        GameClientAgent.ClientInstance.onCreateRoom += OnCreateRoom;
        GameClientAgent.ClientInstance.onCloseRoom += OnCloseRoom;
        GameClientAgent.ClientInstance.onGameStart += OnGameStart;
        GameClientAgent.ClientInstance.onError += OnError;
    }

    void EnterGame() {
        UISwitchMode.ReloadLevel(GameInfo.kMultiplayerScene, false);
    }
    #endregion

    #region Event Callbacks
    public void OnMatch() {
        PlayerPrefs.SetString(kUserNamePlayerPref, Username);
        if (string.IsNullOrEmpty(Username)) {
            UIErrorPrompt.Show("Username can not be empty.", () => {});
        }
        else {
            GameClientAgent.Instance.JoinGame(Username);
            CurrentState = NetworkState.Connecting;
        }
    }

    public void OnExit() {
        if (CurrentState == NetworkState.MainEntry) {
            UISwitchMode.ReloadLevel(GameInfo.kMainScene, true);
        }
        else {
            UISwitchMode.ReloadLevel(GameInfo.kNetworkEntryScene, true);
        }
    }

    void OnSwitchState(NetworkState newState, NetworkState oldState) {
        switch (_state) {
            case NetworkState.MainEntry:
                matchPanel.gameObject.SetActive(true);
                exitButton.gameObject.SetActive(true);
                infoLabel.text = "战术联网系统";
                PassTime = 0;
                break;
            case NetworkState.Connecting:
                matchPanel.gameObject.SetActive(false);
                exitButton.gameObject.SetActive(true);
                infoLabel.text = "连接网络中...";
                break;
            case NetworkState.WaitingForOpponent:
                matchPanel.gameObject.SetActive(false);
                exitButton.gameObject.SetActive(true);
                infoLabel.text = "等待对手中...";
                break;
            case NetworkState.GamePlay:
                matchPanel.gameObject.SetActive(false);
                exitButton.gameObject.SetActive(false);
                infoLabel.text = "准备开战...";
                Invoke("EnterGame", kEnterGameDelay);
                break;
        }
    }

    void OnDisconnect(DisconnectCause disconnectCause) {
        if (disconnectCause == DisconnectCause.None) {
            return;
        }
        UIErrorPrompt.Show("Disonnected.", () => {
            Debug.Log("disconnected, cause: " + disconnectCause);
            UISwitchMode.ReloadLevel(GameInfo.kNetworkEntryScene, true);
        });
    }

    void OnJoinLobby(TypedLobby lobby) {
        if (CurrentState != NetworkState.Connecting) {
            // If it is not in connecting state, return
            return;
        }
        LogRoomList();
        GameClientAgent.ClientInstance.JoinRandomRoom(RoomPwd);
    }

    void OnJoinRoom(Room room) {
        Debug.Log("joined game " + room.Name);
        CurrentState = NetworkState.WaitingForOpponent;
    }

    void OnCreateRoom() {
        GameClientAgent.ClientInstance.CreateTurnbasedRoom(RoomPwd);
    }

    void OnCloseRoom() {
        UIErrorPrompt.Show("Player has left.", () => {
            UISwitchMode.ReloadLevel(GameInfo.kNetworkEntryScene, true);
        });
    }

    void OnGameStart() {
        Debug.Log("game start");
        CurrentState = NetworkState.GamePlay;
    }

    void OnError(Consts.ErrorCode err) {
        switch (err) {
            case Consts.ErrorCode.NotCompatible:
                Debug.Log("game data not compatible");
                UIErrorPrompt.Show("Game data not compatible.", () => {
                    UISwitchMode.ReloadLevel(GameInfo.kNetworkEntryScene, true);
                });
                break;
        }
    }
    #endregion

    #region Unity Callbacks
    void Start() {
        // load stored nickname in playerPrefs
        string prefsName = PlayerPrefs.GetString(kUserNamePlayerPref);
        if (!string.IsNullOrEmpty(prefsName)) {
            Username = prefsName;
        }
        else {
            Username = kDefaultUserName;
        }

        RegisterClientCBs();

        CurrentState = NetworkState.MainEntry;
    }
	
	void Update () {
	    if (_state == NetworkState.Connecting || _state == NetworkState.WaitingForOpponent) {
            PassTime += Time.deltaTime;
            if (PassTime > timeLimit) {
                _state = NetworkState.Timeout;
                UIErrorPrompt.Show("Timeout.", () => {
                    UISwitchMode.ReloadLevel(GameInfo.kNetworkEntryScene, true);
                });
            }
        }
	}
    #endregion

    #region Debug
    void LogRoomList() {
        string roomList = "";
        bool isFirst = true;
        foreach (var roomInfo in GameClientAgent.ClientInstance.RoomInfoList) {
            if (isFirst) {
                isFirst = false;
            }
            else {
                roomList += ", ";
            }
            roomList += roomInfo.Value.Name;
        }
        Debug.Log("RoomList: [" + roomList + "]");
    }
    #endregion
}
