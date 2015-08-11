using UnityEngine;
using System.Collections;

public class LastMoveHint : MonoBehaviour {
    public float tweenDistance = 0.2f;
    public float tweenTime = 0.7f;
    public SpriteRenderer directionHint;

	public void Focus(LastMoveInfo lastMove) {
        if (lastMove == null) {
            UnFocus();
            return;
        }
        transform.position = Unit.PosToScreen(lastMove.from);
        gameObject.SetActive(true);

        directionHint.transform.rotation = Quaternion.Euler(new Vector3(0, 0, _GetDirectionAngle(lastMove)));
        var offset = _GetTweenOffset(lastMove);
        directionHint.transform.localPosition = -offset * .5f;
        iTween.MoveTo(directionHint.gameObject,
            iTween.Hash(
                "position", offset * .5f,
                "time", tweenTime,
                "looptype", iTween.LoopType.pingPong,
                "easetype", iTween.EaseType.easeOutCubic,
                "islocal", true));
    }

    public void UnFocus() {
        gameObject.SetActive(false);
        iTween.Stop(directionHint.gameObject);
    }

    Vector3 _GetTweenOffset(LastMoveInfo lastMove) {
        var offset = lastMove.GetOffset() * tweenDistance;
        return offset;
    }

    float _GetDirectionAngle(LastMoveInfo lastMove) {
        if (lastMove.from.R == lastMove.to.R) {
            if (lastMove.from.C + 1 == lastMove.to.C) {
                return 0f;
            }
            else {
                return 180f;
            }
        }
        else if (lastMove.from.R + 1 == lastMove.to.R) {
            return 90f;
        }
        else {
            return -90f;
        }
    }
}
