using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptimizerSingleton : MonoBehaviour
{
    public static OptimizerSingleton instance;

    private Dictionary<string, OptimizerData> objectsDictionary = new Dictionary<string, OptimizerData>();

    [SerializeField] private string[] dataNames;
    [SerializeField] private float[] disappearDistances;

    [SerializeField] private int waitFrames;
    private int framesWaited;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        Application.targetFrameRate = 60;

        InitializeDictionary();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        framesWaited = 0;
    }

    private void InitializeDictionary()
    {
        if (dataNames.Length != disappearDistances.Length)
        {
            Debug.LogError("dataNames.Length != disappearDistance.Length");
            return;
        }

        for (int i = 0; i < dataNames.Length; i++)
        {
            objectsDictionary.Add(dataNames[i], new OptimizerData(disappearDistances[i]));
        }
    }

    public void AddToDictionary(string key, GameObject objectToAdd)
    {
        objectsDictionary[key].objectsList.Add(objectToAdd);
    }

    private void Update()
    {
        if (framesWaited <= waitFrames)
        {
            framesWaited++;
            return;
        }

        foreach (KeyValuePair<string, OptimizerData> pair in objectsDictionary)
        {
            for (int i = 0; i < pair.Value.objectsList.Count; i++)
            {
                if (pair.Value.objectsList[i] == null)
                {
                    continue;
                }

                //if object is too far, deactivate it
                //else, keep it activated
                pair.Value.objectsList[i].SetActive(Vector2.Distance(pair.Value.objectsList[i].transform.position, Player.instance.transform.position) < pair.Value.disappearDistance);
            }
        }
    }
}
