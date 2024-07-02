using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MyFunctions;

[RequireComponent(typeof(RectTransform))]
public class UILineRenderer : MonoBehaviour
{
    [SerializeField] private Sprite pointSprite;
    [SerializeField] private Color pointColor = Color.white;
    [SerializeField] private Vector2 pointDimensions;

    [SerializeField] private Sprite connectionSprite;
    [SerializeField] private Color connectionColor = Color.white;
    [SerializeField] private float connectionWidth;

    [SerializeField] private List<Vector2> positions;
    private List<GameObject> createdPoints = new List<GameObject>();
    private List<GameObject> createdConnections = new List<GameObject>();

    private Transform pointsParent;
    private Transform connectionsParent;

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            UpdateLine();
        }
#endif
    }

    private void Awake()
    { 
        connectionsParent = CreateParent("ConnectionsParent");
        pointsParent = CreateParent("PointsParent");

        ShowLine();
    }

    private Transform CreateParent(string name)
    {
        GameObject p = new GameObject(name, typeof(RectTransform));
        p.transform.SetParent(transform, false);  
        return p.transform;
    }

    public void SetPosition(int index, Vector2 pos)
    {
        positions[index] = pos;
        UpdateLine();
    }

    public void SetPositions(List<Vector2> list)
    {
        positions = new List<Vector2>(list);
        UpdateLine();
    }

    private void CreatePoint(Vector2 anchoredPosition)
    {
        GameObject point = new GameObject("Point", typeof(Image));

        point.transform.SetParent(pointsParent, false);
        Image img = point.GetComponent<Image>();
        if (pointSprite != null)
        {
            img.sprite = pointSprite;
            img.color = pointColor;
        }
        else
        {
            img.enabled = false;
        }

        RectTransform rectTransform = point.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = pointDimensions;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

        createdPoints.Add(point);
    }

    private void CreatePoint(float x, float y)
    {
        CreatePoint(new Vector2(x, y));
    }

    private void CreateConnection(Vector2 pointA, Vector2 pointB)
    {
        GameObject connection = new GameObject("Connection", typeof(Image));

        connection.transform.SetParent(connectionsParent, false);
        Image img = connection.GetComponent<Image>();
        if (connectionSprite != null)
        {
            img.sprite = connectionSprite;
            img.color = connectionColor;
        }
        else
        {
            img.enabled = false;
        }

        RectTransform rectTransform = connection.GetComponent<RectTransform>();

        Vector2 dir = (pointB - pointA).normalized;
        float distance = Vector2.Distance(pointA, pointB);
        
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(distance, connectionWidth);
        rectTransform.anchoredPosition = pointA + dir * distance * 0.5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, VectorToFloat(dir));

        createdConnections.Add(connection);
    }

    private void UpdateLine()
    {
        ClearLine();
        ShowLine();
    }

    private void ClearPoints()
    {
        for (int i = 0; i < createdPoints.Count; i++)
        {
            Destroy(createdPoints[i]);
        }
        createdPoints.Clear();
    }

    private void ClearConnections()
    {
        for (int i = 0; i < createdConnections.Count; i++)
        {
            Destroy(createdConnections[i]);
        }
        createdConnections.Clear();
    }

    private void ClearLine()
    {
        ClearPoints();
        ClearConnections();
    }

    private void ShowLine()
    {
        for (int i = 0; i < positions.Count; i++)
        {
            float xPosition = positions[i].x;
            float yPosition = positions[i].y;

            CreatePoint(xPosition, yPosition);

            if (i > 0)
            {
                CreateConnection(createdPoints[i - 1].GetComponent<RectTransform>().anchoredPosition, createdPoints[i].GetComponent<RectTransform>().anchoredPosition);
            }
        }
    }
}
