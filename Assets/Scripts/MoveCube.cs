using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MoveCube : MonoBehaviour
{
    public float Speed = 5f;


    [SerializeField] private Transform transform;



    // Start is called before the first frame update
    void Start()
    {

    }

    void FixedUpdate()
    {
        MovementLogic();
    }



    private void MovementLogic()
    {


        Vector3 movement = new Vector3(0f, 0.0f, -1f);


        transform.Translate(movement * Speed * Time.fixedDeltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
