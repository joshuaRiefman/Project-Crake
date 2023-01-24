using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Precursor : Bacterium
{
    /*[SerializeField] private GameObject cyanobacteria;


    private void Start()
    {
        Debug.Log("Life has begun.");
        CreateGenes();
        Spawn();
    }

    private void CreateGenes()
    {
        Gene movement = new Gene("Movement Focus", 0, 1, 1, true);
        Gene reproductionCost = new Gene("Reproduction Cost", 1, 1, 1, false);
        Gene mutationChance = new Gene("Mutation Chance", 2, 1, 1, true);
        Gene nutritionIntakeEfficiency = new Gene("Metabolic Efficiency", 3, 1, 1, false);
        bioformGenome = new Gene[4];
        bioformGenome[0] = movement;
        bioformGenome[1] = reproductionCost;
        bioformGenome[2] = mutationChance;
        bioformGenome[3] = nutritionIntakeEfficiency;
    }

    private void Spawn()
    {
        GameObject newBacteria = Instantiate(cyanobacteria, this.transform.position, Quaternion.identity);
        newBacteria.GetComponent<Bioform>().bioformGenome = this.bioformGenome;
        Destroy(this.gameObject);
    }*/
}
