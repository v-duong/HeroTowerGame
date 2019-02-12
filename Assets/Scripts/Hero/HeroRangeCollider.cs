using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroRangeCollider : MonoBehaviour {
    public CircleCollider2D skillCollider;
    public AbilityBase attachedSkill;

    HeroRangeCollider(AbilityBase skill, CircleCollider2D collider)
    {

    }
}
