using UnityEngine;
using UnityEngine.UI;
using BakeryGirl.Chess;

public class UISwitchMode : MonoBehaviour {
    public Image pveModeLabel;
    public Image pvpModeLabel;
    public Image netModeLabel;
    public Button exitButton;

    private bool IsNetworkMode {
        get {
            return
                GameInfo.Instance.controller.Mode == Controller.GameMode.Agent
                && GameInfo.Instance.controller.Agent is GameClientAgent;
        }
    }

    private bool IsPvEMode {
        get {
            return
                GameInfo.Instance.controller.Mode == Controller.GameMode.Agent
                && GameInfo.Instance.controller.Agent is AIPlayer;
        }
    }

    private bool IsPvPMode {
        get {
            return GameInfo.Instance.controller.Mode == Controller.GameMode.Normal;
        }
    }

    public void Restart() {
        if (!IsNetworkMode) {
            GameInfo.Instance.controller.RestartGame(Controller.GameMode.Stay);
        }
        else {
            SwitchToNet();
        }
    }

    public void SwitchToPvE() {
        if (IsNetworkMode) {
            GameInfo.nextMode = Controller.GameMode.Agent;
            ReturnToMainScene();
        }
        else {
            GameInfo.Instance.controller.RestartGame(Controller.GameMode.Agent);
        }
    }

    public void SwitchToPvP() {
        if (IsNetworkMode) {
            GameInfo.nextMode = Controller.GameMode.Normal;
            ReturnToMainScene();
        }
        else {
            GameInfo.Instance.controller.RestartGame(Controller.GameMode.Normal);
        }
    }

    public void SwitchToNet() {
        ReloadLevel(GameInfo.kNetworkEntryScene, true);
    }

    public void OnExit() {
        if (IsNetworkMode) {
            SwitchToNet();
        }
    }

    public void ReturnToMainScene() {
        ReloadLevel(GameInfo.kMainScene, true);
    }

    public static void ReloadLevel(string sceneName, bool destroyClient) {
        if (destroyClient && GameInfo.Instance != null && GameInfo.Instance.controller != null) {
            GameInfo.Instance.controller.DestroyAgent();
        }
        Application.LoadLevel(sceneName);
    }

    public void NewGame() {
        exitButton.interactable = IsNetworkMode;
        pveModeLabel.gameObject.SetActive(false);
        pvpModeLabel.gameObject.SetActive(false);
        netModeLabel.gameObject.SetActive(false);
        if (IsNetworkMode) {
            netModeLabel.gameObject.SetActive(true);
        }
        else if (IsPvEMode) {
            pveModeLabel.gameObject.SetActive(true);
        }
        else {
            pvpModeLabel.gameObject.SetActive(true);
        }
    }
}
