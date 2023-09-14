using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_CharacterSelectionController : MonoBehaviour
{
    #region Variables
    public SC_CharacterSelectionLogic curBattleLogic;
    #endregion

    #region Logic
    public void SelectCharacter(string name)
    {
        if(curBattleLogic != null)
            curBattleLogic.SelectCharacter(name);
    }

    public void Remove(string name)
    {
        if (curBattleLogic != null)
            curBattleLogic.Remove(name);
    }

    public void StartGameSingleLogic()
    {
        if (curBattleLogic != null)
            curBattleLogic.StartGameSingleLogic();
    }

    public void StartGameMultiLogic()
    {
        if (curBattleLogic != null)
            curBattleLogic.StartGameMultiLogic();
    }
    #endregion
}
