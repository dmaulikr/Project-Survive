﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StunnedState : AIState
{
    public StunnedState(Enemy character)
    {
        this.character = character;
    }

    public override void action()
    {
        if (!character.isStunned())
        {
            character.popState();
        }
    }
}
