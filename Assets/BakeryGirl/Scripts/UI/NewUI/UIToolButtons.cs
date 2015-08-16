using UnityEngine;
using BakeryGirl.Chess;

public class UIToolButtons : MonoBehaviour {
	public void Restart() {
        GameInfo.Instance.controller.RestartGame(Controller.GameMode.Stay);
    }

    public void RestartVersus() {
        GameInfo.Instance.controller.RestartGame(Controller.GameMode.Normal);
    }

    public void RestartAI() {
        GameInfo.Instance.controller.RestartGame(Controller.GameMode.Agent);
    }

    public void SwitchMode() {
        var controller = GameInfo.Instance.controller;
        controller.RestartGame(controller.Mode == Controller.GameMode.Agent ? Controller.GameMode.Normal : Controller.GameMode.Agent);
    }

    public void ReturnToMainScene() {
        UINetwork.ReloadLevel(GameInfo.kMainScene, true);
    }

    public void ReturnToNetworkEntry() {
        UINetwork.ReloadLevel(GameInfo.kNetworkEntryScene, true);
    }
}
