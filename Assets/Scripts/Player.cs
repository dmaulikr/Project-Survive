﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : CMoveCombatable
{
    public static Player instance;
    public static string familyName = "";
    public static Vector3 spawmPos = new Vector3(-10,0,0);

    public new static HealthBar healthBar;

    //Experiance Variables
    public static int pointsOnLevelUp = 3;
    private int levelUpPoints = 0;
    private int xpPerLevelIncrease = 5;
    private int xpPerLevel = 5; //Reach this to level up
    private int xp = 0;

    //Equipment Variables
    private Equipment[] equipment = new Equipment[2];

    //Input Variables
    private bool chargingAttack = false;

    //Menu Variables
    private bool inMenu = false;
    private bool inBagMenu = false;
    private bool inLevelMenu = false;

    //Inventory Variables
    [HideInInspector]
    public Bag bag;
    public ItemsInRange itemsInRange;
    private int gold = 900;

    public new void Start()
    {
        healthBar.resetFill();
        healthBar.setActive(true);
        base.healthBar = healthBar;

        base.Start();

        //Set up family Name
        if (familyName == "")
            familyName = lastName;
        else
            lastName = familyName;

        bag = new Bag(UIManager.instance.bagGUIObject.GetComponent<BagGUI>());

        itemsInRange = new ItemsInRange(this);

        traits[0] = Trait.getTrait();
        traits[0].applyTrait(this);

        //Singleton
        if (instance == null)
            instance = this;

        animator = GetComponentInChildren<Animator>();

        //Set up sprites to include weapon
        weapon.SetActive(true);
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        weapon.SetActive(false);

        UIManager.instance.setAbilities(characterClass.abilities); //REmove when player is generated
    }

    public void setSingleton()
    {
        //Singleton
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Destroy(instance.gameObject);
            instance = this;
        }
    }

    public bool isInMenu()
    {
        return inMenu;
    }

    public void setInMenu(bool inMenu)
    {
        this.inMenu = inMenu;
    }

    public void closeBagMenu()
    {
        inBagMenu = false;
        Invoke("closeMenu", 0.1f);
    }

    public void closeLevelupMenu()
    {
        inLevelMenu = false;
        Invoke("closeMenu", 0.1f);
    }

    public void closeMenu()
    {
        inMenu = false;
    }

    public int getGoldAmount()
    {
        return gold;
    }

    public void addGold(int goldAmount)
    {
        gold += goldAmount;
    }

    public void removeGold(int goldAmount)
    {
        gold -= goldAmount;
    }

    public void levelup(int pointsLeft, int[] stats)
    {
        levelUpPoints = pointsLeft;

        strength += stats[0];
        agility += stats[1];
        endurance += stats[2];

        //Update health
        currentHealth += stats[2] * endMod;
        maxHealth += stats[2] * endMod;
    }

    internal int getLevelUpPoints()
    {
        return levelUpPoints;
    }

    public void useItem(int i)
    {
        bag.useItem(i);
    }

    public override void attackHit()
    {
        CameraFollow.cam.GetComponentInParent<CameraShake>().shake = .5f;
    }

    public override Vector2 movement()
    {
        // read key inputs
        bool wKeyDown = Input.GetKey(KeyCode.W);
        bool aKeyDown = Input.GetKey(KeyCode.A);
        bool sKeyDown = Input.GetKey(KeyCode.S);
        bool dKeyDown = Input.GetKey(KeyCode.D);
        bool shiftKeyDown = Input.GetKey(KeyCode.LeftShift);

        //Movement Vector
        Vector3 movement = Vector3.zero;
        float movementSpeed = walkSpeed;

        if (shiftKeyDown && !weaponDrawn)
            movementSpeed = sprintSpeed;

        if (wKeyDown && !sKeyDown && !jumping)
        {
            movement.y += movementSpeed;
        }
        if (sKeyDown && !wKeyDown && !jumping)
        {
            movement.y -= movementSpeed;
        }
        if (aKeyDown && !dKeyDown)
        {
            movement.x -= movementSpeed;
        }
        if (dKeyDown && !aKeyDown)
        {
            movement.x += movementSpeed;
        }

        

        if (movement == Vector3.zero)
        {
            moving = false;
            animator.SetFloat("movementSpeed", 0);
            return movement;
        }
        else
            moving = true;

        animator.SetFloat("movementSpeed", movementSpeed);

        //Flip player sprite if not looking the right way
        if (movement.x < 0 && transform.localScale.x != -1)
            faceLeft();
        else if (movement.x > 0 && transform.localScale.x != 1)
            faceRight();

        return movement * movementSpeed;// * Time.deltaTime;
    }

    public override bool attack(Ability action)
    {
        //Reset charge attack time, so players can't hold it while using an ability and come straight out for a follow up heavy attack
        startedHolding = float.MaxValue;
        animator.SetBool("charged", false);

        //Get mouse position in relation to the world
        /*Vector2 mousePos = CameraFollow.cam.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction = getDirection(mousePos, 0);*/

        return base.attack(action);
    }

    protected override void death()
    {
        base.death();

        WorldManager.instance.playerDied(this);
    }

     protected override void removeDeadBody()
    {
        return;
    }

    IEnumerator inputHandler()
    {
        //Bag Input
        bool bKeyDown = Input.GetKeyDown(KeyCode.B);
        bool cKeyDown = Input.GetKeyDown(KeyCode.C);

        if (bKeyDown && !attacking && !chargingAttack && (!inMenu || inBagMenu))
        {
            inMenu = !inMenu;
            inBagMenu = !inBagMenu;
            bag.input();
        }

        if (cKeyDown && !attacking && !chargingAttack && (!inMenu || inLevelMenu))
        {
            inMenu = !inMenu;
            inLevelMenu = !inLevelMenu;
            UIManager.instance.toggleLevelUpWindow();
        }

        //Only take menu inputs if in menu
        if (inMenu)
            yield break;

        //Weapon Inputs
        //Attack
        bool leftClickDown = Input.GetMouseButtonDown(0);
        bool leftClickHeld = Input.GetMouseButton(0);
        bool leftClickUp = Input.GetMouseButtonUp(0);
        //Ability #1
        bool rightClick = Input.GetMouseButtonDown(1);
        //Ability #2
        bool spaceKeyDown = Input.GetKeyDown(KeyCode.Space);
        //Draw Weapon
        bool qKeyDown = Input.GetKeyDown(KeyCode.Q);
        //Pickup Item
        bool eKeyDown = Input.GetKeyDown(KeyCode.E);

        if (qKeyDown && !attacking && !chargingAttack)
            drawWeapon();

        if (eKeyDown && !attacking)
            itemsInRange.pickupItem();

        if (Input.GetKeyDown(KeyCode.Y))
            UIManager.instance.newShopWindow();

        //Call movement function to handle movements
        Vector3 movementVector = Vector3.zero;

        if (!attacking)
            movementVector = movement();

        yield return new WaitForFixedUpdate(); //For rigidbody interactions

        //If left click with weapon out and not already attacking, then start charging
        if (((leftClickHeld && !chargingAttack && !stunned) || leftClickDown) && !attacking && weaponDrawn)
        {
            startedHolding = Time.time;
            chargingAttack = true;
        }
        //If left click with no weapon out and not attacking or charging, then draw weapon
        else if (leftClickUp && !attacking && !chargingAttack && !weaponDrawn)
        {
            drawWeapon();
        }
        //If releasing left click after charging up and not already attacking then execute either a heavy or light attack based on charge time
        else if (leftClickUp && !attacking && weaponDrawn && chargingAttack)
        {
            if (startedHolding + chargeTime < Time.time)
                attack(characterClass.heavyAttack); 
            else
                attack(characterClass.basicAttack);

            chargingAttack = false;
        }
        //If you are attacking and can combo then attack with basic attack
        else if (leftClickUp && attacking && weaponDrawn)
        {
            canCombo = true;
            chargingAttack = false;
        }


        else if (rightClick && !attacking && weaponDrawn)
        {
            if (attack(characterClass.abilities[0]))
                UIManager.instance.usedAbility(0);
        }
        else if (spaceKeyDown && !attacking)
        {
            if (attack(characterClass.abilities[1]))
                UIManager.instance.usedAbility(1);
        }

        rb2D.AddForce(movementVector);
    }

    //Set Active
    public override void loseHealth(int damage)
    {
        base.loseHealth(damage);
        StopCoroutine("showHealth");

        if (!dead)
            CameraFollow.cam.GetComponentInParent<CameraShake>().shake = .5f;
    }

    public void addXp(int xpGained)
    {
        xp += xpGained;

        if(xp >= xpPerLevel)
        {
            levelUpPoints += pointsOnLevelUp;
            xp -= xpPerLevel;
            level++;
            xpPerLevel += xpPerLevelIncrease;
        }

        //Update UI xp bar
        UIManager.instance.addXp(xp, xpPerLevel);
    }

    public override void applyStun(float stunTime)
    {
        base.applyStun(stunTime);

        startedHolding = Mathf.Infinity;
        animator.SetBool("charged", false);
        chargingAttack = false;
    }

    public int[] getStats()
    {
        int[] stats = { strength, agility, endurance };
        return stats;
    }
    // Update is called once per frame
    new void Update()
    {
        if (!WorldManager.isPaused)
        {
            base.Update();

            if (!dead && !stunned)
                StartCoroutine("inputHandler"); //Alternte than coroutine??

            if (startedHolding + chargeTime < Time.time)
                animator.SetBool("charged", true);

            if (inMenu)
                animator.SetFloat("movementSpeed", 0);
        }
    }
}