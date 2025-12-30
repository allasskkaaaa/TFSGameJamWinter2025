using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    public float minXClamp;
    public float maxXClamp;
    public float minYClamp;
    public float maxYClamp;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void LateUpdate()
    {
        Vector3 cameraPos;

        cameraPos = transform.position;
        cameraPos.x = Mathf.Clamp(player.transform.position.x, minXClamp, maxXClamp);
        cameraPos.y = Mathf.Clamp(player.transform.position.y, minYClamp, maxYClamp);
        transform.position = cameraPos;

    }
}