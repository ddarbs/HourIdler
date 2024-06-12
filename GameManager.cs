using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Serializable]
    private struct GatherNode
    {
        public float TimeRequired;
        public int GatherGain;
    }
    
    [SerializeField] private GatherNode WoodNode = new GatherNode() { TimeRequired = 5f, GatherGain = 1};
    [SerializeField] private GatherNode CopperNode = new GatherNode() { TimeRequired = 5f, GatherGain = 1};
    [SerializeField] private GatherNode TinNode = new GatherNode() { TimeRequired = 5f, GatherGain = 1};
    [Space(10f)]
    private int Price_BronzeBar = 5;
    private int Price_BronzeSword = 50;
    
    [SerializeField] private ResourceStorageData GoldCoins;
    [Space(10f)]
    [SerializeField] private ResourceStorageData WoodLog;
    [SerializeField] private ResourceStorageData CopperOre;
    [SerializeField] private ResourceStorageData TinOre;
    [Space(10f)]
    [SerializeField] private SmeltRecipeData BronzeBarRecipe;
    [SerializeField] private ResourceStorageData BronzeBar;
    [Space(10f)]
    [SerializeField] private CraftRecipeData BronzeSwordRecipe;
    [SerializeField] private ResourceStorageData BronzeSword;
    [Space(20f)] 
    [SerializeField] private TextMeshProUGUI[] CountTexts;
    [SerializeField] private Slider ProgressBar;
    [SerializeField] private TextMeshProUGUI ProgressBarText;
    [SerializeField] private TextMeshProUGUI StatusText;

    private Coroutine ProgressBarCoroutine, GatherCoroutine, SmeltCoroutine, CraftCoroutine;
    private bool Gathering, Smelting, Crafting;
    private string GatheringType, SmeltingType, CraftingType;

    [SerializeField] private Animator StatusAnimator;
    [SerializeField] private GameObject StatusAnimatorExtras;
    private static readonly int ChoppingAnimation = Animator.StringToHash("Chopping");
    private static readonly int MiningAnimation = Animator.StringToHash("Mining");
    private static readonly int CraftingAnimation = Animator.StringToHash("Crafting");

    private void Start()
    {
        UpdateStatusText();
        UpdateCountTexts();
        ProgressBarCoroutine = null;
    }
    
    private IEnumerator ProgressBarThread()
    {
        int l_ProgressTicks = 0;
        float l_TimeRequired = 0f;
        float l_Tick = 0f;
        ProgressBar.value = 0f;
        
        if (Gathering)
        {
            switch (GatheringType)
            {
                case "Wood Log":
                    l_ProgressTicks = Mathf.CeilToInt(WoodNode.TimeRequired / 0.05f);
                    l_TimeRequired = WoodNode.TimeRequired;
                    break;
                case "Copper Ore":
                    l_ProgressTicks = Mathf.CeilToInt(CopperNode.TimeRequired / 0.05f);
                    l_TimeRequired = CopperNode.TimeRequired;
                    break;
                case "Tin Ore":
                    l_ProgressTicks = Mathf.CeilToInt(TinNode.TimeRequired / 0.05f);
                    l_TimeRequired = TinNode.TimeRequired;
                    break;
            }
        } 
        else if (Smelting)
        {
            switch (SmeltingType)
            {
                case "Bronze Bar":
                    l_ProgressTicks = Mathf.CeilToInt(BronzeBarRecipe.TimeRequired / 0.05f);
                    l_TimeRequired = BronzeBarRecipe.TimeRequired;
                    break;
            }
        }
        else if (Crafting)
        {
            switch (CraftingType)
            {
                case "Bronze Sword":
                    l_ProgressTicks = Mathf.CeilToInt(BronzeSwordRecipe.TimeRequired / 0.05f);
                    l_TimeRequired = BronzeSwordRecipe.TimeRequired;
                    break;
            }
        }

        for (int i = 1; i < l_ProgressTicks; i++)
        {
            ProgressBar.value = (float)i / ((float)l_ProgressTicks);
            ProgressBarText.text = (l_TimeRequired - (float)(i / (float)l_ProgressTicks)*l_TimeRequired).ToString("0.0");
            yield return new WaitForSeconds(0.05f);
        }
        ProgressBarText.text = "";
    }

    private void StopProgressBar()
    {
        if (!Gathering && !Smelting && !Crafting)
        {
            return;
        }
        
        if (Gathering)
        {
            if (GatherCoroutine != null)
            {
                StopCoroutine(GatherCoroutine);
            }
            StatusAnimator.SetBool(ChoppingAnimation, false);
            StatusAnimator.SetBool(MiningAnimation, false);
            Gathering = false;
        }
        else if (Smelting)
        {
            if (SmeltCoroutine != null)
            {
                StopCoroutine(SmeltCoroutine);
            }
            StatusAnimator.SetBool(CraftingAnimation, false);
            StatusAnimatorExtras.SetActive(false);
            Smelting = false;
        }
        else if (Crafting)
        {
            if (CraftCoroutine != null)
            {
                StopCoroutine(CraftCoroutine);
            }
            StatusAnimator.SetBool(CraftingAnimation, false);
            StatusAnimatorExtras.SetActive(false);
            Crafting = false;
        }
        
        StopCoroutine(ProgressBarCoroutine);
        ProgressBar.value = 0f;
        ProgressBarText.text = "";
        UpdateStatusText();
        UpdateCountTexts();
    }

    private void UpdateStatusText()
    {
        string l_Status = "Currently ";
        if (Gathering)
        {
            l_Status = "Gathering " + GatheringType;
        }
        else if (Smelting)
        {
            l_Status = "Smelting " + SmeltingType;
        }
        else if (Crafting)
        {
            l_Status = "Crafting " + CraftingType;
        }
        else
        {
            l_Status = "taking a well deserved break, you earned this";
        }

        StatusText.text = l_Status;
    }

    public void Gather(string _resource)
    {
        StopProgressBar();
        
        Gathering = true;
        GatheringType = _resource;

        UpdateStatusText();
        
        switch (GatheringType)
        {
            case "Wood Log":
                StatusAnimator.SetBool(ChoppingAnimation, true);
                break;
            case "Copper Ore":
            case "Tin Ore":
                StatusAnimator.SetBool(MiningAnimation, true);
                break;
        }
        
        GatherCoroutine = StartCoroutine(GatherThread());
    }

    private IEnumerator GatherThread()
    {
        ProgressBarCoroutine = StartCoroutine(ProgressBarThread());
        switch (GatheringType)
        {
            case "Wood Log":
                yield return new WaitForSeconds(WoodNode.TimeRequired);
                WoodLog.Current_Count += WoodNode.GatherGain;
                break;
            case "Copper Ore":
                yield return new WaitForSeconds(CopperNode.TimeRequired);
                CopperOre.Current_Count += CopperNode.GatherGain;
                break;
            case "Tin Ore":
                yield return new WaitForSeconds(TinNode.TimeRequired);
                TinOre.Current_Count += TinNode.GatherGain;
                break;
        }

        UpdateCountTexts();
        
        if (Gathering)
        {
            GatherCoroutine = StartCoroutine(GatherThread());
        }
    }
    
    public void Smelt(string _item)
    {
        StopProgressBar();
        
        Smelting = true;
        SmeltingType = _item;

        UpdateStatusText();
        
        StatusAnimator.SetBool(CraftingAnimation, true);
        StatusAnimatorExtras.SetActive(true);
        
        SmeltCoroutine = StartCoroutine(SmeltThread());
    }
    
    private IEnumerator SmeltThread()
    {
        ProgressBarCoroutine = StartCoroutine(ProgressBarThread());
        switch (SmeltingType)
        {
            case "Bronze Bar":
                if (BronzeBarRecipe.ResourceOne.Current_Count >= BronzeBarRecipe.ResourceOneRequirement
                && BronzeBarRecipe.ResourceTwo.Current_Count >= BronzeBarRecipe.ResourceTwoRequirement)
                {
                    yield return new WaitForSeconds(BronzeBarRecipe.TimeRequired);
                    BronzeBarRecipe.ResourceOne.Current_Count -= BronzeBarRecipe.ResourceOneRequirement;
                    BronzeBarRecipe.ResourceTwo.Current_Count -= BronzeBarRecipe.ResourceTwoRequirement;
                    BronzeBar.Current_Count += BronzeBarRecipe.RecipeGain;
                }
                else 
                {
                    StopProgressBar();
                    yield break;
                }
                break;
        }

        UpdateCountTexts();
        
        if (Smelting)
        {
            SmeltCoroutine = StartCoroutine(SmeltThread());
        }
    }
    
    public void Craft(string _item)
    {
        StopProgressBar();
        
        Crafting = true;
        CraftingType = _item;

        UpdateStatusText();
        
        StatusAnimator.SetBool(CraftingAnimation, true);
        StatusAnimatorExtras.SetActive(true);
        
        CraftCoroutine = StartCoroutine(CraftThread());
    }
    
    private IEnumerator CraftThread()
    {
        ProgressBarCoroutine = StartCoroutine(ProgressBarThread());
        switch (CraftingType)
        {
            case "Bronze Sword":
                if (BronzeSwordRecipe.ResourceOne.Current_Count >= BronzeSwordRecipe.ResourceOneRequirement
                && BronzeSwordRecipe.ResourceTwo.Current_Count >= BronzeSwordRecipe.ResourceTwoRequirement)
                {
                    yield return new WaitForSeconds(BronzeSwordRecipe.TimeRequired);
                    BronzeSwordRecipe.ResourceOne.Current_Count -= BronzeSwordRecipe.ResourceOneRequirement;
                    BronzeSwordRecipe.ResourceTwo.Current_Count -= BronzeSwordRecipe.ResourceTwoRequirement;
                    BronzeSword.Current_Count += BronzeSwordRecipe.RecipeGain;
                }
                else
                {
                    StopProgressBar();
                    yield break;
                }
                break;
        }

        UpdateCountTexts();
        
        if (Crafting)
        {
            CraftCoroutine = StartCoroutine(CraftThread());
        }
    }
    
    public void Sell(string _item)
    {
        StopProgressBar();
        
        switch (_item)
        {
            case "Bronze Bar":
                if (BronzeBar.Current_Count <= 0)
                {
                    return;
                }
                
                GoldCoins.Current_Count += (BronzeBar.Current_Count * Price_BronzeBar);
                BronzeBar.Current_Count = 0;
                break;
            case "Bronze Sword":
                if (BronzeSword.Current_Count <= 0)
                {
                    return;
                }

                GoldCoins.Current_Count += (BronzeSword.Current_Count * Price_BronzeSword);
                BronzeSword.Current_Count = 0;
                break;
        }
        
        UpdateCountTexts();
    }

    private void UpdateCountTexts()
    {
        for (int i = 0; i < CountTexts.Length; i++)
        {
            CountTexts[i].text = CountTexts[i].name + " : ";
            switch (CountTexts[i].name)
            {
                case "Gold Coins":
                    CountTexts[i].text += GoldCoins.Current_Count.ToString();
                    break;
                case "Wood Log":
                    CountTexts[i].text += WoodLog.Current_Count.ToString();
                    break;
                case "Copper Ore":
                    CountTexts[i].text += CopperOre.Current_Count.ToString();
                    break;
                case "Tin Ore":
                    CountTexts[i].text += TinOre.Current_Count.ToString();
                    break;
                case "Bronze Bar":
                    CountTexts[i].text += BronzeBar.Current_Count.ToString();
                    break;
                case "Bronze Sword":
                    CountTexts[i].text += BronzeSword.Current_Count.ToString();
                    break;
            }
        }
    }
}
