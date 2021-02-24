using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class WebManager : MonoBehaviour
{
    string m_info = "无数据";//数据信息
    public Texture2D m_uploadImage;//上传图片
    protected Texture2D m_downloadImage;//下载图片

    //UI界面
    private void OnGUI()
    {
        GUI.BeginGroup(new Rect(Screen.width * 0.5f - 100, Screen.height * 0.5f - 100, 500, 200));
        GUI.Label(new Rect(10, 10, 400, 30), m_info);

        if(GUI.Button(new Rect(10, 50, 150, 30), "Get Data"))
        {
            StartCoroutine(IGetData());
        }

        if(GUI.Button(new Rect(10, 100, 150, 30), "Post Data"))
        {
            StartCoroutine(IPostData());
        }

        if(m_downloadImage != null)
        {
            GUI.DrawTexture(new Rect(0, 0, m_downloadImage.width, m_downloadImage.height), m_downloadImage);
        }

        if(GUI.Button(new Rect(10, 150, 150, 30), "Request Image"))
        {
            StartCoroutine(IRequestPNG());
        }

        GUI.EndGroup();
    }

    //Get方式
    IEnumerator IGetData()//使用协程获取服务器返回数据
    {
        //UnityWebRequest 提供了一个模块化系统，用于构成 HTTP 请求和处理 HTTP 响应。
        //UnityWebRequest 系统的主要目标是让 Unity 游戏与 Web 浏览器后端进行交互。
        //该系统还支持高需求功能，例如分块 HTTP 请求、流式 POST/PUT 操作以及对 HTTP 标头和动词的完全控制。
        UnityWebRequest www =UnityWebRequest.Get("http://127.0.0.1/index.php?username=get&password=12345");
        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            m_info = www.error;
        }
        else
        {
            m_info = www.downloadHandler.text;
        }
    }

    //Post方式
    IEnumerator IPostData()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", "post");
        form.AddField("password", "6789");

        UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1/index.php", form);
        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            m_info = www.error;
        }
        else
        {
            m_info = www.downloadHandler.text;
        }
    }

    //图片的上传下载
    IEnumerator IRequestPNG()
    {
        //上传图片，将图片转换为byte[]，然后以表单的形式发送给服务器
        //byte[] ubs = File.ReadAllBytes(@"D:\UnitySources\Unity2D3DSource\rawdata\airplane\star.png");
        byte[] ubs = m_uploadImage.EncodeToJPG();
        Debug.Log(ubs.Length);
        WWWForm form = new WWWForm();
        form.AddBinaryData("picture", ubs, "screenshot", "image/png");

        //请求，downloadHandler属性要新建为DownloadHandlerTexture类型的
        UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1/index.php", form);
        //此一步极为重要！！！！！如果不做的话会一直报错
        www.downloadHandler = new DownloadHandlerTexture();
        yield return www.SendWebRequest();

        if(www.isNetworkError || www.isHttpError || www.downloadHandler.data.Length == 0)
        {
            m_info = "error";
        }
        else
        {
            m_downloadImage = DownloadHandlerTexture.GetContent(www);
        }
    }
    
    
}
