using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class Cyanobacteria : Bacterium
{
    [Header("Base Values")]
    [SerializeField] private float baseMovementSpeed = 1;
    [SerializeField] private float baseReproductionCost = 10f;
    [SerializeField] private float baseMutationChance = 0.05f;
    [SerializeField] private float baseEfficiency = 2f;
    [SerializeField] private float baseReproductiveFervor = 1f;
    [SerializeField] private int bioformGeneration = 0;
    [SerializeField] private float baseSize = 1;
    [SerializeField] private float baseLifespan = 60f;
    [SerializeField] private float baseEvasiveness = 1f;
    [SerializeField] private GameObject cyanobacteriaObject;

    [Header("Lifeform Values")]
    [SerializeField] [Min(10f)] public float reproductionCost;
    [SerializeField] [Min(1f)] private float nutritionIntakeEfficiency;
    [SerializeField] private float mutationChance;
    [SerializeField] private float metabolicCost;
    [SerializeField] [Min(0.33f)] private float reproductiveFervor;
    [SerializeField] [Range(0.33f, 3.33f)] private float size;
    [SerializeField] private float lifespan;
    [SerializeField] private float age;
    public float evasiveness;

    [Header("Simulation")]
    private Enviroment enviroment;
    [SerializeField] private NutrientNode node;
    [SerializeField] private float nodeSearchCooldown = 5;

    protected override void Start()//TODO ASAP: COLOUR LERP FOR FOOD 
    {//ISSUES: 
        base.Start();

        bioformGenome = Mutate(bioformGenome, mutationChance, 0.4f);

        targetPositions = GetNewTargetPositions();
        enviroment = FindObjectOfType<Enviroment>();

        EvaluateIndependentValues();
        EvaluateDependentValues();
        StartCoroutine(FindNutrientNode());

    }

    private void FixedUpdate()
    {
        Move();
        Metabolize();

        if (ShouldReproduce())
        {
            Reproduce();
        }

    }

    private void Reproduce()
    {
        if (nutritionStore > (1.5f * reproductionCost))
        {
            Cyanobacteria childCyanobacteria = Instantiate(cyanobacteriaObject).GetComponent<Cyanobacteria>();
            nutritionStore -= reproductionCost;

            Vector3[] newPositions = GetValidPositions();

            transform.position = newPositions[1];
            childCyanobacteria.transform.position = newPositions[0];

            childCyanobacteria.bioformGeneration = bioformGeneration++;
            childCyanobacteria.bioformGenome = CreateNewGenome(bioformGenome);
            childCyanobacteria.gameObject.name = "Cyanobacteria" + childCyanobacteria.bioformGeneration;
        }
    }

    private bool ShouldReproduce()
    {
        float reproductiveThreshold = reproductiveFervor * ReproductiveProspects(node.nodeNutrients);
        return reproductiveThreshold >= 1 ? true : false;
    }

    public static float ReproductiveProspects(float x, float a = 0.01f, float b = 0.5f)
    {
        return Mathf.Exp(a * x) - b;
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

        float nutrientsPhotosynthesized;

        if (nutritionStore < 100)
        {
            nutrientsPhotosynthesized = node.ConsumeNutrients(nutritionIntakeEfficiency * metabolicCost * Time.fixedDeltaTime);
        }
        else
        {
            nutrientsPhotosynthesized = 0f;
        }

        float nutrientBalance = nutrientsPhotosynthesized - (metabolicCost * Time.fixedDeltaTime);
        nutritionStore += nutrientBalance;

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
        return modifiedMetabolicCost;
    }

    private void EvaluateDependentValues()
    {
        reproductionCost = baseReproductionCost * size;
        transform.localScale = new Vector3(1, 1, 1) * size;
        nutritionStore = 1f;
        metabolicCost = GetMetabolicCost();
        evasiveness = baseEvasiveness * bioformGenome.genes[1].expressionStrength * bioformGenome.genes[0].expressionStrength / bioformGenome.genes[3].expressionStrength;
        VariableDictionary[0] = metabolicCost;
        VariableDictionary[2] = evasiveness;
    }

    private void EvaluateIndependentValues()
    {
        movementSpeed = baseMovementSpeed * bioformGenome.genes[0].expressionStrength;
        size = baseSize * bioformGenome.genes[1].expressionStrength;
        mutationChance = baseMutationChance * bioformGenome.genes[2].expressionStrength;
        nutritionIntakeEfficiency = baseEfficiency * bioformGenome.genes[3].expressionStrength;
        reproductiveFervor = baseReproductiveFervor * bioformGenome.genes[4].expressionStrength;
        lifespan = baseLifespan * bioformGenome.genes[5].expressionStrength;
    }

    public override void ChangeDirection() => targetPositions = GetNewTargetPositions();


    private IEnumerator FindNutrientNode()
    {
        while (true)
        {
            node = enviroment.FindClosestNode(this.transform.position);
            yield return new WaitForSeconds(nodeSearchCooldown);
        }
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

}
