using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SendMsgValue : MonoBehaviour
{
    private List<float> rotsdata = new List<float>();
    public List<Quaternion> childQuatRotations = new List<Quaternion>();
    private List<Quaternion> childQuatRotations_init = new List<Quaternion>();

    private FetcherNode rotationsFetcher;
    private SocketClient socketNode;

    private NetworkManager manger;

    public int hz = 10; // 10 hz



    void Start()
    {
        rotationsFetcher = GetComponent<FetcherNode>();
        childQuatRotations_init = rotationsFetcher.FetchQuatRotations("Manus-Hand-Right");// init value

        manger = GetComponent<NetworkManager>();

        socketNode = GetComponent<SocketClient>();

        Time.fixedDeltaTime = 1.0f / hz; // set fixedupdate hz

    }

    public Vector3 ConvertEulerAnglesToRadians(Vector3 eulerAngles)
    {
        return new Vector3(
            eulerAngles.x * Mathf.Deg2Rad,
            eulerAngles.y * Mathf.Deg2Rad,
            eulerAngles.z * Mathf.Deg2Rad
        );
    }

    void FixedUpdate()
    {

        rotsdata.Clear();
        childQuatRotations = rotationsFetcher.FetchQuatRotations("Manus-Hand-Right");

        // foreach (Vector3 euler in childQuatRotations)
        for (int i = 0; i < childQuatRotations.Count; i++)
        {
            // use init minus.
            // Quaternion deltaRotation = childQuatRotations[i] * Quaternion.Inverse(childQuatRotations_init[i]);
            // Vector3 v = deltaRotation.eulerAngles;

            Vector3 v = childQuatRotations[i].eulerAngles;
            v = ConvertEulerAnglesToRadians(v);
            rotsdata.Add(v.x);
            rotsdata.Add(v.y);
            rotsdata.Add(v.z);
        }
        if (manger.isconnected)
        {
            socketNode.SendMessageToServer("FromUnity", rotsdata);
        }
    }
}
