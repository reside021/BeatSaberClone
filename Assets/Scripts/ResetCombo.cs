using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetCombo : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Cube"))
        {
            Blade.combo = 1;
        }
    }
}
