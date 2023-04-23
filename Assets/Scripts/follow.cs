using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class follow : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float speed = 1.5f;
    private int up;
    void OnCollisionEnter2D(Collision2D collision) 
    {
        up = Random.Range(0, 3);
    }
    void OnCollisionStay2D(Collision2D collision)
    {
   
        if (collision.gameObject.tag != "Zombie")
        {
            Debug.Log(up);
            if (up == 1) transform.position = new Vector3(transform.position.x, transform.position.y + 0.1f, 0);
            else if (up == 2) transform.position = new Vector3(transform.position.x - 0.1f, transform.position.y, 0);
            else if (up == 3) transform.position = new Vector3(transform.position.x + 0.1f, transform.position.y, 0);
            else transform.position = new Vector3(transform.position.x, transform.position.y - 0.1f, 0);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
        //transform.up = player.transform.position - transform.position;
        //freeze = true;
        //transform.Rotate(0, 0, 0);
    }
}
