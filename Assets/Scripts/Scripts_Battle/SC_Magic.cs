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
