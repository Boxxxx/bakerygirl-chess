using UnityEngine;
using UnityEngine.UI;
using System;

public class UIErrorPrompt : MonoBehaviour {
    private static UIErrorPrompt _instance;

    public static UIErrorPrompt Instance { get { return _instance; } }

    public Text infoLabel;

    private Action _confirmCB = null;

    public UIErrorPrompt() {
        _instance = this;
    }

    public void OnExit() {
        if (_confirmCB != null) {
            _confirmCB();
        }
        gameObject.SetActive(false);
    }

    public static void Show(string info, Action cb = null) {
        _instance._confirmCB = cb;
        _instance.infoLabel.text = info;
        _instance.gameObject.SetActive(true);
    }
}
