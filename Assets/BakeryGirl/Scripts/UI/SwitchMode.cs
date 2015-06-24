using UnityEngine;
using System.Collections;

public class SwitchMode : MonoBehaviour {

    private Controller controller;

    void RefreshText()
    {
        if (GlobalInfo.Instance.controller.Mode == Controller.GameMode.Normal)
            transform.Find("Label").GetComponent<UILabel>().text = "切换到 人机对战";
        else
            transform.Find("Label").GetComponent<UILabel>().text = "切换到 双人对战";
        transform.Find("Label").GetComponent<UILabel>().Update();
    }

    void Start()
    {
        GetComponent<UIButton>().defaultColor = new Color(0, 0, 0);
        GetComponent<UIButton>().UpdateColor(true, true);

        RefreshText();
    }

    void OnClick()
    {
        controller = GlobalInfo.Instance.controller;
        controller.RestartGame(controller.Mode == Controller.GameMode.Normal ? Controller.GameMode.AI : Controller.GameMode.Normal);

        RefreshText();
    }
}
