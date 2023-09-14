using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_MultiPlayerController : MonoBehaviour
{
    public void Btn_PlayLogic()
    {
        if (GlobalVariables.finalSelection_.Count == 4)
            SC_MultiPlayerLogic.Instance.Btn_PlayLogic();
    }
}
