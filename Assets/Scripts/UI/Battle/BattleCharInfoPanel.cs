using System.Linq;
using System.Text;
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
    public SoulAbilityPanel SoulAbilityPanel;

    public Button unsummonButton;
    public Button movementButton;
    private bool confirmUnsummon = false;
    private static StringBuilder stringBuilder = new StringBuilder(128);

    public void Update()
    {
        stringBuilder.Clear();
        if (actor == null || actor.Data.IsDead || !actor.isActiveAndEnabled)
        {
            this.gameObject.SetActive(false);
            targetName = null;
            actor = null;
            infoText.text = "";
            statusText.text = "";
            SoulAbilityPanel.UpdateTarget(null);
            return;
        }
        stringBuilder.AppendFormat("Lv{0} {1}\n", actor.Data.Level, targetName);
        if (actor.Data.CurrentHealth < 1 && actor.Data.CurrentHealth > 0)
            stringBuilder.AppendFormat("Health: {0:N2}/{1:N0}", actor.Data.CurrentHealth, actor.Data.MaximumHealth);
        else
            stringBuilder.AppendFormat("Health: {0:N0}/{1:N0}", actor.Data.CurrentHealth, actor.Data.MaximumHealth);

        if (actor.Data.MaximumManaShield > 0)
        {
            stringBuilder.AppendFormat("\nShield: {0:N0}/{1:N0}", actor.Data.CurrentManaShield, actor.Data.MaximumManaShield);
        }
        statusText.text = "";

        if (actor is HeroActor hero)
        {
            stringBuilder.AppendFormat("\nSP: {0:N0}/{1:N0}", actor.Data.CurrentSoulPoints, actor.Data.MaximumSoulPoints);

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
                soulAbilityButton.interactable = true;
                if (soulAbility.currentSoulCooldownTimer > 0)
                {
                    soulAbilityButton.interactable = false;
                    soulAbilityImageFill.fillAmount = soulAbility.currentSoulCooldownTimer / soulAbility.soulCooldown;
                }
                else
                {
                    soulAbilityImageFill.fillAmount = 0;
                }

                if (hero.Data.CurrentSoulPoints < soulAbility.soulCost)
                {
                    soulAbilityButton.interactable = false;
                }
            }
        }

        infoText.text = stringBuilder.ToString();
        UpdateStatuses();
    }

    private void UpdateStatuses()
    {
        System.Collections.Generic.List<ActorEffect> bleedList = actor.GetStatusEffectAll(EffectType.BLEED);
        if (bleedList.Count > 0)
        {
            statusText.text += "<sprite=13>";
            if (bleedList.Count > 1)
                statusText.text += "x" + bleedList.Count + "\n";
            else
                statusText.text += bleedList[0].GetEffectValue().ToString("N0") + "/s " + bleedList[0].duration.ToString("F1") + "\n";
        }

        System.Collections.Generic.List<ActorEffect> poisonList = actor.GetStatusEffectAll(EffectType.POISON);
        if (poisonList.Count > 0)
        {
            statusText.text += "<sprite=14>x" + poisonList.Count + "\n";
        }

        ActorEffect burn = actor.GetStatusEffect(EffectType.BURN);
        if (burn != null)
        {
            statusText.text += "<sprite=1> " + burn.GetEffectValue().ToString("N0") + "/s " + burn.duration.ToString("F1") + "s\n";
        }

        ActorEffect chill = actor.GetStatusEffect(EffectType.CHILL);
        if (chill != null)
        {
            statusText.text += "<sprite=2> " + chill.GetEffectValue().ToString("N0") + "% " + chill.duration.ToString("F1") + "s\n";
        }

        if (actor.GetStatusEffect(EffectType.ELECTROCUTE) is ElectrocuteEffect electrocute)
        {
            statusText.text += "<sprite=3> " + electrocute.GetEffectValue().ToString("N0") + "/s " + electrocute.duration.ToString("F1") + "s\n";
        }

        if (actor.GetStatusEffect(EffectType.FRACTURE) is FractureEffect fracture)
        {
            statusText.text += "<sprite=4> " + fracture.GetEffectValue().ToString("N0") + "% " + fracture.duration.ToString("F1") + "s\n";
        }

        if (actor.GetStatusEffect(EffectType.PACIFY) is PacifyEffect pacify)
        {
            statusText.text += "<sprite=5> " + pacify.GetEffectValue().ToString("N0") + "% " + pacify.duration.ToString("F1") + "s\n";
        }

        if (actor.GetStatusEffect(EffectType.RADIATION) is RadiationEffect radiation)
        {
            statusText.text += "<sprite=6> " + radiation.GetEffectValue().ToString("N0") + "/s " + radiation.duration.ToString("F1") + "s\n";
        }
    }

    public void SetTarget(Actor actor)
    {
        this.actor = actor;
        if (actor != null)
        {
            SoulAbilityPanel.UpdateTarget(actor);
            this.gameObject.SetActive(true);

            if (actor.GetActorType() == ActorType.ALLY)
            {
                heroControls.SetActive(true);
                targetText.text = LocalizationManager.Instance.GetLocalizationText("primaryTargetingType." + actor.targetingPriority.ToString());
                confirmUnsummon = false;
                SetUnsummonButtonText();
                if (actor is HeroActor hero)
                {
                    var soulAbility = hero.GetSoulAbility();
                    if (soulAbility != null && soulAbility.IsUsable)
                    {
                        soulAbilityButton.gameObject.SetActive(true);
                        soulAbilityButton.GetComponentInChildren<TextMeshProUGUI>().text = soulAbility.abilityBase.LocalizedName + "  -" + soulAbility.soulCost + " SP";
                    }
                    else
                    {
                        soulAbilityButton.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                heroControls.SetActive(false);
            }

            targetName = LocalizationManager.Instance.GetLocalizationText_Enemy(actor.Data.Name, ".name");
        }
        else
        {
            SoulAbilityPanel.UpdateTarget(null);
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
        SoulAbilityPanel.gameObject.SetActive(false);

        if (actor is HeroActor hero)
        {
            ActorAbility soulAbility = hero.GetSoulAbility();

            if (soulAbility != null && soulAbility.currentSoulCooldownTimer <= 0 && hero.Data.CurrentSoulPoints >= soulAbility.soulCost)
            {
                switch (soulAbility.abilityBase.targetType)
                {
                    case AbilityTargetType.ALL:
                        soulAbility.FireSoulAbility(hero, hero.transform.position, StageManager.Instance.BattleManager.activeHeroes.Cast<Actor>().ToList());
                        return;

                    case AbilityTargetType.SELF:
                        soulAbility.FireSoulAbility(hero, hero.transform.position);
                        return;

                    default:
                        break;
                }

                switch (soulAbility.abilityBase.abilityShotType)
                {
                    case AbilityShotType.HITSCAN_MULTI:
                        soulAbility.FireSoulAbility(hero, hero.transform.position);
                        return;

                    case AbilityShotType.NOVA_AOE:
                        soulAbility.FireSoulAbility(hero, hero.transform.position);
                        return;

                    case AbilityShotType.PROJECTILE_NOVA:
                        soulAbility.FireSoulAbility(hero, hero.transform.position);
                        return;
                }

                SoulAbilityPanel.ActivatePanel(soulAbility);
            }
        }
    }
}