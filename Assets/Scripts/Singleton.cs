using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace David.singletons
{
    public class Singleton<T> : MonoBehaviour
        where T : Component
    {
        private static T _instance;
        public static T Instance 
        { 
            get 
            { 
                if (_instance == null)
                {
                    var objs = FindObjectOfType(typeof(T)) as T[];
                    if (objs.Length > 0)
                        _instance = objs[0];
                    if (objs.Length > 1)
                    {
                        Debug.LogError("Debug Error");
                    }
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = string.Format("_{0}", typeof(T).Name);
                        _instance = obj.AddComponent<T>();
                    }
                }
                return _instance; 
            } 
        
        
        
        }
    }
}