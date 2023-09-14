using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVariables : MonoBehaviour
{
    public static Dictionary<int, string> finalSelection_ = new Dictionary<int, string>();
    public static string userId = string.Empty;
    public static int turnTime = 200;
    public static GameState gameState;

    public enum GameState
    {
        SinglePlayer,
        MultiPlayer
    }
}
