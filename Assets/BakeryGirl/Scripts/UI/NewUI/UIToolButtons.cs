using UnityEngine;
using BakeryGirl.Chess;

public class UIToolButtons : MonoBehaviour {
	public void Restart() {
        GlobalInfo.Instance.controller.RestartGame(Controller.GameMode.Stay);
    }

    public void RestartVersus() {
        GlobalInfo.Instance.controller.RestartGame(Controller.GameMode.Normal);
    }

    public void RestartAI() {
        GlobalInfo.Instance.controller.RestartGame(Controller.GameMode.Agent);
    }

    public void SwitchMode() {
        var controller = GlobalInfo.Instance.controller;
        controller.RestartGame(controller.Mode == Controller.GameMode.Agent ? Controller.GameMode.Normal : Controller.GameMode.Agent);
    }

    public void ReturnToMainScene() {
        UINetwork.ReloadLevel(GlobalInfo.kMainScene, true);
    }

    public void ReturnToNetworkEntry() {
        UINetwork.ReloadLevel(GlobalInfo.kNetworkEntryScene, true);
    }
}
