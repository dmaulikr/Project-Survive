﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MoveState : AIState
{
    //Hold a movement field to determine how to move?

    public MoveState(Enemy character)
    {
        this.character = character;
    }


    public override void action()
    {
        base.action();

        //Transition to Idle State if target gets too far away or they don't exist anymore
        if (character.target == null || (character.target.position - character.transform.position).magnitude > character.aggroRange)
        {
            character.target = null;
            character.animator.SetFloat("movementSpeed", 0f);
            character.popState();
            character.pushState(new IdleState(character));
            return;
        }

        
        //Transition to Attack State or converse state if target gets within range
        if ((character.getTargetPositon() - character.transform.position).magnitude < 0.2f)
        {
            if (FactionManager.instance.isHostile(character.faction, character.target.GetComponent<CMoveCombatable>().faction))
            {
                character.animator.SetFloat("movementSpeed", 0f);
                character.pushState(new AttackState(character));
                
            }
            else
            {
                character.animator.SetFloat("movementSpeed", 0f);
                character.popState();
                character.pushState(new ConverseState(character));
            }
            return;
        }

        //Leave till last so don't move if switching states
        character.rb2D.AddForce(character.movement());
    }

    private Vector2 movement()
    {

        float movementSpeed = character.walkSpeed;

        if (character.target.transform.position.x < character.transform.position.x && character.transform.localScale.x != -1)
            character.faceLeft();
        else if (character.target.transform.position.x > character.transform.position.x && character.transform.localScale.x != 1)
            character.faceRight();

        Vector3 dir = character.getDirection(character.getTargetPositon(), character.target.gameObject.GetComponent<CHitable>().objectHeight);

        character.animator.SetFloat("movementSpeed", 2.5f);
        
        return (dir * movementSpeed);

    }
}