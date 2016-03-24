using UnityEngine;
using System.Collections;

public class DNARotation : MonoBehaviour {

    Transform from;
    Transform to;
    [Range(0.01f, 0.1f)]
    public float rotationSpeed;
    bool IsRotating = true;

	void Start () {
        StartCoroutine(HelixRotation());
	}

    IEnumerator HelixRotation() {
        float percent;
        while (IsRotating) {
            percent = 0;
            from = transform;
                to = transform;
                to.localRotation = Quaternion.Euler(Random.Range(1, 360), to.rotation.y, to.rotation.z);
            while (percent < 1)
            {
                percent += Time.deltaTime * rotationSpeed;
                transform.rotation = Quaternion.Slerp(from.rotation, to.rotation, percent);
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}
