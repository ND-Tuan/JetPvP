using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolObjects {
    public GameObject prefab;
    public GameObject parent;
    public int Amount;
}

[System.Serializable]
public class ParentList {
    public string ListTag;
    public List<GameObject> poolObjects;
}

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }
    [SerializeField] private List<PoolObjects> Objects;
    [SerializeField] private List<ParentList> PoolList;
    
    void Awake()
    {
        Instance =this;
        PoolList = new List<ParentList>();
    
        // Khởi tạo danh sách các đối tượng trong pool
        foreach (PoolObjects obj in Objects)
        {
            ParentList parentListElement = new();
            parentListElement.ListTag = obj.prefab.tag;
            parentListElement.poolObjects = new List<GameObject>();
    
            // Tạo các đối tượng và thêm vào danh sách pool
            for (int i = 0; i < obj.Amount; i++)
            {
                GameObject ObjTmp = Instantiate(obj.prefab, obj.parent.transform);
                ObjTmp.SetActive(false);
                parentListElement.poolObjects.Add(ObjTmp);
            }
            Instance.PoolList.Add(parentListElement);
        }
        
    }
    
    // Lấy object từ pool
    public GameObject GetObject(string tag)
    {   
        if(GetAllObjects(tag).Count == 0){
            for(int i = 0; i < PoolList.Count; i++){
                if(PoolList[i].ListTag == tag){
                    GameObject ObjTmp = Instantiate(Objects[i].prefab, Objects[i].parent.transform);
                    ObjTmp.SetActive(false);
                    PoolList[i].poolObjects.Add(ObjTmp);
                    return ObjTmp;
                }
            }
        }
        // Tìm đối tượng không hoạt động trong pool theo tag
        foreach (ParentList list in PoolList)
        {
            if (list.ListTag == tag)
            {
                for (int i = 0; i < list.poolObjects.Count; i++)
                {
                    if (!list.poolObjects[i].activeInHierarchy)
                        return list.poolObjects[i];
                }
            }
        }
        return null; // Trả về null nếu không tìm thấy đối tượng phù hợp
    }

    // Lấy list objects từ pool
    public List<GameObject> GetAllObjects(string tag){
        List<GameObject> gameObjects = new();
        foreach (ParentList list in PoolList)
        {
            if (list.ListTag == tag)
            {
                for (int i = 0; i < list.poolObjects.Count; i++)
                {
                    if (!list.poolObjects[i].activeInHierarchy)
                        gameObjects.Add(list.poolObjects[i]);
                }
            }
        }
        return gameObjects;
    }
}
