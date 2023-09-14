using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_MagicController : MonoBehaviour
{
    #region Variables
    public static Dictionary<string, GameObject> unitySounds;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        InitializeUnityObjects();
    }
    #endregion

    #region Inititiation
    private static void InitializeUnityObjects()
    {
        unitySounds = new Dictionary<string, GameObject>();
        GameObject[] _spellSounds = GameObject.FindGameObjectsWithTag("SpellSounds");
        foreach (GameObject g in _spellSounds)
        {
            if (!unitySounds.ContainsKey(g.name))
                unitySounds.Add(g.name, g);
            else
                Debug.LogError("This key " + g.name + " is already inside the Dictionary!!!");
        }
    }
    #endregion

}
