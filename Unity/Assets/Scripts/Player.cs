using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

// Handle player & wall collision checking
public class Player : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField]
    private int collisionCount;
    [SerializeField]
    private bool isHurt;
    
    void Start(){
        rb = gameObject.GetComponent<Rigidbody>();
    }

    // Call by the player body part colliders
    public void OnChildTriggerMatchedTag() {
        collisionCount++;
    }
    
    // Cleanup function after finish collision trigger
    void OnTriggerExit(Collider collision) {
        if (collisionCount > 0 && collisionCount < 13)
            isHurt = true;
    }

    public bool MatchAllCollider() {
        if (collisionCount == 13) {
            isHurt = false;
            return true;
        }
        return false;
    }

    public bool IsHurt {
        get { return isHurt; }
    }

    // Cleanup function for each round
    public void NewRound() {
        collisionCount = 0;
        isHurt = false;
    }
}
