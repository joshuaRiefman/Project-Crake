using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Gene
{
    public string geneName;
    public int id;
    public float expressionStrength;
    public float metabolicCostModifier;

    public Gene(string geneName, int id, float expressionStrength, float metabolicCostModifier)
    {
        this.geneName = geneName;
        this.id = id;
        this.expressionStrength = expressionStrength;
        this.metabolicCostModifier = metabolicCostModifier;
    }
}
