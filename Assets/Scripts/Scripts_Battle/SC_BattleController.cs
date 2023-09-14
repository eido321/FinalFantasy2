using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_BattleController : MonoBehaviour
{
    #region Variables
    public SC_BattleLogic curBattleLogic;
    #endregion

    #region Logic
    public void Run()
    {
        if (curBattleLogic != null)
            curBattleLogic.Run();
    }

    public void DeclareAttack(string attackType)
    {
        if (curBattleLogic != null)
            curBattleLogic.DeclareAttack(attackType);
    }

    public void ChooseSpell(int num)
    {
        if (curBattleLogic != null)
            curBattleLogic.ChooseSpell(num);
    }

    public void Back()
    {
        if (curBattleLogic != null)
            curBattleLogic.Back();
    }

    public void Attack(int num)
    {
        if(curBattleLogic != null)
            curBattleLogic.Attack(num);
    }

    public void Support(int num)
    {
        if (curBattleLogic != null)
            curBattleLogic.Support(num);
    }

    public void Restart()
    {
        if( curBattleLogic != null)
            curBattleLogic.Restart();
    }

    public void ShowIndicator(string _name)
    {
        if(curBattleLogic != null)
            curBattleLogic.ShowIndicator(_name);
    }

    public void ConcealIndicator(string _name)
    {
        if (curBattleLogic != null)
            curBattleLogic.ConcealIndicator(_name);
    }

    public void NextSpellOnList()
    {
        if (curBattleLogic != null)
            curBattleLogic.NextSpellOnList();
    }

    public void PreviousSpelOnlList()
    {
        if (curBattleLogic != null)
            curBattleLogic.PreviousSpelOnlList();
    }

    #endregion
}
