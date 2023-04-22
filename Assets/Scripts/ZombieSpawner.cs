using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    public int waveNumber = 0;
    public int enemiesAmount = 0;
    public GameObject zombie;
    public Camera cam;
    // Use this for initialization
    void Start () {
        cam = Camera.main;

    }

    // Update is called once per frame
    void Update () {
        float height = 2f * cam.orthographicSize*2;
        float width = height * cam.aspect*2;
        for (int i = 0; i < waveNumber; i++) {
            for (int j = 0; j < enemiesAmount; j++) {
               
                Instantiate(zombie, new Vector3(cam.transform.position.x + Random.Range(-width, width),cam.transform.position.z+height+Random.Range(-width,width),cam.transform.position.y+height+Random.Range(10,30)),Quaternion.identity);
                enemiesAmount--;
                
            }
             
            // waveNumber--;
        }
    }


}
