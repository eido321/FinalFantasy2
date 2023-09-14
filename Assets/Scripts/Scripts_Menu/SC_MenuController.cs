using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SC_MenuLogic;

public class SC_MenuController : MonoBehaviour
{
    #region Variables
    public SC_MenuLogic curMenuLogic;
    #endregion

    #region Logic
    public void Btn_BackLogic()
    {
        if (curMenuLogic != null)
            curMenuLogic.Btn_BackLogic();
    }
    public void Btn_MainMenu_Play()
    {
        if (curMenuLogic != null)
            curMenuLogic.Btn_MainMenu_PlayLogic();
    }

    public void Btn_MainMenu_MultiPlayerLogic()
    {
        if (curMenuLogic != null)
            curMenuLogic.Btn_MainMenu_MultiPlayerLogic();
    }

    public void Btn_ChangeScreen(string _ScreenName)
    {
        if (curMenuLogic != null)
        {
            try
            {
                Screens _toScreen = (Screens)Enum.Parse(typeof(Screens), _ScreenName);
                curMenuLogic.ChangeScreen(_toScreen);
            }
            catch (Exception e)
            {
                Debug.LogError("Fail to convert: " + e.ToString());
            }
        }
    }

    public void Show_Indicator(string _name)
    {
        if (curMenuLogic != null)
        {
            curMenuLogic.ShowIndicator(_name);
        }
    }

    public void Conceal_Indicator(string _name)
    {
        if (curMenuLogic != null)
        {
            curMenuLogic.ConcealIndicator(_name);
        }
    }

    #endregion

}
