using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InstanceReusePool : MonoBehaviour {

    public delegate void OnItemChangeUsage(GameObject obj);

    public Object instance_prefab;
    public bool autoFillIndex = false;

    public bool autoAdjustPos = false;

    public Vector3 slots_beginPos;
    public Vector2 slots_offsetPos;
    public int slots_maxPerLineOrRow = 1;
    public bool horizontal = true;

    public OnItemChangeUsage onItemDequeue = null;
    public OnItemChangeUsage onItemEnqueue = null;

    public T Get<T>(int i) where T : Component {
        return this[i].GetComponent<T>();
    }
    public GameObject this[int i] {
        get {
            if (i >= 0 && i < instancesInUse.Count)
                return instancesInUse[i];
            if (!autoFillIndex || i < 0) {
                Debug.LogWarning("You're trying to access the index " + i.ToString() + " in pool " + name + ", which does not exit or over its item count.");
                return null;
            }
            else if (autoFillIndex) {
                MakeItems(i + 1);
                return instancesInUse[i];
            }
            return null;
        }
    }
    public int Count {
        get {
            return instancesInUse.Count;
        }
    }

    public void MakeItems(int count) {
        if (count < 0) count = 0;
        while (count < instancesInUse.Count) {
            var obj = instancesInUse[instancesInUse.Count - 1];
            if (onItemDequeue == null)
                obj.SetActive(false);
            else
                onItemDequeue(obj);
            instancesNotInUse.Add(obj);
            instancesInUse.RemoveAt(instancesInUse.Count - 1);
        }

        while (count > instancesInUse.Count) {
            if (instancesNotInUse.Count == 0)
                CreateInstance();
            var obj = instancesNotInUse[instancesNotInUse.Count - 1];

            if (onItemEnqueue == null)
                obj.SetActive(true);
            else
                onItemEnqueue(obj);

            instancesInUse.Add(obj);
            instancesNotInUse.Remove(obj);
        }
        if (autoAdjustPos)
            for (int i = 0; i < count; i++)
                this[i].transform.localPosition = GetOffsetPos(i);
    }

    public GameObject AllocateObject(bool setActive = true) {
        if (instancesNotInUse.Count == 0)
            CreateInstance();
        var ret = instancesNotInUse[0];
        instancesInUse.Add(ret);
        instancesNotInUse.Remove(ret);
        if (setActive)
            ret.SetActive(true);
        if (onItemEnqueue != null)
            onItemEnqueue(ret);
        return ret;
    }
    public void RecycleObject(GameObject obj, bool setInActive = true) {
        if (setInActive)
            obj.SetActive(false);
        if (instancesInUse.Contains(obj)) {
            instancesNotInUse.Add(obj);
            instancesInUse.Remove(obj);
            if (onItemDequeue != null)
                onItemDequeue(obj);
        }
    }

    public Vector3 GetOffsetPos(int index) {
        return slots_beginPos + new Vector3(slots_offsetPos.x * (horizontal ? (index % slots_maxPerLineOrRow) : (index / slots_maxPerLineOrRow)),
                                        slots_offsetPos.y * (horizontal ? (index / slots_maxPerLineOrRow) : (index % slots_maxPerLineOrRow)),
                                        0);
    }

    List<GameObject> instancesInUse = new List<GameObject>();
    List<GameObject> instancesNotInUse = new List<GameObject>();


    void CreateInstance() {
        GameObject gameObj = Object.Instantiate(instance_prefab) as GameObject;
        gameObj.transform.parent = transform;
        gameObj.transform.localRotation = ((GameObject)instance_prefab).transform.localRotation;
        gameObj.transform.localScale = ((GameObject)instance_prefab).transform.localScale;
        instancesNotInUse.Add(gameObj);
    }


    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
