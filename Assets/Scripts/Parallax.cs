using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float startPosX, startPosY;
    public GameObject cam;
    [Range(0,1)]
    public float parallaxEffect;
    public float length;

    // Start is called before the first frame update
    void Start()
    {
        startPosX = transform.position.x;
        startPosY = transform.position.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float tempX = (cam.transform.position.x * (1 - parallaxEffect));
        float distX = (cam.transform.position.x * parallaxEffect);

        float tempY = (cam.transform.position.y * (1 - parallaxEffect));
        float distY = (cam.transform.position.y * parallaxEffect);

        transform.position = new Vector3(startPosX + distX, startPosY + distY, transform.position.z);
    
        if (tempX > startPosX + length)
        {
            startPosX += length;
        }
        else if (tempX < startPosX - length)
        {
            startPosX -= length;
        }

        if (tempY > startPosY + length)
        {
            startPosY += length;
        }
        else if (tempY < startPosY - length)
        {
            startPosY -= length;
        }
    }
}
