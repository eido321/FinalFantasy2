using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Magic
{
    #region Variables
    public string name_;
    public int attack_;
    public int mpCost_;
    public Attributes type_;
    public AudioSource SFX_=null;
    public List<Jobs> jobs_ = new List<Jobs>();
    #endregion

    #region Constructors
    public SC_Magic(string name, int attack, int mpCost_, Attributes type, List<Jobs> jobs)
    {
        this.name_ = name;
        this.attack_ = attack;
        this.mpCost_ = mpCost_;
        this.type_ = type;
        this.jobs_ = jobs;
    }
    public SC_Magic(string name, int attack, int mpCost_, AudioSource SFX_, Attributes type, List<Jobs> jobs)
    {
        this.name_ = name;
        this.attack_ = attack;
        this.mpCost_ = mpCost_;
        this.SFX_ = SFX_;
        this.type_ = type;
        this.jobs_ = jobs;
    }
    public SC_Magic(SC_Magic other)
    {
        this.name_ = other.name_;
        this.attack_ = other.attack_;
        this.mpCost_ = other.mpCost_;
        this.SFX_ = other.SFX_;
        this.type_ = other.type_;
        this.jobs_ = new List<Jobs>(other.jobs_);
    }
    #endregion

    #region Inititiation
    public static List<SC_Magic> Allspells_ = new List<SC_Magic>
    {
        new SC_Magic("Fire", 10, 5,SC_MagicController.unitySounds["Fire"].GetComponent<AudioSource>(), Attributes.Fire, new List<Jobs> { Jobs.Mage, Jobs.Swordmen, Jobs.Monk, Jobs.Thieve }),
        new SC_Magic("Blizzard", 15, 5, SC_MagicController.unitySounds["Blizzard"].GetComponent<AudioSource>(),Attributes.Ice, new List<Jobs> { Jobs.Mage, Jobs.Swordmen, Jobs.Monk, Jobs.Thieve }),
        new SC_Magic("Thunder", 10, 5,SC_MagicController.unitySounds["Thunder"].GetComponent<AudioSource>(), Attributes.Electric, new List<Jobs> { Jobs.Mage, Jobs.Swordmen, Jobs.Monk, Jobs.Thieve }),
        new SC_Magic("Cure", 5, 5,SC_MagicController.unitySounds["Cure"].GetComponent<AudioSource>(), Attributes.Healing, new List<Jobs> { Jobs.WhiteMage }),
        new SC_Magic("Life", 10, 10, Attributes.Resurrection, new List<Jobs> { Jobs.WhiteMage }),
        new SC_Magic("Berserk", 5, 8, SC_MagicController.unitySounds["Berserk"].GetComponent<AudioSource>(),Attributes.Buff, new List<Jobs> { Jobs.Swordmen }),
        new SC_Magic("Protect", 5, 8, SC_MagicController.unitySounds["Protect"].GetComponent < AudioSource >(), Attributes.Buff, new List<Jobs> { Jobs.WhiteMage, Jobs.Knight }),
        new SC_Magic("Flare", 60, 20, Attributes.Fire, new List<Jobs> { Jobs.Mage }),
        new SC_Magic("Blizzaga", 55, 20, Attributes.Ice, new List<Jobs> { Jobs.Mage }),
        new SC_Magic("Thundaga", 60, 20, Attributes.Electric, new List<Jobs> { Jobs.Mage }),
        new SC_Magic("Holy", 70, 30,SC_MagicController.unitySounds["Holy"].GetComponent<AudioSource>(), Attributes.Light, new List<Jobs> { Jobs.WhiteMage }),
        new SC_Magic("Meteor", 80, 40, Attributes.Dark, new List<Jobs> { Jobs.BlackMage }),
        new SC_Magic("Ultima", 90, 50, Attributes.Light, new List<Jobs> { Jobs.WhiteMage, Jobs.BlackMage }),
    };
    #endregion
}

#region Attributes
public enum Attributes
{
    Fire,
    Ice,
    Electric,
    Earth,
    Healing,
    Resurrection,
    Buff,
    Light,
    Dark
}
#endregion
