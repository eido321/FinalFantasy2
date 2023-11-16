using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_MagicController : MonoBehaviour
{
    #region Variables
    public static Dictionary<string, GameObject> unitySounds;
    public static List<SC_Magic> Allspells_;
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

        Allspells_ = new List<SC_Magic>
    {
        new SC_Magic("Fire", 10, 5,unitySounds["Fire"].GetComponent<AudioSource>(), Attributes.Fire, new List<Jobs> { Jobs.Mage, Jobs.Swordmen, Jobs.Monk, Jobs.Thieve }),
        new SC_Magic("Blizzard", 15, 5, unitySounds["Blizzard"].GetComponent<AudioSource>(),Attributes.Ice, new List<Jobs> { Jobs.Mage, Jobs.Swordmen, Jobs.Monk, Jobs.Thieve }),
        new SC_Magic("Thunder", 10, 5,unitySounds["Thunder"].GetComponent<AudioSource>(), Attributes.Electric, new List<Jobs> { Jobs.Mage, Jobs.Swordmen, Jobs.Monk, Jobs.Thieve }),
        new SC_Magic("Cure", 5, 5,unitySounds["Cure"].GetComponent<AudioSource>(), Attributes.Healing, new List<Jobs> { Jobs.WhiteMage }),
        new SC_Magic("Life", 10, 10, Attributes.Resurrection, new List<Jobs> { Jobs.WhiteMage }),
        new SC_Magic("Berserk", 5, 8, unitySounds["Berserk"].GetComponent<AudioSource>(),Attributes.Buff, new List<Jobs> { Jobs.Swordmen }),
        new SC_Magic("Protect", 5, 8, unitySounds["Protect"].GetComponent < AudioSource >(), Attributes.Buff, new List<Jobs> { Jobs.WhiteMage, Jobs.Knight }),
        new SC_Magic("Flare", 60, 20, Attributes.Fire, new List<Jobs> { Jobs.Mage }),
        new SC_Magic("Blizzaga", 55, 20, Attributes.Ice, new List<Jobs> { Jobs.Mage }),
        new SC_Magic("Thundaga", 60, 20, Attributes.Electric, new List<Jobs> { Jobs.Mage }),
        new SC_Magic("Holy", 70, 30,unitySounds["Holy"].GetComponent<AudioSource>(), Attributes.Light, new List<Jobs> { Jobs.WhiteMage }),
        new SC_Magic("Meteor", 80, 40, Attributes.Dark, new List<Jobs> { Jobs.BlackMage }),
        new SC_Magic("Ultima", 90, 50, Attributes.Light, new List<Jobs> { Jobs.WhiteMage, Jobs.BlackMage }),
    };
    }
    #endregion

}
