using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Entity : MonoBehaviour
{
    #region Variables
    public string name_;
    public Sprite sprite_;
    public int health_;
    public int mana_;
    public int intellect_;
    public int strength_;
    public int defence_;
    public int agility_;
    public int MagicResistance;
    public int Evasion;
    public int Accuracy;
    public int MagicPower;
    public int level;
    public Attributes weakness_;
    public List<SC_Magic> spells_ = new List<SC_Magic>();
    public bool Alive = true;
    #endregion
}
