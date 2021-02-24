using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HiScoreApp : MonoBehaviour
{
    // 按钮位置
    public Rect m_uploadBut;
    public Rect m_downLoadBut;

    // 输入框位置
    public Rect m_nameLablePos;
    public Rect m_scoreLablePos;
    public Rect m_nameTxtField;
    public Rect m_scoreTxtField;

    // 滑动框位置
    public Rect m_scrollViewPosition;
    public Vector2 m_scrollPos;
    public Rect m_scrollView;

    // 网格位置
    public Rect m_gridPos;

    public string[] m_hiscores;

    // 用户名
    protected string m_name = "";

    // 得分
    protected string m_score = "";

    public bool useRedis = false;

    public class UserData
    {
        public int id;
        public string name;
        public int score;
    }

    // Use this for initialization
    void Start()
    {
        m_hiscores = new string[20]; // 列出20个排行榜名单
    }

    // 创建UI
    void OnGUI()
    {
        GUI.Label(m_nameLablePos, "用户名");
        GUI.Label(m_scoreLablePos, "得分");
        m_name = GUI.TextField(m_nameTxtField, m_name);  // 名字输入框
        m_score = GUI.TextField(m_scoreTxtField, m_score);  // 密码输入框

        GUI.skin.button.alignment = TextAnchor.MiddleCenter;  // 设置文字对齐

        // 上传分数按钮
        if (GUI.Button(m_uploadBut, "上传"))
        {
            StartCoroutine(UploadScore(m_name, m_score));//上传分数
            m_name = "";//清空文本框
            m_score = "";
        }

        // 下载分数按钮
        if (GUI.Button(m_downLoadBut, "下载"))
        {
            StartCoroutine(DownloadScore());
        }


        GUI.skin.button.alignment = TextAnchor.MiddleLeft;  // 设置文字对齐

        m_scrollPos = GUI.BeginScrollView(m_scrollViewPosition, m_scrollPos, m_scrollView);

        m_gridPos.height = m_hiscores.Length * 30;

        // 显示分数排行榜
        GUI.SelectionGrid(m_gridPos, 0, m_hiscores, 1);

        GUI.EndScrollView();
    }

    IEnumerator UploadScore(string name, string score)
    {
        //string url = "http://127.0.0.1/UploadScore_redis.php";//本机请求地址
        string url = "http://192.168.1.102/UploadScore_redis.php";//虚拟机地址

        WWWForm form = new WWWForm();
        form.AddField("name", name);
        form.AddField("score", score);

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if(www.isNetworkError || www.isHttpError) 
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
        }
    }

    IEnumerator DownloadScore()
    {
        //string url = "http://127.0.0.1/DownloadScore_redis.php";
        string url = "http://192.168.1.102/DownloadScore_redis.php";//虚拟机地址
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();

        if(www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            try
            {
                //将PHP返回的数据解析为字典格式
                Dictionary<string, object> dict = MiniJSON.Json.Deserialize(www.downloadHandler.text) as Dictionary<string, object>;
                int index = 0;
                foreach (object v in dict.Values)
                {
                    UserData user = new UserData();
                    MiniJSON.Json.ToObject(user, v);
                    m_hiscores[index] = string.Format("ID : {0:D2}  名字 : {1}  分数 : {2}", user.id, user.name, user.score);
                    index++;
                }
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}
