using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class Amoeba : Bacterium
{
    [Header("Base Values")]
    [SerializeField] private float baseMovementSpeed = 1;
    [SerializeField] private float baseReproductionCost = 10f;
    [SerializeField] private float baseMutationChance = 0.05f;
    [SerializeField] private float baseSensoryRange = 5f;
    [SerializeField] private float baseReproductiveFervor = 1f;
    [SerializeField] private int bioformGeneration = 0;
    [SerializeField] private float baseSize = 1;
    [SerializeField] private float baseLifespan = 60f;
    [SerializeField] private float baseAttackStrength = 1f;
    [SerializeField] private float baseMetabolicCost = 0.1f;
    [SerializeField] private GameObject amoebaObject;

    [Header("Lifeform Values")]
    [SerializeField] [Min(10f)] private float reproductionCost;
    [SerializeField] private float sensoryRange;
    [SerializeField] private float mutationChance;
    [SerializeField] private float metabolicCost;
    [SerializeField] [Min(0.33f)] private float reproductiveFervor;
    [SerializeField] [Range(0.33f, 3.33f)] private float size;
    [SerializeField] private float lifespan;
    [SerializeField] private float attackStrength;

    [Header("Sensory")]
    [SerializeField] private Cyanobacteria target;
    [SerializeField] private bool hasTarget = false;
    [SerializeField] public float attackCooldownLength = 3f;
    [SerializeField] public float attackCooldown = 0f;

    protected override void Start() 
    {
        base.Start();
        nutritionStore = 10f;

        bioformGenome = Mutate(bioformGenome, mutationChance, 0.4f);

        AcquireTargetPositions();

        EvaluateIndependentValues();
        EvaluateDependentValues();

        EventManager.OnBioformDestroyed += RemoveTarget;

    }

    private void OnDestroy()
    {
        EventManager.OnBioformDestroyed -= RemoveTarget;
    }

    private void FixedUpdate()
    {
        Sense();
        Search();
        Metabolize();
        if (ShouldReproduce())
        {
            Reproduce();
        }

    }

    public void ReceiveNutrients(float nutrients)
    {
        nutritionStore += nutrients;
    }

    public void RemoveTarget(Bacterium bacteria)
    {
        if (target == bacteria)
        {
            target = null;
            hasTarget = false;
        }
    }

    private void Sense()
    {
        attackCooldown -= Time.fixedDeltaTime;

        if (attackCooldown > 0)
        {
            hasTarget = false;
            target = null;

            if (targetPositions == null)
            {
                AcquireTargetPositions();
            }

            return;
        }
        
        if (hasTarget)
        {
            if (Vector3.Distance(target.gameObject.transform.position, this.transform.position) > sensoryRange)
            {
                target = null;
                hasTarget = false;
            }
        }

        if (!hasTarget)
        {
            AcquireClosestCyanobacteriaWithinRange(sensoryRange);
        }

        if (target != null)
        {
            hasTarget = true;
            targetPositions = null;
        }

        if (target == null && targetPositions == null)
        {
            AcquireTargetPositions();
        }

    }

    private void AttackTarget(Cyanobacteria target)
    {
        if (attackCooldown <= 0 && target != null)
        {
            Attack attackAttempt = new Attack(target, this, attackStrength, target.evasiveness);
            attackAttempt.Attempt();
        }
    }

    private void AcquireTargetPositions() => targetPositions = GetNewTargetPositions();

    private void AcquireClosestCyanobacteriaWithinRange(float range)
    {
        if (SimulationManager.cyanobacterium.Count == 0)
        {
            return;
        }

        Bacterium[] cyanobacteria = SimulationManager.cyanobacterium.ToArray();

        Cyanobacteria closestCyanobacteria = null;
        float closestDistance = Mathf.Infinity;

        foreach (Cyanobacteria bacteria in cyanobacteria)
        {
            float distance = Vector3.Distance(bacteria.transform.position, this.transform.position);
            if (distance < closestDistance)
            {
                closestCyanobacteria = bacteria;
                closestDistance = distance;
            }
        }

        if (closestCyanobacteria != null && closestDistance < range)
        {
            target = closestCyanobacteria;
        }
    }

    private void Reproduce()
    {
        Amoeba childAmoeba = Instantiate(amoebaObject).GetComponent<Amoeba>();
        nutritionStore -= reproductionCost;

        Vector3[] newPositions = GetValidPositions();

        transform.position = newPositions[1];
        childAmoeba.transform.position = newPositions[0];

        childAmoeba.bioformGeneration = bioformGeneration++;
        childAmoeba.bioformGenome = CreateNewGenome(bioformGenome);
        childAmoeba.gameObject.name = "Amoeba" + childAmoeba.bioformGeneration;
    }

    private bool ShouldReproduce()
    {
        float reproductiveThreshold = (reproductionCost / reproductiveFervor) * 1.75f;
        return nutritionStore >= reproductiveThreshold;
    }

    private void Metabolize()
    {
        if (SimulationManager.lifespanEnabled)
        {
            lifespan -= Time.fixedDeltaTime;

            if (lifespan <= 0)
            {
                Apoptosis(1);
            }
        }

        float nutrientCost = metabolicCost * Time.fixedDeltaTime;
        nutritionStore -= nutrientCost;

        if (nutritionStore < 0)
        {
            TakeDamage(1 * Time.fixedDeltaTime);
        }
    }

    private float GetMetabolicCost()
    {
        float rawMetabolicCost = 1f;

        foreach (Gene gene in bioformGenome.genes)
        {
            if (gene.metabolicCostModifier > 0)
            {
                float metabolicCostIncrease = gene.expressionStrength - 1f;
                rawMetabolicCost += metabolicCostIncrease * gene.metabolicCostModifier;
            }
        }

        float modifiedMetabolicCost = Mathf.Exp(Mathf.Log(1.5f) * rawMetabolicCost) - 0.5f;
        return modifiedMetabolicCost * baseMetabolicCost;
    }

    private void EvaluateDependentValues()
    {
        reproductionCost = baseReproductionCost * size;
        transform.localScale = new Vector3(1, 1, 1) * size;
        metabolicCost = GetMetabolicCost();
        attackStrength = baseAttackStrength * bioformGenome.genes[1].expressionStrength * bioformGenome.genes[0].expressionStrength;
        VariableDictionary[0] = metabolicCost;
        VariableDictionary[1] = attackStrength;
    }

    private void EvaluateIndependentValues()
    {
        movementSpeed = baseMovementSpeed * bioformGenome.genes[0].expressionStrength;
        size = baseSize * bioformGenome.genes[1].expressionStrength;
        mutationChance = baseMutationChance * bioformGenome.genes[2].expressionStrength;
        sensoryRange = baseSensoryRange * bioformGenome.genes[3].expressionStrength;
        reproductiveFervor = baseReproductiveFervor * bioformGenome.genes[4].expressionStrength;
        lifespan = baseLifespan * bioformGenome.genes[5].expressionStrength;
    }

    public override void ChangeDirection() => AcquireTargetPositions();

    private void Search()
    {
        if (hasTarget)
        {
            MoveTowardsTarget();
        } else if (!hasTarget)
        {
            Move();
        }
    }

    private void MoveTowardsTarget()
    {
        Vector2 targetPosition = Vector3ToVector2(target.transform.position);
        Vector2 position = Vector3ToVector2(transform.position);

        Vector2 direction = (targetPosition - position).normalized;

        Vector2 positionChange = direction * movementSpeed * Time.fixedDeltaTime;
        Vector2 newPosition = position + positionChange;

        transform.position = Vector2ToVector3(newPosition);
    }

    private Vector3[] GetValidPositions()
    {
        Vector3[] newPositions = new Vector3[2];
        float attempts = 0;
        while (true)
        {
            Vector2 deltaPosition = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized * spawnDistance;
            Vector3 childNewPosition = new Vector3(transform.position.x + deltaPosition.x, transform.position.y + deltaPosition.y, transform.position.z);
            Vector3 parentNewPosition = new Vector3(transform.position.x - deltaPosition.x, transform.position.y - deltaPosition.y, transform.position.z);

            if (IsWithinBounds(childNewPosition) && IsWithinBounds(parentNewPosition))
            {
                newPositions[0] = childNewPosition;
                newPositions[1] = parentNewPosition;

                break;
            }

            attempts++;

            if (attempts > 10)
            {
                return new Vector3[2] { transform.position, transform.position };
            }
        }

        return newPositions;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Cyanobacteria>() == target)
        {
            AttackTarget(target);
        }
    }

    public void MutationCycle(float length)
    {
        for (int i = 0; i < length; i++)
        {
            bioformGenome = Mutate(bioformGenome, mutationChance, 1f);
        }
    }

}
