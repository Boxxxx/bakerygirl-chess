using UnityEngine;
using System.Collections;

public class LastMoveHint : MonoBehaviour {
	public void Focus(Unit unit) {
        if (unit == null) {
            UnFocus();
            return;
        }
        transform.position = unit.transform.position;
        gameObject.SetActive(true);
    }

    public void UnFocus() {
        gameObject.SetActive(false);
    }
}
