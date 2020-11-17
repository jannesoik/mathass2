using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    public GameObject fireLocation;
    public GameObject bulletPrefab;
    float rotationSpeed = 2;

    Vector3 position;
    Quaternion rotation;

    void Start()
    {
        position = transform.position;
        rotation = transform.rotation;
        
    }

    // Update is called once per frame
    void Update()
    {
        // rotate cannon using arrow keys
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        transform.Rotate(-vertical * rotationSpeed, horizontal * rotationSpeed, 0.0f);

        if (Input.GetKeyUp(KeyCode.Space))
            shoot();
    }

    void shoot()
    {
        // instantiate bullet at firelocation
        //GameObject bullet = Instantiate(bulletPrefab, fireLocation.transform.position, Quaternion.identity);
        bulletPrefab.gameObject.SetActive(true);

        // add force to bullet (only bullet has rigidbody)
        //bullet.GetComponent<Rigidbody>().AddForce(transform.forward * firePower, ForceMode.Impulse);
        //LevelManager.instance.shoot();
    }
}
