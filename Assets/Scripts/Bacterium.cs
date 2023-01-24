using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class Bacterium : MonoBehaviour
{
    [SerializeField] public Genome bioformGenome = new();
    [SerializeField] public int id;

    [Header("Vitals")]
    [SerializeField] protected float nutritionStore = 0f;
    [SerializeField] protected float health = 5;
    [SerializeField] protected float maxHealth = 5;
    [SerializeField] protected float movementSpeed;

    [Header("Movement")]
    [SerializeField] protected float distanceLeap = 5f;
    [SerializeField] protected Vector2[] targetPositions;
    [SerializeField] protected float timeToTarget;
    [SerializeField] protected float timeTravelledToTarget;
    [SerializeField] protected float spawnDistance = 0.5f;

    protected static Dictionary<int, string> DeathCauseDictionary = new Dictionary<int, string>();
    public Dictionary<int, float> VariableDictionary = new();

    public static Vector2 Vector3ToVector2(Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.y);
    }

    public static Vector2 Vector2ToVector3(Vector2 vector2)
    {
        return new Vector3(vector2.x, vector2.y, 0);
    }

    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        return a + (b - a) * t;
    }

    public static Vector2 QuadraticCurve(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 p0 = Lerp(a, b, t);
        Vector2 p1 = Lerp(b, c, t);
        return Lerp(p0, p1, t);
    }

    protected void TakeDamage(float value)
    {
        health -= value;
        if (health < 0)
        {
            Apoptosis(0);
        }
    }

    protected void RegenerateHealth(float value = 0.5f)
    {
        if (nutritionStore > 0)
        {
            health += (maxHealth * value * Time.deltaTime);
        }
    }

    public void Apoptosis(int deathCause)
    {
        Debug.Log(this.gameObject.name + DeathCauseDictionary[deathCause]);


        EventManager.BioformDestroyed(this);
        EventManager.OnSimulationEnd -= Apoptosis;

        Destroy(this.gameObject);
    }

    protected void Apoptosis()
    {
        Apoptosis(3);
    }

    public virtual void ChangeDirection()
    {

    }

    protected virtual void Start()
    {
        EventManager.BioformCreated(this);
        EventManager.OnSimulationEnd += Apoptosis;

        DeathCauseDictionary[0] = " has died of starvation.";
        DeathCauseDictionary[1] = " has died of old age.";
        DeathCauseDictionary[2] = " has been killed.";
        DeathCauseDictionary[3] = " has been smitten by God.";

    }

    public static float GetMutationPower(float mutationChance, float sigma = 0.4f)
    {
        float x = Random.Range(-1f, 1f);
        float sign = Random.Range(0, 2) * 2 - 1;
        return mutationChance * (1f - SampleBellCurve(sigma, 0f, x)) * sign;
    }

    public static float SampleBellCurve(float sigma, float mu, float x)
    {
        float y = (1 / (sigma * Mathf.Sqrt(2 * Mathf.PI))) * Mathf.Exp(-0.5f * Mathf.Pow((x - mu) / sigma, 2));
        return y;
    }

    public static bool IsWithinBounds(Vector3 position)
    {
        bool verification = false;

        if (position.x < SimulationManager.SimulationCoordinateBounds.x &&
            position.x > SimulationManager.SimulationCoordinateBounds.y &&
            position.y > SimulationManager.SimulationCoordinateBounds.z &&
            position.y < SimulationManager.SimulationCoordinateBounds.w)
        {
            verification = true;
        }

        return verification;
    }

    protected Vector2[] GetNewTargetPositions()
    {
        Vector2[] vectors = GetNewVectors();
        int i = 0;
        while ((Vector2.Dot(vectors[0].normalized, vectors[2].normalized) <= 0.70f) || (Vector2.Dot(vectors[0].normalized, vectors[1].normalized) <= 0.33f))
        {
            if (i > 10) { break; }
            vectors = GetNewVectors();
            i++;
        }

        Vector2 midPosition = vectors[0];
        Vector2 endPosition = vectors[1];
        Vector2 oldPosition = vectors[2];

        float distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.y), endPosition);
        timeToTarget = distance / movementSpeed;
        timeTravelledToTarget = 0;

        return new Vector2[] { midPosition, endPosition, oldPosition };
    }

    protected Vector2[] GetNewVectors()
    {
        Vector2 midPosition;
        Vector2 endPosition;
        Vector2 oldPosition;

        midPosition = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)) * distanceLeap;
        endPosition = midPosition + (new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)) * distanceLeap);
        oldPosition = new Vector2(transform.position.x, transform.position.y);

        return new Vector2[3] { midPosition, endPosition, oldPosition };
    }

    protected void Move()
    {
        Vector2 midPosition = targetPositions[0];
        Vector2 targetPosition = targetPositions[1];
        Vector2 oldPosition = targetPositions[2];

        timeTravelledToTarget += Time.fixedDeltaTime;
        float x = timeTravelledToTarget / timeToTarget;

        if (timeTravelledToTarget > timeToTarget)
        {
            targetPositions = GetNewTargetPositions();
        }
        else
        {
            ApplyMovement(oldPosition, midPosition, targetPosition, x);
        }
    }

    protected void ApplyMovement(Vector2 currentPosition, Vector2 midPosition, Vector2 endPosition, float x)
    {
        Vector2 newPosition = QuadraticCurve(currentPosition, midPosition, endPosition, x);
        transform.position = newPosition;
    }

    protected Genome CreateNewGenome(Genome oldGenome)
    {
        Gene[] newGenes = new Gene[6];
        Gene[] oldGenes = oldGenome.genes;
        for (int i = 0; i < newGenes.Length; i++)
        {
            newGenes[i] = new Gene(oldGenes[i].geneName, oldGenes[i].id, oldGenes[i].expressionStrength, oldGenes[i].metabolicCostModifier);
        }

        Genome newGenome = new Genome(newGenes);
        return newGenome;
    }

    protected Genome Mutate(Genome genome, float mutationChance, float sigma)
    {
        for (int i = 0; i < genome.genes.Length; i++)
        {
            genome.genes[i].expressionStrength *= (1 + (GetMutationPower(mutationChance, sigma) * 0.1f));
        }

        genome.genes[1].expressionStrength = Mathf.Max(genome.genes[1].expressionStrength, 0.33f);
        genome.genes[3].expressionStrength = Mathf.Max(genome.genes[3].expressionStrength, 1f);
        genome.genes[5].expressionStrength = Mathf.Min(genome.genes[5].expressionStrength, 2f);

        return genome;
    }
}
