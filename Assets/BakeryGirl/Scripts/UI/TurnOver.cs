using UnityEngine;
using System.Collections;

public class TurnOver : MonoBehaviour {

    void Start()
    {
        GetComponent<UIButton>().defaultColor = new Color(1, 1, 1);
        sprite_spark spark = transform.Find("Background").GetComponent<UISprite>().gameObject.AddComponent<sprite_spark>();
        spark.isSparkAlpha = false;
        spark.workType = sprite_spark.WorkingType.UISprite;
        GetComponent<UIButton>().UpdateColor(true, true);
    }

    void OnClick()
    {
        GlobalInfo.Instance.controller.NextTurn();
    }

    void Update()
    {
        
    }
}
