using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Smelt Recipe Data"), System.Serializable]
public class SmeltRecipeData : ScriptableObject
{
    public float TimeRequired;
    public ResourceStorageData ResourceOne;
    public int ResourceOneRequirement;
    public ResourceStorageData ResourceTwo;
    public int ResourceTwoRequirement;
    [Space(10f)]
    public int RecipeGain;
}
