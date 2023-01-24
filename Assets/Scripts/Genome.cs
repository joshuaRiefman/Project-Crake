using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Genome
{
    public Gene[] genes;

    public Genome(Gene[] genes)
    {
        this.genes = genes;
    }
 
}
