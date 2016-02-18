using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

    public LayerMask collisionMask;
    float speed = 10;
    float damage = 20;

    float lifeTime = 2;
    float skinWidth;

    void Start() {
        Destroy(gameObject, lifeTime);

        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask);
        if (initialCollisions.Length > 0){
            OnHitObject(initialCollisions[0]);
        }
    }

	public void SetSpeed(float newSpeed) {
        speed = newSpeed;
	}
	
	void Update () {
        float moveDistance = Time.deltaTime * speed;
        CheckCollisions(moveDistance);
        transform.Translate(Vector3.forward * moveDistance);
	}

    RaycastHit CheckCollisions(float moveDistance) {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide)) {
            return hit;
        }
        return hit;

    }

    void ObjectHit(RaycastHit hit) {
        IDamageable damageableObject = hit.collider.GetComponent<IDamageable>();
        if (damageableObject != null) {
            damageableObject.TakeHit(damage, hit);
        }
        //print(hit.collider.gameObject.name);
        GameObject.Destroy(gameObject);
    }

    void OnHitObject(Collider c){
        IDamageable damageableObject = c.GetComponent<IDamageable>();
        if (damageableObject != null)
        {
            damageableObject.TakeHit(damage, CheckCollisions(0.5f));
        }
        //print(hit.collider.gameObject.name);
        GameObject.Destroy(gameObject);
    }

}
