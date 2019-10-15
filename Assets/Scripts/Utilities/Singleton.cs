using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static bool _isApplicationClosing = false;
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null && !_isApplicationClosing)
            {
                _instance = (T)FindObjectOfType(typeof(T));

                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    _instance = singletonObject.AddComponent<T>();
                    singletonObject.name = typeof(T).ToString() + " (Singleton)";
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return _instance;
        }
    }
    private void Start()
    {
        if (_instance != null && _instance != this) Destroy(this);
    }

    private void OnApplicationQuit()
    {
        _isApplicationClosing = true;
    }
}