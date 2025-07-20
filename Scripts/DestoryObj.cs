using UnityEngine;

public class DestoryObj : MonoBehaviour {
    [SerializeField] private float time;

    System.Collections.IEnumerator Start() {
        yield return new WaitForSeconds(time);

        Destroy(this.gameObject);
    }
}
