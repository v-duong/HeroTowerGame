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

    private void Update()
    {
        if (currentSoulAbility == null)
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
        if (currentTarget != null)
        {
            if ((targetType == AbilityTargetType.ALLY && currentTarget is HeroActor) || (targetType == AbilityTargetType.ENEMY && currentTarget is EnemyActor))
            {

                useButton.interactable = true;
                useButton.GetComponentInChildren<TextMeshProUGUI>().text = "Use Ability";
            } else
            {
                useButton.GetComponentInChildren<TextMeshProUGUI>().text = "Invalid Target";
                useButton.interactable = false;
            }
        } else
        {
            useButton.GetComponentInChildren<TextMeshProUGUI>().text = "No Target";
            useButton.interactable = false;
        }
    }

    public void ActivatePanel(ActorAbility ability)
    {
        if (ability.abilityBase.isSoulAbility)
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