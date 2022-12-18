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

    // obj의 req 패킷을 보내서 res 패킷을 받는 역할
    [Obsolete]
    IEnumerator CoSendWebRequest<T>(string url, string method, object obj, Action<T> res)
    {
        // 보내야 하는 url
        string sendUrl = $"{BaseUrl}/{url}";

        // json 형태로 변환
        byte[] jsonBytes = null;
        if(obj != null)
        {
            string jsonStr = JsonUtility.ToJson(obj);
            jsonBytes = Encoding.UTF8.GetBytes(jsonStr);
        }

        using (var uwr = new UnityWebRequest(sendUrl, method))
        {
            // 보내는 버퍼
            uwr.uploadHandler = new UploadHandlerRaw(jsonBytes);
            // 받는 버퍼
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            yield return uwr.SendWebRequest();

            if(uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // 응답 패킷 수신
                T resObj = JsonUtility.FromJson<T>(uwr.downloadHandler.text);
                res.Invoke(resObj);
            }
        }
    }
} 
