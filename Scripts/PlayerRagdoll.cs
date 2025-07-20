using UnityEngine;
using System.Collections.Generic;

public class PlayerRagdoll : MonoBehaviour {
    public Animator anim;
    public List<Collider> colliderToEnable;
    public List<Rigidbody> ragdollRigidBodies;

    public Collider[] allCollider;

    void Start() {
        allCollider = GetComponentsInChildren<Collider>(true);
        foreach(var collider in allCollider) {
            if (collider.transform != transform) {
                var rag_rb = collider.GetComponent<Rigidbody>();
                if (rag_rb) {
                    ragdollRigidBodies.Add(rag_rb);
                }
            }
        }

        EnableRagdoll(GetComponent<PlayerManager>().isDead);
    }

    public void EnableRagdoll(bool enableRagdoll) {
        anim.enabled = !enableRagdoll;
        foreach(Collider item in allCollider) {
            item.enabled = enableRagdoll;
        }

        foreach(Rigidbody ragdollRigidBody in ragdollRigidBodies) {
            ragdollRigidBody.useGravity = enableRagdoll; 
            ragdollRigidBody.isKinematic = !enableRagdoll;
        }

        foreach(Collider item in colliderToEnable) {
            item.enabled = !enableRagdoll;
        }
    }
}