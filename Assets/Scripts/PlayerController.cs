using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float playerSpeed;
    private Rigidbody2D rb;
    private Vector3 playerDirection;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
}
