using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public GameObject player;
    float step = 0.02f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            print("Going up");
            player.transform.position += new Vector3(0, step, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            print("Going down");
            player.transform.position += new Vector3(0, -step, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            print("Going right");
            player.transform.position += new Vector3(step, 0, 0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            print("Going left");
            player.transform.position += new Vector3(-step, 0, 0);
        }
    }
}
