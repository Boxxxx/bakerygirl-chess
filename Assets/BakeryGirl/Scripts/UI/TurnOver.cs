using UnityEngine;
using System.Collections;

public class TurnOver : MonoBehaviour {

    void Start()
    {
        sprite_spark spark = gameObject.AddComponent<sprite_spark>();
        spark.isSparkAlpha = false;
        spark.workType = sprite_spark.WorkingType.UISprite;
    }

    public void OnClick()
    {
        GlobalInfo.Instance.controller.NextTurn(true);
    }

}
