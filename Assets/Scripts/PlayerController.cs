using System.Collections;
using System.Collections.Generic;
using Gtec.UnityInterface;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float playerSpeed;
    private Rigidbody2D rb;
    private Vector3 playerDirection;
    private GameObject Camera_main;

    public Animator anim;
    public int LastAnimState = 0;


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


        
        if(directionX > 0)
        {
            anim.Play("Right");
            LastAnimState = 1;
        }
        else if(directionX < 0)
        {
            anim.Play("Left");
            LastAnimState = -1;
        }
        else if(directionX == 0 && directionY == 0)
        {
            if (LastAnimState == -1)
                anim.Play("IdleLeft");
            else anim.Play("IdleRight");
        }
        if (directionY != 0)
        {
            if (LastAnimState == -1)
            {
                anim.Play("Left");
            }
            else anim.Play("Right");
        }

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
            anim.Play("Deth");
            SceneManager.LoadScene(0);
            
        }
    }
}
