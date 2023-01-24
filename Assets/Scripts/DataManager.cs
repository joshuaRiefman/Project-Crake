using UnityEngine;
using System.IO;
using System.Collections;

public class DataManager : MonoBehaviour
{
    public static int collectionIteration = 1;

    [SerializeField] private float collectionInterval = 15f;

    private void Start()
    {
        EventManager.OnSimulationBegin += CreateReport;
        EventManager.OnSimulationBegin += BeginCollection;

    }

    private void BeginCollection() => StartCoroutine(DataCollection());

    private IEnumerator DataCollection()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(collectionInterval / Time.timeScale);
            if (!SimulationManager.SimulationIsRunning)
            {
                Debug.Log("Data Collection Cancelled!");
                break;
            }

            AppendToReport(SimulationData.Parse(SimulationManager.AcquireSimulationData(collectionIteration)));
            collectionIteration++;
        }

    }

    private static readonly string reportDirectoryName = "Report";
    private static readonly string reportFileName = "Data.csv";
    private static readonly string reportSeparator = ",";
    private static readonly string[] reportHeaders = new string[20] {
        "Iteration",
        "Bacterium Count",
        "Cyanobacteria Count",
        "Cyanobacteria Movement Speed",
        "Cyanobacteria Size",
        "Cyanobacteria Mutation Chance",
        "Cyanobacteria Metabolic Cost",
        "Cyanobacteria Nutrition Efficiency",
        "Cyanobacteria Reproductive Fervor",
        "Cyanobacteria Lifespan",
        "Cyanobacteria Evasiveness",
        "Amoeba Count",
        "Amoeba Movement Speed",
        "Amoeba Size",
        "Amoeba Mutation Chance",
        "Amoeba Sensory Focus",
        "Amoeba Metabolic Cost",
        "Amoeba Attack Strength",
        "Amoeba Reproductive Fervor",
        "Amoeba Lifespan",

    };
    private static readonly string timeStampHeader = "Time Stamp";


    private static void AppendToReport(string[] strings)
    {
        VerifyDirectory();
        VerifyFile();
        using (StreamWriter sw = File.AppendText(GetFilePath()))
        {
            string finalString = "";
            for (int i = 0; i < strings.Length; i++)
            {
                if (finalString != "")
                {
                    finalString += reportSeparator;
                }
                finalString += strings[i];
            }
            finalString += reportSeparator + GetTimeStamp();
            sw.WriteLine(finalString);
            Debug.Log("Updated Report!");
        }

    }

    public static void CreateReport()
    {
        collectionIteration = 1;

        VerifyDirectory();
        using (StreamWriter sw = File.CreateText(GetFilePath()))
        {
            string finalString = "";
            for (int i = 0; i < reportHeaders.Length; i++)
            {
                if (finalString != "")
                {
                    finalString += reportSeparator;
                }
                finalString += reportHeaders[i];
            }
            finalString += reportSeparator + timeStampHeader;
            sw.WriteLine(finalString);
        }

        Debug.Log("Created Report!");
    }

    private static void VerifyDirectory()
    {
        string dir = GetDirectoryPath();
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    private static void VerifyFile()
    {
        string file = GetFilePath();
        if (!File.Exists(file))
        {
            CreateReport();
        }
    }

    private static string GetDirectoryPath()
    {
        return Application.dataPath + "/" + reportDirectoryName;
    }

    private static string GetFilePath()
    {
        return GetDirectoryPath() + "/" + reportFileName;
    }

    private static string GetTimeStamp()
    {
        return System.DateTime.UtcNow.ToString();
    }

}
