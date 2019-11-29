using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroAbilityScrollWindow : MonoBehaviour
{
    [SerializeField]
    private HeroUIAbilitySlot SlotPrefab;

    private Stack<HeroUIAbilitySlot> SlotsInUse = new Stack<HeroUIAbilitySlot>();
    private Stack<HeroUIAbilitySlot> AvailableSlots = new Stack<HeroUIAbilitySlot>();

    [NonSerialized]
    public static int slot;

    private void Start()
    {
        SetGridCellSize();
    }

    private void SetGridCellSize()
    {
        GridLayoutGroup grid = GetComponent<GridLayoutGroup>();
        float ySize = 275;
        if (GameManager.Instance.aspectRatio >= 1.85)
        {
            grid.cellSize = new Vector2(200, ySize);
        }
        else
        {
            grid.cellSize = new Vector2(230, ySize);
        }
    }

    public void OnEnable()
    {
        InitializeAbilitySlots();
        ((RectTransform)transform).anchoredPosition = Vector3.zero;
    }

    public void InitializeAbilitySlots()
    {
        HeroData hero = HeroDetailWindow.hero;

        foreach (HeroUIAbilitySlot slot in SlotsInUse)
        {
            slot.gameObject.SetActive(false);
            AvailableSlots.Push(slot);
        }
        SlotsInUse.Clear();
        AddAbilitySlot(null, null, 0);

        foreach (var ability in hero.PrimaryArchetype.AvailableAbilityList)
        {
            AddAbilitySlot(ability.abilityBase, hero.PrimaryArchetype, hero.GetAbilityLevel(ability.abilityBase));
        }
        if (hero.SecondaryArchetype != null)
        {
            foreach (var ability in hero.SecondaryArchetype.AvailableAbilityList)
            {
                AddAbilitySlot(ability.abilityBase, hero.SecondaryArchetype, hero.GetAbilityLevel(ability.abilityBase));
            }
        }
        foreach (AbilityCoreItem abilityItem in GameManager.Instance.PlayerStats.AbilityInventory)
        {
            AddAbilitySlot(abilityItem.Base, abilityItem, hero.GetAbilityLevel(abilityItem.Base));
        }
    }

    public void AddAbilitySlot(AbilityBase ability, IAbilitySource abilitySource, int level)
    {
        HeroUIAbilitySlot slot;
        if (AvailableSlots.Count > 0)
        {
            slot = AvailableSlots.Pop();
        }
        else
        {
            slot = Instantiate(SlotPrefab, transform);
        }
        slot.gameObject.SetActive(true);
        SlotsInUse.Push(slot);

        slot.SetSlot(ability, abilitySource, level);

        if (ability == null)
        {
            slot.infoText.text = "Remove Ability From Slot";
            return;
        }
    }
}