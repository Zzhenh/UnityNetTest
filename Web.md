# Web-与Web服务器的交互

书上本章的内容在Windows系统上完成，我更改为在lInux虚拟机（CentOS7系统）上完成，并尝试在阿里云服务器上完成。

### 安装程序

Apache和Redis都已经装好，不在详细记录。
安装Mysql和PHP。

### php基本语法

```php
<?php   //注释为双斜杠，PHP代码必须由<?php开始
$name = "jack";//声明一个字符串变量，变量名开头为$，语句用;结束
$number = 5 * 3 + (8 / 2);//数学表达式

if($number > 100) //判断语句
{
	echo "if true"."<p>"; //echo是最常用的输出函数，连接字符串使用.
}
else
{
	echo "else false"."<p>";
}

for($i = 0; $i < 5; $i++)//for循环语句
{
	echo "number:".$i."<p>";
}

while($number > 0)//while循环
{
	echo "while:".$number."<p>";
	$number--;
}

function functionname($varname)//定义函数
{
	echo $varname;
}
functionname("hello world!<p>");//执行函数

$array = array("linux", "mac", "windows");//定义数组
//$array = ["linux", "mac", "windows"];//定义数组的另一种方式
$array[3] = "android";//添加元素
array_push($array, "iphone");//添加元素

print_r($array);//打印数组
echo '<p>';
foreach($array as &$item)//foreach遍历数组
{
	echo '<p>'.$item.'<p>';
}

$array = array("name"=>"android", "resulotion"=>"1024", "price"=>1000);//创建字典数组
$array["vender"] = "huawei";//添加更多字典元素
foreach($array as $key => $value)//foreach遍历数组，打印出每一项的键和值
{
	echo $key.':'.$value.'<br>';
}

class People //定义类
{
	public $name = "jack";//共有成员
	protected $age = 18;//保护成员
	private $money = 0;//私有成员
	
	function __construct($age)//使用关键字__construct创建类的构造函数
	{
		$this->age = $age;//赋值初始化私有成员
		echo "<p>new people</p>";
	}
	
	function Say($something)//类的成员函数
	{
		echo $this->name.":".$something."<p>";
	}
}

$p = new People(18); //创建一个对象，自动调用构造函数
$p->name = "Divad"; //修改共有成员变量
$p->Say("Hello,World!<p>"); //调用成员函数
//文件以？>结尾
?> 
```

### WWW基本应用

Unity提供了一个WWW类，用来处理基于HTTP协议的客户端和服务器的网络传输
使用HTTP协议传输数据的方式有多种，其中最常用的就是GET和POST方式，GET方式会将请求的数据附加在URL之后，POST方式则是通过FORM（表单）的形式提交。
*问题：表单是什么？表单的原理是什么？*

**两种请求方式**

1.创建脚本WebManager，指定给某一游戏体，在脚本中创建UI界面
```c#
	string m_info = "无数据";//数据信息

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

        GUI.EndGroup();
    }
```
2.使用GET请求提交数据
```c#
	//Get方式
    IEnumerator IGetData()//使用协程获取服务器返回数据
    {
        //UnityWebRequest 提供了一个模块化系统，用于构成 HTTP 请求和处理 HTTP 响应。
        //UnityWebRequest 系统的主要目标是让 Unity 游戏与 Web 浏览器后端进行交互。
        //该系统还支持高需求功能，例如分块 HTTP 请求、流式 POST/PUT 操作以及对 HTTP 标头和动词的完全控制。
        //using UnityEngine.Networking
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
```
3.POST请求
```c#
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
```
4.上传下载图片
```c#
	IEnumerator IRequestPNG()
    {
        //上传图片，将图片转换为byte[]，然后以表单的形式发送给服务器
        //byte[] ubs = File.ReadAllBytes(@"D:\UnitySources\Unity2D3DSource\rawdata\airplane\star.png");
        byte[] ubs = m_uploadImage.EncodeToPNG();
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
```
5.php代码
```php
<?php

if(isset($_GET['username']) && isset($_GET['password']))//处理Get请求
{
    echo 'username is '.$_GET['username'].' and password is '.$_GET['password'];
}
else if(isset ($_POST['username']) && isset ($_POST['password']))//处理Post请求
{
    echo 'username is '.$_POST['username'].' and password is '.$_POST['password'];
}
else if(isset($_FILES['picture']))//处理文件类型请求
{
    echo file_get_contents($_FILES['picture']['tmp_name']);
}
else
{
    echo "error!";
}
```

### 分数排行榜

1.创建数据库以及数据表
2.创建PHP脚本
```php
//UploadScore.php
<?php

//连接数据库，输入地址，用户名，密码和数据库名称

$myHandle = mysqli_connect("localhost", "root", "password", "mysocredb");

if(mysqli_connect_error())//如果连接失败
{
    echo mysqli_connect_error();
    die();//输出一条消息，并退出脚本
    exit(0);
}

//确保数据库文本使用UTF-8
mysqli_query($myHandle, "set names utf8");

//读入由Unity传来的用户名和分数，使用mydqli_real_escape_string校验用户名合法性
//问题：该方法用来转义特殊字符，为什么能防止SQL注入？
$UserID = mysqli_real_escape_string($myHandle, $_POST["name"]);//用户名
$hiscore = $_POST["score"];//分数

//向数据库中添加新数据
$sql = "insert into hiscores value(NULL,'$UserID','$hiscore')";
mysqli_query($myHandle, $sql) or die("SQL ERROR : ".$sql);

//关闭数据库
mysqli_close($myHandle);

//将结果发送给Unity
echo 'upload '.$UserID." : ".$hiscore;
?>
```
```php
//DownloadScore.php
<?php

//连接数据库，输入地址，用户名，密码和数据库名称

$myHandle = mysqli_connect("localhost", "root", "password", "mysocredb");

if(mysqli_connect_error())//如果连接失败
{
    echo mysqli_connect_error();
    die();//输出一条消息，并退出脚本
    exit(0);
}

//确保数据库文本使用UTF-8
mysqli_query($myHandle, "set names utf8");

//查询语句
$requestSQL = "SELECT * FROM hiscores ORDER BY score DESC LIMIT 20";
$result = mysqli_query($myHandle, $requestSQL) or die("SQL ERROR : ".$requestSQL);
$num_results = mysqli_num_rows($result);

//创建数组，保存查询到的数据
$arr = array();
//将查询结果写入数组
for($i = 0; $i < $num_results; $i++)
{
    $row = mysqli_fetch_array($result, MYSQLI_ASSOC);//获得一行数据
    
    $id = $row['id'];//获得ID
    $arr[$id]['id'] = $row['id'];//保存ID
    $arr[$id]['name'] = $row['name'];//保存name
    $arr[$id]['score'] = $row['score'];//保存score
}

mysqli_free_result($result);//释放SQL查询结果
mysqli_close($myHandle);//关闭数据库

//向Unity发送JSON格式数据
echo json_encode($arr);
?>
```
3.创建c#脚本，上传下载分数
```c#
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
        string url = "http://127.0.0.1/UploadScore.php";//请求地址

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
        string url = "http://127.0.0.1/DownloadScore.php";

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
```
4.使用Redis缓存数据
请求数据库操作非常慢，流行做法是将Mysql数据库中的数据写入Redis缓存中
```php
//UploadScore_redis
//上传的数据写入Redis
<?php

//连接数据库，输入地址，用户名，密码和数据库名称

$myHandle = mysqli_connect("localhost", "root", "password", "myunitysocredb");

if(mysqli_connect_error())//如果连接失败
{
    echo mysqli_connect_error();
    die();//输出一条消息，并退出脚本
    exit(0);
}

//确保数据库文本使用UTF-8
mysqli_query($myHandle, "set names utf8");

//读入由Unity传来的用户名和分数，使用mydqli_real_escape_string校验用户名合法性
//问题：该方法用来转义特殊字符，为什么能防止SQL注入？
$UserID = mysqli_real_escape_string($myHandle, $_POST["name"]);//用户名
$hiscore = $_POST["score"];//分数

//向数据库中添加新数据
$sql = "insert into hiscores value(NULL,'$UserID','$hiscore')";
mysqli_query($myHandle, $sql) or die("SQL ERROR : ".$sql);

//将数据同时写入redis中
$id = mysqli_insert_id($myHandle);//获取最后插入的id
$redis = new Redis();
$redis->connect("127.0.0.1", 6379);//连接Redis
$redis->lPush("rankid", $id);
//将数据写入Redis，键名是user.和ID编号的结合
$redis->hMset("user.$id", array("id"=>$id, "name"=>$UserID, "score"=>$hiscore));

//关闭数据库
mysqli_close($myHandle);

//将结果发送给Unity
echo 'upload '.$UserID." : ".$hiscore;
?>
```
```php
//DownloadScore_redis
//从redis下载数据
//数据库中原本的数据不会下载，这里只写了如何从redis中下载数据
<?php

$redis = new Redis();
$redis->connect("127.0.0.1", 6379);

//使用redis排序功能
//rankid是键名，BY是依据什么数据排序，SORT是排序方式，LIMIT表示排序数量，GET是排序后的返回的数据
$result = $redis->sort("rankid", array("BY"=>"user.*->score", "SORT"=>"DESC", "LIMIT"=>array(0, 20), "GET"=>array('#', "user.*->name", "user.*->score")));

//将从Redis查询出数据读入$arr中
$arr = array();
for($i = 0; $i < count($result); $i+=3)
{
    $id = $result[$i];
    $name = $result[$i+1];
    $score = $result[$i+2];
    $arr[$id]["id"] = $id;
    $arr[$id]["name"] = $name;
    $arr[$id]["score"] = $score;
}

//Json化发送给Unity
echo json_encode($arr);
?>
```

### MD5验证

1.MD5部分代码
```c#
public static string Md5Sum(string strToEncrypt)
{
	//将需要加密的字符串转换为byte数组
	byte[] bs = UTF8Encoding.UTF8.GetBytes(strToEncrypt);
	//创建MD5对象
	System.Security.Cryptography.MD5 md5;
	md5 = System.Security.Cryptography.CryptoServiceProvider.Create();
	//生成16位二进制校验码
	byte[] hashBytes = md5.ComputeHash(bs);
	
	//转为32位字符串
	string hashString = "";
	for(int i = 0; i < hashBytes.Length; i++)
	{
		hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
	}
	
	return hashString.PadLeft(32, '0');
}
```
2.方法使用
```c#
string data = "hello world";//数据
string key = "123";//密钥
Md5Sum(data + key);//返回MD5的校验码字符串
```

### 总结

1.简述HTTP协议的主要特点是什么
HTTP协议是建立在TCP协议基础上的客户端、服务端通信协议，HTTP服务端常用端口位80，服务端处理完每个连接请求后即主动断开，不去跟踪客户端的状态，因此也被称作无状态协议。HTTP协议不适合对实时性要求较高的需求，比如实时聊天
2.UNITY用什么方法实现HTTP请求
使用UnityWebRequest类实现HTTP请求。该请求何时返回取决于服务器的响应时间和网络状态。HTTP请求的附加数据通常使用GET和POST方式，两种方式都采用了类似字典的方式保存数据，GET方式可以将数据附加在URL后且有长度限制，其他方面GET和POST并无本质区别。