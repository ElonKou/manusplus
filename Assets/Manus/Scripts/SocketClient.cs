using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System;

public class Message
{
    public string name;

    public List<float> rots;

    public string MyToString()
    {
        string ret = "{\"name\":\"" + name + "\",\"rots\":[";
        for (int i = 0; i < rots.Count; i++)
        {
            ret = ret + rots[i].ToString("F7"); // save 7 bits for float
            if (i != rots.Count - 1)
            {
                ret = ret + ",";
            }
        }
        ret = ret + "]}";
        return ret;
    }
}

public class Data
{
    public string floatValueAsString;

    public Data(float floatValue)
    {
        floatValueAsString = floatValue.ToString("F2");
    }
}

public class SocketClient : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;

    public bool ConnectToServer(string ip, int port)
    {
        try
        {
            client = new TcpClient(ip, port);
            stream = client.GetStream();
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError("Connection failed :" + ip + ":" + port.ToString() + $", {ex.Message}");
            return false;
        }
    }

    public void SendMessageToServer(string messagename, List<float> rotsdata)
    {
        Message message = new Message
        {
            name = messagename,
            rots = rotsdata
        };
        // string json = JsonUtility.ToJson(message); // long
        string json = message.MyToString(); // short

        // Debug.Log(json); // send messages

        byte[] rawData = Encoding.UTF8.GetBytes(json + "\n"); // set "\n" as end.
        stream.Write(rawData, 0, rawData.Length);
    }

    void OnApplicationQuit()
    {
        if (stream != null) stream.Close();
        if (client != null) client.Close();
    }
}
