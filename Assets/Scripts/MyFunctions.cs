using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public static class MyFunctions 
{
    public static void SetPosition(Transform obj, int posIndex, float value)
    {
        float[] newPos = new float[3];
        for (int i = 0; i < newPos.Length; i++)
        {
            if (i == posIndex)
            {
                newPos[i] = value;
            }
            else if (i == 0)
            {
                newPos[i] = obj.position.x;
            }
            else if (i == 1)
            {
                newPos[i] = obj.position.y;
            }
            else if (i == 2)
            {
                newPos[i] = obj.position.z;
            }
        }
        obj.position = new Vector3(newPos[0], newPos[1], newPos[2]);
    }

    public static void SetScale(Transform obj, int scaleIndex, float value)
    {
        float[] newScale = new float[3];
        for (int i = 0; i < newScale.Length; i++)
        {
            if (i == scaleIndex)
            {
                newScale[i] = value;
            }
            else if (i == 0)
            {
                newScale[i] = obj.localScale.x;
            }
            else if (i == 1)
            {
                newScale[i] = obj.localScale.y;
            }
            else if (i == 2)
            {
                newScale[i] = obj.localScale.z;
            }
        }
        obj.localScale = new Vector3(newScale[0], newScale[1], newScale[2]);
    }

    public static void SetRotation(Transform obj, int rotateIndex, float value)
    {
        float[] newRotation = new float[3];
        for (int i = 0; i < newRotation.Length; i++)
        {
            if (i == rotateIndex)
            {
                newRotation[i] = value;
            }
            else if (i == 0)
            {
                newRotation[i] = obj.rotation.x;
            }
            else if (i == 1)
            {
                newRotation[i] = obj.rotation.y;
            }
            else if (i == 2)
            {
                newRotation[i] = obj.rotation.z;
            }
        }
        obj.rotation = Quaternion.Euler(newRotation[0], newRotation[1], newRotation[2]);
    }

    public static float VectorToFloat(Vector2 direction)
    {
        double dX = direction.x;
        double dY = direction.y;
        double newDirection = Math.Atan2(dY, dX);
        newDirection = newDirection * (180 / Math.PI);
        float floatNewDirection = (float)newDirection;
        return floatNewDirection;
    }

    public static Vector2 GetRandomPointInCollider(Collider2D collider)
    {
        Vector2 point;

        Bounds bounds = collider.bounds;

        point = new Vector2(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 
            UnityEngine.Random.Range(bounds.min.y, bounds.max.y));

        point = collider.ClosestPoint(point);

        return point;
    }

    public static T GetComponentInHierarchy<T>(Transform t)
    {
        if (t.root.GetComponentInChildren<T>() != null)
        {
            return t.root.GetComponentInChildren<T>();
        }
        return default(T);
    }

    public static T GetComponentInHierarchy<T>(Transform t, bool startAtCurrent)
    {
        if (startAtCurrent && t.GetComponent<T>() != null)
        {
            return t.GetComponent<T>();           
        }
        return GetComponentInHierarchy<T>(t);
    }

    public static T[] GetComponentsInHierarchy<T>(Transform t)
    {
        List<T> components = new List<T>();
        foreach(T child in t.root.GetComponentsInChildren<T>())
        {
            components.Add(child);
        }
        return components.ToArray();
    }

    public static void SetGlobalScale(Transform t, Vector3 globalScale)
    {
        t.localScale = Vector3.one;
        t.localScale = new Vector3(globalScale.x / t.lossyScale.x, globalScale.y / t.lossyScale.y, globalScale.z / t.lossyScale.z);
    }

    public static double CubeRoot(float number)
    {
        return Math.Pow(number, (float)1 / 3);
    }

    public static void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) 
    {
        Vector3 dir = point - pivot;
        dir = Quaternion.Euler(angles) * dir;
        point = dir + pivot;
        return point;
    }
}
