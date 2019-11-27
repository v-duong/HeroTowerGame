using TMPro;
using UnityEngine;

public class BattleCharInfoPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject heroControls;

    private Actor actor;
    private string targetName;

    public TextMeshProUGUI infoText;
    public TextMeshProUGUI targetText;
    public TextMeshProUGUI statusText;

    public void Update()
    {
        if (actor == null || actor.Data.IsDead)
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

        var a = actor.GetStatusEffect(EffectType.CHILL);
        if (a != null)
        {
            statusText.text = "Chill " + a.GetSimpleEffectValue() + "%";
        }
        else
        {
            statusText.text = "";
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
            }
            else
            {
                heroControls.SetActive(false);
            }

            targetName = LocalizationManager.Instance.GetLocalizationText_Enemy(actor.Data.Name, ".name");
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
}