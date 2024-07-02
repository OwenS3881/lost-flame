using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackable
{
    public void OnAttacked(BasicAttack attack);

    public void OnAttacked(int attackerLevel, int power, int attackerAttack, CharacterType attacker);

    public CharacterType characterType { get; set; }
}
