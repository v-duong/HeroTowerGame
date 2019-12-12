using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleCharInfoPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject heroControls;

    private Actor actor;
    private string targetName;

    public TextMeshProUGUI infoText;
    public TextMeshProUGUI targetText;
    public TextMeshProUGUI statusText;
    public Button soulAbilityButton;
    public Image soulAbilityImageFill;

    public Button unsummonButton;
    public Button movementButton;
    private bool confirmUnsummon = false;

    public void Update()
    {
        if (actor == null || actor.Data.IsDead || !actor.isActiveAndEnabled)
        {
            this.gameObject.SetActive(false);
            targetName = null;
            actor = null;
            infoText.text = "";
            return;
        }
        infoText.text = "Lv" + actor.Data.Level + " " + targetName + "\n";
        if (actor.Data.CurrentHealth < 1 && actor.Data.CurrentHealth > 0)
            infoText.text += "Health: " + actor.Data.CurrentHealth.ToString("F2") + "/" + actor.Data.MaximumHealth;
        else
            infoText.text += "Health: " + actor.Data.CurrentHealth.ToString("F0") + "/" + actor.Data.MaximumHealth;

        if (actor.Data.MaximumManaShield > 0)
        {
            infoText.text += "\nShield: " + actor.Data.CurrentManaShield.ToString("F0") + "/" + actor.Data.MaximumManaShield;
        }
        statusText.text = "";

        if (actor is HeroActor hero)
        {
            if (hero.isBeingRecalled)
            {
                movementButton.interactable = false;
            }
            else
            {
                movementButton.interactable = true;
            }

            if (hero.IsMoving)
                unsummonButton.interactable = false;
            else
                unsummonButton.interactable = true;

            ActorAbility soulAbility = hero.GetSoulAbility();

            if (soulAbility != null)
            {
                if (soulAbility.currentSoulCooldownTimer > 0)
                    soulAbilityImageFill.fillAmount = soulAbility.currentSoulCooldownTimer / soulAbility.soulCooldown;
                else
                    soulAbilityImageFill.fillAmount = 0;
            }
        }
    }

    public void SetTarget(Actor actor)
    {
        this.actor = actor;
        if (actor != null)
        {
            this.gameObject.SetActive(true);

            if (actor.GetActorType() == ActorType.ALLY)
            {
                heroControls.SetActive(true);
                targetText.text = LocalizationManager.Instance.GetLocalizationText("primaryTargetingType." + actor.targetingPriority.ToString());
                confirmUnsummon = false;
                SetUnsummonButtonText();
                if (actor is HeroActor hero)
                    soulAbilityButton.GetComponentInChildren<TextMeshProUGUI>().text = hero.GetSoulAbility().abilityBase.LocalizedName;
            }
            else
            {
                heroControls.SetActive(false);
            }

            targetName = LocalizationManager.Instance.GetLocalizationText_Enemy(actor.Data.Name, ".name");
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

    private void SetUnsummonButtonText()
    {
        if (actor is HeroActor hero && hero.isBeingRecalled)
        {
            unsummonButton.GetComponentInChildren<TextMeshProUGUI>().text = "<b>Cancel Recall?</b>";
        }
        else if (confirmUnsummon)
        {
            unsummonButton.GetComponentInChildren<TextMeshProUGUI>().text = "<b>Confirm Recall?</b>";
        }
        else
        {
            unsummonButton.GetComponentInChildren<TextMeshProUGUI>().text = "Recall Hero";
        }
    }

    public void UnsummonHero()
    {
        InputManager.Instance.selectedHero = null;
        InputManager.Instance.IsMovementMode = false;
        InputManager.Instance.SetTileHighlight(false);
        if (actor is HeroActor hero)
        {
            if (hero.isBeingRecalled)
            {
                hero.StopCurrentRecall();
            }
            else if (!confirmUnsummon)
            {
                confirmUnsummon = true;
            }
            else
            {
                hero.UnsummonHero();
                confirmUnsummon = false;
            }

            SetUnsummonButtonText();
        }
    }

    public void MoveHero()
    {
        InputManager.Instance.selectedHero = actor as HeroActor;
        InputManager.Instance.IsMovementMode = true;
        InputManager.Instance.SetTileHighlight(true);
    }

    public void TargetSelectionOnClick(int i)
    {
        switch (actor.targetingPriority)
        {
            case PrimaryTargetingType.FIRST when i < 0:
                actor.targetingPriority = PrimaryTargetingType.RANDOM;
                break;

            case PrimaryTargetingType.RANDOM when i > 0:
                actor.targetingPriority = PrimaryTargetingType.FIRST;
                break;

            default:
                actor.targetingPriority += i;
                break;
        }

        targetText.text = LocalizationManager.Instance.GetLocalizationText("primaryTargetingType." + actor.targetingPriority.ToString());
    }

    public void SoulAbilityOnClick()
    {
        if (actor is HeroActor hero)
        {
            ActorAbility soulAbility = hero.GetSoulAbility();

            if (soulAbility != null && soulAbility.currentSoulCooldownTimer <= 0 && hero.Data.CurrentSoulPoints >= soulAbility.soulCost)
            {
                soulAbility.currentSoulCooldownTimer = soulAbility.soulCooldown;
            }
        }
    }
}