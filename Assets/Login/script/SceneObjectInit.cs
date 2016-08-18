using UnityEngine;
using System.Collections.Generic;

public class SceneObjectInit : MonoBehaviour {
    [System.Serializable]
    public class ObjectInit
    {
        public GameObject obj;
        public bool bActive;
    }

    public List<ObjectInit> list;

    void Start()
    {
        if(list != null)
        {
            foreach(ObjectInit init in list)
            {
                
                init.obj.SetActive(true);
                if (!init.bActive)
                    init.obj.SetActive(false);
            }
        }
    }
}
