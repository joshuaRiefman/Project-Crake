using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NutrientNode : MonoBehaviour
{
    private Enviroment enviroment;

    [SerializeField] private float maxNodeNutrients = 100;
    [Range(0, 100)] public float nodeNutrients = 50;
    [Range(0, 10)] private const float nutrientProductionModifier = 1.1f; 

    private void Awake()
    {
        enviroment = FindObjectOfType<Enviroment>();
        enviroment.nodes.Add(this);
    }

    private void FixedUpdate()
    {
        if (!SimulationManager.SimulationIsRunning) { return; }
        if (nodeNutrients < maxNodeNutrients)
        {
            RegenerateNutrients();
        }
    }

    public float ConsumeNutrients(float attemptedNutrientExtract)
    {
        float nutrientExtractCoefficient = Mathf.Clamp01(1.25f * Mathf.Log10(nodeNutrients + 40) - 1.5f);
        float extractedNutrients = attemptedNutrientExtract * nutrientExtractCoefficient;

        nodeNutrients -= extractedNutrients;
        return extractedNutrients;

    }

    private void RegenerateNutrients()
    {
        float nutrientRegenerationCoefficient = enviroment.nutritionSlider.value / 10;
        float nutrientRegeneration = maxNodeNutrients * (nutrientRegenerationCoefficient * nutrientProductionModifier) * Time.fixedDeltaTime;
        nodeNutrients += nutrientRegeneration;
    }
    
}
