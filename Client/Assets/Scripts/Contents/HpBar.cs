using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBar : MonoBehaviour
{
    // Unity�� HpBar�� Bar �κ�
    [SerializeField]
    Transform _hpBar = null;

    public void SetHpBar(float ratio)
    {
        // 0 �̸� or 1 �̻��� �� 0,1���� �ڵ� ����
        ratio = Mathf.Clamp(ratio, 0, 1);
        _hpBar.localScale = new Vector3(ratio, 1, 1);
    }
}
