using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack
{
    public Cyanobacteria prey;
    public Amoeba predator;
    public float attackStrength;
    public float evasiveness;

    public Attack(Cyanobacteria prey, Amoeba predator, float attackStrength, float evasiveness)
    {
        this.prey = prey;
        this.predator = predator;
        this.attackStrength = attackStrength;
        this.evasiveness = evasiveness;
    }

    public void Attempt()
    {
        float attackPower = Random.Range(0f, 2f) * attackStrength;
        if (attackPower > evasiveness)
        {
            SuccessfulAttack();
        } else
        {
            FailedAttack();
        }

    }

    private void SuccessfulAttack()
    {
        prey.Apoptosis(2);
        predator.ReceiveNutrients(Mathf.Pow((prey.reproductionCost / 30), 1/3) * 30);
        predator.attackCooldown = predator.attackCooldownLength;
    }

    private void FailedAttack()
    {
        predator.attackCooldown = predator.attackCooldownLength;
        predator.RemoveTarget(prey);
    }

}
