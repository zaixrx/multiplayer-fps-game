using UnityEngine;
using System.Collections;

public class OthersController : MonoBehaviour {
    public Animator animator;
    public Transform playerCamera;
    public Transform playerOrientaion;

    public float animatiorTransititonTime = .2f;

    float velocityX = 0;
    float velocityZ = 0;

    int oldTick;
    int currentTick;
    int Δtick;
    int interpolationTime;
    Vector3 oldPosition;
    Vector3 currentPosition;

    public void OnServerMovementState(StatePayload _statePayload) {
        oldPosition = currentPosition;
        oldTick = currentTick;

        currentPosition = _statePayload.position;
        currentTick = _statePayload.tick;

        Δtick = currentTick - oldTick;
        interpolationTime = (_statePayload.tick - oldTick) / Δtick;

        StartCoroutine(Move(interpolationTime));

        playerCamera.localEulerAngles = _statePayload.cameraRotation;
        playerOrientaion.localEulerAngles = new Vector3(0, playerCamera.localEulerAngles.y, 0);

        AnimateCharacter(_statePayload);
    }

    private void AnimateCharacter(StatePayload _statePayload) {
        velocityX = (_statePayload.input.x == 0) ? 0 : /****/
                    (_statePayload.input.x > 0) ? .5f : -.5f;

        velocityZ = (_statePayload.input.z == 0) ? 0 : /****/
                    (_statePayload.input.z > 0) ? .5f : -.5f;

        animator.SetFloat("velocityX", velocityX, animatiorTransititonTime, Time.deltaTime * 10f);        
        animator.SetFloat("velocityZ", velocityZ, animatiorTransititonTime, Time.deltaTime * 10f);        

        animator.SetBool("fall", !_statePayload.isGrounded);
    }

    IEnumerator Move(float timeToLerp) {
        float lerpTime = 0;

        while (lerpTime < timeToLerp) {
            lerpTime += Time.deltaTime / (1 / Constants.ServerTickRate) * Δtick;
            transform.position = Vector3.Slerp(oldPosition, currentPosition, lerpTime);

            yield return null;
        }
    }
}
