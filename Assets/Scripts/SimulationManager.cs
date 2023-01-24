using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

public class SimulationManager : MonoBehaviour
{
    public static bool SimulationIsRunning { private set; get; } = false;

    [Header("Spawning")]
    [SerializeField] private GameObject precursor;
    [SerializeField] private GameObject amoebaObject;
    [SerializeField] private Vector3 startLocation = Vector3.zero;
    public static readonly Vector4 SimulationCoordinateBounds = new(100, -100, -100, 100);
    [SerializeField] private int amoebaMutationCycleLength;

    [Header("Time Scale")]
    [SerializeField] private Slider timeScaleSlider;
    [SerializeField] private TMP_Text timeScaleText;

    [Header("Data")]
    [SerializeField] private TMP_Text bioformCountText;
    [SerializeField] private TMP_Text cyanobacteriaCountText;
    [SerializeField] private TMP_Text amoebaCountText;
    [SerializeField] private TMP_Text iterationText;
    [SerializeField] private Button introduceCyanobacteria;
    [SerializeField] private Button introduceAmoeba;
    [SerializeField] private Toggle autoAmoeba;

    public static List<Bacterium> bioforms = new();
    public static List<Bacterium> cyanobacterium = new();
    public static List<Bacterium> amoebae = new();

    private static Dictionary<int, List<Bacterium>> bioformDictionary = new();

    //Simulation Rules
    public static Dictionary<int, bool> simulationRules = new();

    public static bool lifespanEnabled = true;

    private void Start()
    {
        EventManager.OnBioformCreated += AddBioform;
        EventManager.OnBioformDestroyed += RemoveBioform;

        bioformDictionary[0] = cyanobacterium;
        bioformDictionary[1] = amoebae;

        simulationRules[0] = lifespanEnabled;
    }

    private void Update()
    {
        Time.timeScale = timeScaleSlider.value;
        timeScaleText.text = "Time Scale: " + Mathf.FloorToInt(Time.timeScale);

        bioformCountText.text = "Bacterium Count: " + bioforms.Count;
        cyanobacteriaCountText.text = "Cyanobacteria Count: " + cyanobacterium.Count;
        amoebaCountText.text = "Amoeba Count: " + amoebae.Count;

        introduceCyanobacteria.interactable = cyanobacterium.Count <= 0 && SimulationIsRunning;
        introduceAmoeba.interactable = amoebae.Count <= 0 && SimulationIsRunning;

        iterationText.text = "Collection Iteration: " + DataManager.collectionIteration.ToString();

        if (amoebae.Count == 0 && autoAmoeba.isOn == true)
        {
            IntroduceAmoeba();
        }

    }

    public void ToggleSimulationRule(int id) => simulationRules[id] = !simulationRules[id];

    public void PauseSimulation() => timeScaleSlider.value = Mathf.Epsilon;

    public void IntroduceCyanobacteria() => Instantiate(precursor, startLocation, Quaternion.identity);

    public void IntroduceAmoeba()
    {
        float length = cyanobacterium.Count;
        List<Bacterium> sentencedToDie = new List<Bacterium>();

        for (int i = 0; i < length; i++)
        {
            float value = Random.Range(0f, 1f);
            if (value <= 0.1f)
            {
                Amoeba newAmoeba = Instantiate(amoebaObject, cyanobacterium[i].transform.position, Quaternion.identity).GetComponent<Amoeba>();
                newAmoeba.MutationCycle(amoebaMutationCycleLength);
                sentencedToDie.Add(cyanobacterium[i]);
            }
        }

        for (int i = 0; i < sentencedToDie.Count; i++)
        {
            sentencedToDie[i].Apoptosis(3);
        }
    }

    private void AddBioform(Bacterium bacterium)
    {
        bioforms.Add(bacterium);
        bioformDictionary[bacterium.id].Add(bacterium);
    }

    private void RemoveBioform(Bacterium bacterium)
    {
        bioforms.Remove(bacterium);
        bioformDictionary[bacterium.id].Remove(bacterium);
    }

    public void BeginSimulation()
    {
        if (SimulationIsRunning)
        {
            return;
        }

        IntroduceCyanobacteria();

        SimulationIsRunning = true;
        EventManager.SimulationBegin();

        Debug.Log("Simulation has begun.");
    }

    public void EndSimulation()
    {
        if (!SimulationIsRunning)
        {
            return;
        }

        Bacterium[] bacteria = FindObjectsOfType<Bacterium>();
        for (int i = 0; i < bacteria.Length; i++)
        {
            Destroy(bacteria[i].gameObject);
        }

        EventManager.SimulationEnd();
        SimulationIsRunning = false;
    }

    public static SimulationData AcquireSimulationData(int iteration)
    {
        SimulationData simulationData = SimulationData.empty;

        simulationData.iteration = iteration;
        simulationData.bacteriumCount = bioforms.Count;
        simulationData.cyanobacteriumCount = cyanobacterium.Count;
        simulationData.cyanobacteriaMovementSpeed = AggregateGeneData(0, 0);
        simulationData.cyanobacteriaSize = AggregateGeneData(1, 0);
        simulationData.cyanobacteriaMutationChance = AggregateGeneData(2, 0);
        simulationData.cyanobacteriaMetabolicCost = AggregateIndependentVariable(0, 0);
        simulationData.cyanbacteriaNutritionEfficiency = AggregateGeneData(3, 0);
        simulationData.cyanobacteriaReproductiveFervor = AggregateGeneData(4, 0);
        simulationData.cyanobacteriaLifespan = AggregateGeneData(5, 0);
        simulationData.cyanobacteriaEvasiveness = AggregateIndependentVariable(2, 0);
        simulationData.amoebaCount = amoebae.Count;
        simulationData.amoebaMovementSpeed = AggregateGeneData(0, 1);
        simulationData.amoebaSize = AggregateGeneData(1, 1);
        simulationData.amoebaMutationChance = AggregateGeneData(2, 1);
        simulationData.amoebaMetabolicCost = AggregateIndependentVariable(0, 1);
        simulationData.amoebaAttackStrength = AggregateIndependentVariable(1, 1);
        simulationData.amoebaSensoryFocus = AggregateGeneData(3, 1);
        simulationData.amoebaReproductiveFervor = AggregateGeneData(4, 1);
        simulationData.amoebaLifespan = AggregateGeneData(5, 1);

        return simulationData;
    }

    public static float AggregateGeneData(int geneID, int bioformID)
    {
        Bacterium[] bioformsToExamine = bioformDictionary[bioformID].ToArray();
        float value = 0;
        float count = 0;
        for (int i = 0; i < bioformsToExamine.Length; i++)
        {
            value += bioformsToExamine[i].bioformGenome.genes[geneID].expressionStrength;
            count++;
        }

        return value / count;
    }

    public static float AggregateIndependentVariable(int variableID, int bioformID)
    {
        Bacterium[] bioformsToExamine = bioformDictionary[bioformID].ToArray();
        float value = 0;
        float count = 0;

        for (int i = 0; i < bioformsToExamine.Length; i++)
        {
            value += bioformsToExamine[i].VariableDictionary[variableID];
            count++;
        }

        return value / count;
    }
}

public class SimulationData
{
    public int iteration;
    public int bacteriumCount;
    public int cyanobacteriumCount;
    public float cyanobacteriaMovementSpeed;
    public float cyanobacteriaSize;
    public float cyanobacteriaMutationChance;
    public float cyanobacteriaMetabolicCost;
    public float cyanbacteriaNutritionEfficiency;
    public float cyanobacteriaReproductiveFervor;
    public float cyanobacteriaLifespan;
    public float cyanobacteriaEvasiveness;
    public int amoebaCount;
    public float amoebaMovementSpeed;
    public float amoebaSize;
    public float amoebaMutationChance;
    public float amoebaSensoryFocus;
    public float amoebaMetabolicCost;
    public float amoebaAttackStrength;
    public float amoebaReproductiveFervor;
    public float amoebaLifespan;

    public SimulationData(int iteration, int bacteriumCount, int cyanobacteriumCount, float cyanobacteriaMovementSpeed, float cyanobacteriaSize, float cyanobacteriaMutationChance, float cyanobacteriaMetabolicCost, float cyanbacteriaNutritionEfficiency, float cyanobacteriaReproductiveFervor, float cyanobacteriaLifespan, float cyanobacteriaEvasiveness, int amoebaCount, float amoebaMovementSpeed, float amoebaSize, float amoebaMutationChance, float amoebaSensoryFocus, float amoebaMetabolicCost, float amoebaAttackStrength, float amoebaReproductiveFervor, float amoebaLifespan)
    {
        this.iteration = iteration;
        this.bacteriumCount = bacteriumCount;
        this.cyanobacteriumCount = cyanobacteriumCount;
        this.cyanobacteriaMovementSpeed = cyanobacteriaMovementSpeed;
        this.cyanobacteriaSize = cyanobacteriaSize;
        this.cyanobacteriaMutationChance = cyanobacteriaMutationChance;
        this.cyanobacteriaMetabolicCost = cyanobacteriaMetabolicCost;
        this.cyanbacteriaNutritionEfficiency = cyanbacteriaNutritionEfficiency;
        this.cyanobacteriaReproductiveFervor = cyanobacteriaReproductiveFervor;
        this.cyanobacteriaLifespan = cyanobacteriaLifespan;
        this.cyanobacteriaEvasiveness = cyanobacteriaEvasiveness;
        this.amoebaCount = amoebaCount;
        this.amoebaMovementSpeed = amoebaMovementSpeed;
        this.amoebaSize = amoebaSize;
        this.amoebaMutationChance = amoebaMutationChance;
        this.amoebaSensoryFocus = amoebaSensoryFocus;
        this.amoebaMetabolicCost = amoebaMetabolicCost;
        this.amoebaAttackStrength = amoebaAttackStrength;
        this.amoebaReproductiveFervor = amoebaReproductiveFervor;
        this.amoebaLifespan = amoebaLifespan;
    }

    public static SimulationData empty = new SimulationData(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    public static string[] Parse(SimulationData data)
    {
        string[] strings = {
            data.iteration.ToString(),
            data.bacteriumCount.ToString(),
            data.cyanobacteriumCount.ToString(),
            data.cyanobacteriaMovementSpeed.ToString(),
            data.cyanobacteriaSize.ToString(),
            data.cyanobacteriaMutationChance.ToString(),
            data.cyanobacteriaMetabolicCost.ToString(),
            data.cyanbacteriaNutritionEfficiency.ToString(),
            data.cyanobacteriaReproductiveFervor.ToString(),
            data.cyanobacteriaLifespan.ToString(),
            data.cyanobacteriaEvasiveness.ToString(),
            data.amoebaCount.ToString(),
            data.amoebaMovementSpeed.ToString(),
            data.amoebaSize.ToString(),
            data.amoebaMutationChance.ToString(),
            data.amoebaSensoryFocus.ToString(),
            data.amoebaMetabolicCost.ToString(),
            data.amoebaAttackStrength.ToString(),
            data.amoebaReproductiveFervor.ToString(),
            data.amoebaLifespan.ToString(),
        };

        return strings;
    }
}