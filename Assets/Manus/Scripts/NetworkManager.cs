using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour
{
    public InputField ipInputField;
    public InputField portInputField;
    public Button connectButton;
    public Text conText; // for connected flag.
    public Text stausText; // for all status.
    public Button myButton;

    // connected informations.
    private FetcherNode rotationsFetcher;
    private SocketClient socketNode;
    private SendMsgValue sender;

    public string ipAddress = "127.0.0.1"; // dafault IP:127.0.0.1
    public int port = 12345; // default port: 12345
    public bool isconnected = false;



    void Start()
    {
        ipInputField.text = ipAddress;
        portInputField.text = port.ToString();

        // 注册输入事件监听
        ipInputField.onEndEdit.AddListener(delegate { OnEndEditIP(ipInputField.text); });
        portInputField.onEndEdit.AddListener(delegate { OnEndEditPort(portInputField.text); });

        // 注册点击事件监听
        connectButton.onClick.AddListener(OnConnectButtonClick);

        myButton.onClick.AddListener(OnMyFunction);

        // 初始化conText
        conText.text = "NO";
        conText.color = Color.red;

        socketNode = GetComponent<SocketClient>();
        sender = GetComponent<SendMsgValue>();

        rotationsFetcher = GameObject.Find("allscripts_node").GetComponent<FetcherNode>();
    }

    private void OnEndEditIP(string ip)
    {
        ipAddress = ip;
        AppendMessageToScrollView($"IP set to: {ipAddress}", 0);
    }

    private void OnEndEditPort(string portInput)
    {
        if (int.TryParse(portInput, out int parsedPort))
        {
            port = parsedPort;
            AppendMessageToScrollView($"Port set to: {port}", 0);
        }
        else
        {
            AppendMessageToScrollView("Invalid Port Number.", 1);
        }
    }

    private void OnConnectButtonClick()
    {
        ConnectToServer(ipAddress, port);
    }

    private void OnMyFunction()
    {
        if (rotationsFetcher != null)
        {
            rotationsFetcher.SetAllChildZero("Manus-Hand-Right"); // set all child to zero.
            AppendMessageToScrollView("Reset rotation.", 0);
            Debug.Log("Reset rotation");
        }
    }

    private void ConnectToServer(string ip, int port)
    {
        isconnected = false;
        try
        {
            isconnected = socketNode.ConnectToServer(ip, port);

            if (isconnected)
            {
                AppendMessageToScrollView("Connection established :" + ip + ":" + port.ToString(), 2);
                Debug.Log("Connection established :" + ip + ":" + port.ToString());
                conText.text = "OK";
                conText.color = Color.green;
            }
            else
            {
                AppendMessageToScrollView($"Failed to connect: " + ip + ":" + port.ToString(), 1);
                Debug.LogError("Failed to connect :" + ip + ":" + port.ToString());
                conText.text = "NO";
                conText.color = Color.red;
            }
        }
        catch (Exception ex)
        {
            AppendMessageToScrollView($"Failed to connect: {ex.Message}, at " + ip + ":" + port.ToString(), 1);
            Debug.LogError("Failed to connect :" + ip + ":" + port.ToString());
            conText.text = "NO";
            conText.color = Color.red;
        }
    }

    private void AppendMessageToScrollView(string message, int idx = 0)
    {
        // idx=0 (black)
        // idx=1 (red)
        // idx=2 (green)

        // Text contentText = scrollView.content.GetComponentInChildren<Text>();
        // Text scrollViewText = scrollView.content.GetComponentInChildren<Text>();

        stausText.text = message;
        if (idx == 0)
        {
            stausText.color = Color.black;
        }
        else if (idx == 1)
        {
            stausText.color = Color.red;
        }
        else
        {
            stausText.color = Color.green;
        }
    }

}