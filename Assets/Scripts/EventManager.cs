using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void SimulationEvent();
    public static event SimulationEvent OnSimulationBegin;
    public static event SimulationEvent OnSimulationEnd;

    public delegate void BioformAction(Bacterium bacterium);
    public static event BioformAction OnBioformCreated;
    public static event BioformAction OnBioformDestroyed;


    public static void SimulationBegin()
    {
        if (OnSimulationBegin != null)
        {
            OnSimulationBegin.Invoke();
        }
    }

    public static void SimulationEnd()
    {
        if (OnSimulationEnd != null)
        {
            OnSimulationEnd.Invoke();
        }
    }

    public static void BioformCreated(Bacterium bacterium)
    {
        if (OnBioformCreated != null)
        {
            OnBioformCreated.Invoke(bacterium);
        }
    }

    public static void BioformDestroyed(Bacterium bacterium)
    {
        if (OnBioformDestroyed != null)
        {
            OnBioformDestroyed.Invoke(bacterium);
        }
    }
}
