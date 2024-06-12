using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Craft Recipe Data"), System.Serializable]
public class CraftRecipeData : ScriptableObject
{
    public float TimeRequired;
    public ResourceStorageData ResourceOne;
    public int ResourceOneRequirement;
    public ResourceStorageData ResourceTwo;
    public int ResourceTwoRequirement;
    [Space(10f)]
    public int RecipeGain;
}
