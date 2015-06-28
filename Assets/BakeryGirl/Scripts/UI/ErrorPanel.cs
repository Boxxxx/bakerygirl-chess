using UnityEngine;
using System;
using System.Collections;

public class ErrorPanel : MonoBehaviour {
    private static ErrorPanel _instance;

    public static ErrorPanel Instance { get { return _instance; } }

    public UIPanel panel;
    public UILabel info;
    public NGUIButtonHandler confirmButton;

    private Action _confirmCB = null;

    public ErrorPanel() {
        _instance = this;
    }

    void Awake() {
        confirmButton.clickedHandle += (data, index) => {
            if (_confirmCB != null) {
                _confirmCB();
            }
            panel.gameObject.SetActive(false);
        };
    }

    public static void Show(string info, Action cb = null) {
        Instance._confirmCB = cb;
        Instance.info.text = info;
        Instance.panel.gameObject.SetActive(true);
    }
}
