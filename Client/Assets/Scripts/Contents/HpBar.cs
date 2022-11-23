using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBar : MonoBehaviour
{
    // Unity의 HpBar의 Bar 부분
    [SerializeField]
    Transform _hpBar = null;

    public void SetHpBar(float ratio)
    {
        // 0 미만 or 1 이상일 때 0,1으로 자동 세팅
        ratio = Mathf.Clamp(ratio, 0, 1);
        _hpBar.localScale = new Vector3(ratio, 1, 1);
    }
}
