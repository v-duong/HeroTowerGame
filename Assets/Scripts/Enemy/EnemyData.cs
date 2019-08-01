using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyData : ActorData
{
        public EnemyData() : base()
    {
        Id = Guid.NewGuid();
    }

    public void SetBase(EnemyBase enemyBase)
    {
        MaximumHealth = (int)(enemyBase.level * enemyBase.healthScaling + 120);
        CurrentHealth = MaximumHealth;
        movementSpeed = enemyBase.movementSpeed;
        for (int i = 0; i < (int)ElementType.COUNT; i++)
        {
            ElementType element = (ElementType)i;
            Resistances[element] = enemyBase.resistances[i];
        }
    }
}

