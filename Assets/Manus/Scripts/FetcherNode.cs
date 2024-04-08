using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FetcherNode : MonoBehaviour
{
    public List<Quaternion> FetchQuatRotations(string nodeName, bool islocal = true)
    {
        // get local deformation default : local
        Transform rootNode = GameObject.Find(nodeName)?.transform;
        rootNode = rootNode.Find("SK_Hand")?.transform;
        rootNode = rootNode.Find("root")?.transform;

        List<Quaternion> quatRotations = new List<Quaternion>();

        if (rootNode == null)
        {
            Debug.LogError("No GameObject found with the name: " + nodeName);
            return null;
        }

        GetChildRotations(rootNode, quatRotations, islocal);
        return quatRotations;
    }

    private bool iscontainsStr(Transform obj)
    {
        bool ret = false;
        ret = obj.name.Contains("antiScale");
        return ret;
    }

    private void GetChildRotations(Transform currentNode, List<Quaternion> quatRotations, bool islocal)
    {
        // recursive get all ratation
        foreach (Transform child in currentNode)
        {
            if (!iscontainsStr(child))
            {

                if (islocal)
                {
                    quatRotations.Add(child.localRotation);
                }
                else
                {
                    quatRotations.Add(child.rotation);
                }
            }
            GetChildRotations(child, quatRotations, islocal);
        }
    }

    public void SetAllChildZero(string nodeName, bool setall = true)
    {
        // setall will set all node's local rotation = 0
        Transform rootNode = GameObject.Find(nodeName)?.transform;
        if (!setall)
        {
            rootNode = rootNode.Find("SK_Hand")?.transform;
            rootNode = rootNode.Find("root")?.transform;
        }
        rootNode.localEulerAngles = Vector3.zero;


        if (rootNode == null)
        {
            Debug.LogError("No GameObject found with the name: " + nodeName);
        }
        else
        {
            SetChildZero(rootNode);
        }

    }

    private void SetChildZero(Transform currentNode)
    {
        foreach (Transform child in currentNode)
        {
            child.localEulerAngles = Vector3.zero;
            SetChildZero(child);
        }
    }

}