using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool isGrounded = false;
    void OnTriggerEnter(Collider other) {
        //if (other.gameObject.CompareTag("Ground")) {
            isGrounded = true;
        //}
    }
    void OnTriggerExit(Collider other) {
        //if (other.gameObject.CompareTag("Ground")) {
            isGrounded = false;
        //}
    }
}