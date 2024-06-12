using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Resource Storage Data"), System.Serializable]
public class ResourceStorageData : ScriptableObject
{
    public string ResourceName;
    public int Current_Count;
}
