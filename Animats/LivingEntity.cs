﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Base class taken from youtube tutorial url:
/// for ai testing 
/// </summary>

public class LivingEntity : MonoBehaviour, IDamageable {

    protected float startingHealth = 100;
    protected float health;
    protected bool dead;
    public Transform essenceBlock;

    public event System.Action OnDeath;

    protected virtual void Start() {
        health = startingHealth;
    }

    public void TakeHit(float damage, RaycastHit hit) {
        health -= damage;

        if (health <= 0 && !dead)
        {

            Die();
        }
    }



    protected virtual void Die(){
        dead = true;
        Transform Essenceholder = Instantiate(essenceBlock, transform.position, Random.rotation) as Transform;
        Essenceholder.GetComponent<AnimatEssence>().setReaourceAdundance((int)(startingHealth / 4), (int)(startingHealth - (startingHealth / 4)));
        if (OnDeath != null) {
            OnDeath();
        }
        //Debug.Log(transform.gameObject.ToString() + " Has Died");
        GameObject.Destroy(gameObject);
    }
}
