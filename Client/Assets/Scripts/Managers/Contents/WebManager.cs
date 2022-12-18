using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class WebManager
{
    public string BaseUrl { get; set; } = "https://localhost:5001/api";

    [Obsolete]
    public void SendPostRequest<T>(string url, object obj, Action<T> res)
    {
        Managers.Instance.StartCoroutine(CoSendWebRequest(url,UnityWebRequest.kHttpVerbPOST,obj,res));
    }

    // obj�� req ��Ŷ�� ������ res ��Ŷ�� �޴� ����
    [Obsolete]
    IEnumerator CoSendWebRequest<T>(string url, string method, object obj, Action<T> res)
    {
        // ������ �ϴ� url
        string sendUrl = $"{BaseUrl}/{url}";

        // json ���·� ��ȯ
        byte[] jsonBytes = null;
        if(obj != null)
        {
            string jsonStr = JsonUtility.ToJson(obj);
            jsonBytes = Encoding.UTF8.GetBytes(jsonStr);
        }

        using (var uwr = new UnityWebRequest(sendUrl, method))
        {
            // ������ ����
            uwr.uploadHandler = new UploadHandlerRaw(jsonBytes);
            // �޴� ����
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            yield return uwr.SendWebRequest();

            if(uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // ���� ��Ŷ ����
                T resObj = JsonUtility.FromJson<T>(uwr.downloadHandler.text);
                res.Invoke(resObj);
            }
        }
    }
} 
