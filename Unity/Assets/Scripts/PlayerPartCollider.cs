using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handle the collision between the player body part and the pose wall body parts;
public class PlayerPartCollider : MonoBehaviour
{
    void OnTriggerEnter(Collider other) {
        if (gameObject.CompareTag(other.tag)) {
            GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<Player>().OnChildTriggerMatchedTag();
        }
    }
}
