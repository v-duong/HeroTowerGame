using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoulAbilityPanel : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Button useButton;
    public ActorAbility currentSoulAbility;
    public AbilityTargetType targetType;
    public Actor currentTarget;
    private TargetingCircle activeAbilityTargeting;

    public TargetingCircle ActiveAbilityTargeting
    {
        get
        {
            if (activeAbilityTargeting == null)
            {
                activeAbilityTargeting = Instantiate(ResourceManager.Instance.TargetingCirclePrefab);
                activeAbilityTargeting.SetColor(new Color(0.2f, 0.6f, 1f));
                activeAbilityTargeting.gameObject.SetActive(false);
            }
            return activeAbilityTargeting;
        }
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (currentSoulAbility == null || currentSoulAbility.AbilityOwner.Data.IsDead || !currentSoulAbility.AbilityOwner.gameObject.activeSelf)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            if (currentSoulAbility.AbilityOwner.Data.CurrentSoulPoints < currentSoulAbility.soulCost)
            {
                currentSoulAbility = null;
                currentTarget = null;
                this.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateTarget(Actor target)
    {
        currentTarget = target;
        if (currentTarget != null && currentSoulAbility != null)
        {
            if ((targetType == AbilityTargetType.ALLY && currentTarget is HeroActor) || (targetType == AbilityTargetType.ENEMY && currentTarget is EnemyActor))
            {
                useButton.interactable = true;
                useButton.GetComponentInChildren<TextMeshProUGUI>().text = "Use Ability";

                if (currentSoulAbility.abilityBase.useAreaAroundTarget)
                {
                    ActiveAbilityTargeting.transform.SetParent(target.transform, false);
                    activeAbilityTargeting.transform.localPosition = Vector3.zero;
                    ActiveAbilityTargeting.transform.localScale = new Vector2(currentSoulAbility.AreaRadius*2, currentSoulAbility.AreaRadius*2);
                    ActiveAbilityTargeting.gameObject.SetActive(true);
                }
                else
                {
                    ActiveAbilityTargeting.gameObject.SetActive(false);
                }
            }
            else
            {
                useButton.GetComponentInChildren<TextMeshProUGUI>().text = "Invalid Target";
                useButton.interactable = false;
                ActiveAbilityTargeting.gameObject.SetActive(false);
            }
        }
        else
        {
            useButton.GetComponentInChildren<TextMeshProUGUI>().text = "No Target";
            useButton.interactable = false;
            ActiveAbilityTargeting.gameObject.SetActive(false);
        }
    }

    public void ActivatePanel(ActorAbility ability)
    {
        if (ability != null && ability.abilityBase.isSoulAbility)
        {
            currentSoulAbility = ability;
            currentTarget = null;
            targetType = ability.abilityBase.targetType;
            this.gameObject.SetActive(true);
            nameText.text = "Target an " + LocalizationManager.Instance.GetLocalizationText(targetType) + " for " + currentSoulAbility.abilityBase.LocalizedName;
            useButton.GetComponentInChildren<TextMeshProUGUI>().text = "No Target";
            useButton.interactable = false;
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

    public void OnClickCancel()
    {
        currentSoulAbility = null;
    }

    public void OnClickUse()
    {
        if (currentSoulAbility != null)
        {
            currentSoulAbility.FireSoulAbility(currentTarget, currentTarget.transform.position);
            this.gameObject.SetActive(false);
            currentSoulAbility = null;
            currentTarget = null;
        }
    }
}