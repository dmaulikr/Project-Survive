﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    //Components
    private Rigidbody2D rb2D;
    new private SpriteRenderer renderer;

    //Information Variables
    private CMoveCombatable caster;
    public int damage;
    private float stunTime;
    private bool hitTarget;

    //Lifetime Variables
    private float lifespan = 1.1f;
    private float timeShot = 0f;

    //Speed Variables
    private float velocity = .25f;
    private Vector3 dir;

	// Use this for initialization
	public void Setup(CMoveCombatable caster, int damage, float stunTime, Vector3 dir)
    {
        rb2D = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        hitTarget = false;

        this.caster = caster;
        this.damage = damage;
        this.stunTime = stunTime;
        this.dir = dir;

        timeShot = Time.time;

        Vector3 spawnPos = new Vector3(caster.transform.position.x, caster.transform.position.y + caster.objectHeight / 2, caster.transform.position.z);
        transform.position = spawnPos + (dir * 0.5f) ;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {

        //If an object has been hit first, destroy the bullet
        if (collider.transform.gameObject.tag == "Object")
        {
            if (!collider.isTrigger)
                this.gameObject.SetActive(false);
            //Add destoryed particle effect here
        }

        CHitable targetHit = collider.GetComponentInParent<CHitable>();

        //If object hit is hitable, and this bullet hasn't hit anything else this life
        if (targetHit != null && !hitTarget)
        {

            if (targetHit.isInvuln() || targetHit.isKnockedback())
                return;

            hitTarget = true;

            //Apply damage and knockback
            targetHit.setAttacker(caster);
            //objectHit.knockback(pos, abilityKnockback, objectHit.objectHeight); //Need to use original pos for knockback so the position of where you attacked from is the knockback
            targetHit.loseHealth(damage);

            //Apply stun to the target
            targetHit.applyStun(stunTime);

            //TODO: Play audio sound
            caster.attackHit();

            this.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if(Time.time - lifespan > timeShot)
            this.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        rb2D.MovePosition(transform.position + (dir * velocity));
    }
}
