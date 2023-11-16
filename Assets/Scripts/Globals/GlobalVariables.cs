using com.shephertz.app42.gaming.multiplayer.client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVariables : MonoBehaviour
{
    public static Dictionary<int, string> finalSelection_ = new Dictionary<int, string>();
    public static string userId = string.Empty;
    public static int turnTime = 60;
    public static GameState gameState;
    public static string roomId;

    public enum GameState
    {
        SinglePlayer,
        MultiPlayer
    }
}
