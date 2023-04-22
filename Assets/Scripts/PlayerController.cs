using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float playerSpeed;
    private Rigidbody2D rb;
    private Vector3 playerDirection;
    private GameObject Camera_main;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Awake()
    {
        Camera_main = GameObject.Find("Main Camera");
    }

    // Update is called once per frame
    void Update()
    {
        float directionX = Input.GetAxisRaw("Horizontal");
        float directionY = Input.GetAxisRaw("Vertical");

        playerDirection = new Vector3(directionX, directionY).normalized;
    }

    void FixedUpdate()
    {
        rb.MovePosition(transform.position + playerDirection * playerSpeed * Time.fixedDeltaTime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Zombie")
        {
            Destroy(this.gameObject);
            Camera_main.transform.position = new Vector3(0, 0, -10);
        }
    }
}
