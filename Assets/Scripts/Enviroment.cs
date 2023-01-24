using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Enviroment : MonoBehaviour
{
    [Header("UI")]
    public Slider nutritionSlider;
    [SerializeField] private TMP_Text nutritionText;

    [Header("Main")]
    [SerializeField] public List<NutrientNode> nodes = new();

    private void Update()
    {
        nutritionText.text = "Nutrition Flow: " + Mathf.FloorToInt(nutritionSlider.value);
    }

    public NutrientNode FindClosestNode(Vector2 position)
    {
        float distance = Mathf.Infinity;
        NutrientNode closestNode = nodes[0];

        foreach (NutrientNode node in nodes)
        {
            Transform nodeTransform = node.gameObject.transform;
            Vector2 nodePosition = new Vector2(nodeTransform.position.x, nodeTransform.position.y);
            if (Vector2.Distance(position, nodePosition) < distance)
            {
                closestNode = node;
                distance = Vector2.Distance(position, nodePosition);
            }
        }

        return closestNode;
    }
}
