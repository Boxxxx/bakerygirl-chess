using UnityEngine;
using System.Collections;

public class Restart : MonoBehaviour {

    void Start()
    {
        GetComponent<UIButton>().defaultColor = new Color(0, 0, 0);
        GetComponent<UIButton>().UpdateColor(true, true);
    }

    void OnClick()
    {
        GameInfo.Instance.controller.RestartGame(Controller.GameMode.Stay);
    }
}
