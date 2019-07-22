using UnityEngine;

public class AbilityColliderContainer : MonoBehaviour
{
    public ActorAbility ability;
    public Actor parentActor;
    public Collider2D abilityCollider;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        Actor actor = collision.gameObject.GetComponent<Actor>();
        if (actor != null)
            ability.AddToTargetList(actor);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Actor actor = collision.gameObject.GetComponent<Actor>();
        if (actor != null)
            ability.RemoveFromTargetList(actor);
    }
}