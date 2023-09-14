using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

#region Character



#endregion

public class SC_Character : SC_Entity
{
    #region Variables
    public SC_Weapon weapon_;
    public Jobs job_;
    public int exp;

    private Animator anim;
    #endregion

    #region Inititiation
    public void CopyFrom(SC_Character other)
    {
        this.weapon_ = other.weapon_;
        this.job_ = other.job_;
        this.exp = other.exp;
        this.name_ = other.name_;
        this.sprite_ = other.sprite_;
        this.health_ = other.health_;
        this.mana_ = other.mana_;
        this.intellect_ = other.intellect_;
        this.strength_ = other.strength_;
        this.defence_ = other.defence_;
        this.agility_ = other.agility_;
        this.MagicResistance = other.MagicResistance;
        this.Evasion = other.Evasion;
        this.Accuracy = other.Accuracy;
        this.MagicPower = other.MagicPower;
        this.level = other.level;
        this.weakness_ = other.weakness_;
        this.spells_ = new List<SC_Magic>();
        foreach (SC_Magic spell in other.spells_)
        {
            this.spells_.Add(new SC_Magic(spell));
        }
    }
    public void Init()
    {
        GetComponent<SpriteRenderer>().sprite = sprite_;
        anim = GetComponent<Animator>();
        anim.Play("Idle_" + name_);
        InitSpells();
    }

    private void InitSpells()
    {
        foreach (SC_Magic magic in SC_Magic.Allspells_)
        {
            if (magic.jobs_.Contains(job_))
            {
                spells_.Add(magic);
            }
        }
    }
    #endregion

    #region Logic
    public int CastSpell(SC_Magic spell, SC_Monster target)
    {
        if (spell.SFX_ != null)
            spell.SFX_.Play();
        mana_ -= spell.mpCost_;
        int spellDamage = CalculateSpellDamage(spell);
        if (spell.type_ == target.weakness_)
            spellDamage += 10;
        target.health_ -= spellDamage;
        target.GetComponentInChildren<TMP_Text>().text = "-" + spellDamage.ToString();
        StartCoroutine(RestMonsterText(2f, target));
        return target.health_;
    }

    public void Heal(SC_Magic spell, SC_Character target)
    {
        if (spell.SFX_ != null)
            spell.SFX_.Play();
        int healingAmount = spell.attack_ + (level * MagicPower);
        target.GetComponentInChildren<TMP_Text>().text = "+" + healingAmount.ToString();
        StartCoroutine(RestPlayerText(2f, target));
        target.health_ += healingAmount;
        mana_ -= spell.mpCost_;
    }

    public void Revive(SC_Magic spell, SC_Character target, int maxHp)
    {
        if (!target.Alive)
        {
            mana_ -= spell.mpCost_;
            target.Alive = true;
            target.health_ = (int)(maxHp * 0.3);
            target.ReturnToIdle();
        }
    }

    public void Buff(SC_Magic spell, SC_Character target)
    {
        if (spell.SFX_ != null)
            spell.SFX_.Play();
        mana_ -= spell.mpCost_;
        switch (spell.name_)
        {
            case "Berserk":
                target.strength_ += spell.attack_;
                break;

            case "Protect":
                target.defence_ += spell.attack_;
                break;

            default:
                break;
        }
    }

    private int CalculateSpellDamage(SC_Magic spell)
    {
        int charMagPOW = intellect_ / 4 + spell.attack_;
        int randomValue = Random.Range(0, charMagPOW + 1);
        return charMagPOW + randomValue;
    }

    public int NormalAttack(SC_Monster target)
    {
        StartCoroutine(MoveLeftAndReturn());
        anim.Play("attack_" + name_);
        int attackDamage = CalculateAttackDamage();
        target.health_ -= attackDamage;
        target.GetComponentInChildren<TMP_Text>().text = "-" + attackDamage.ToString();
        StartCoroutine(RestMonsterText(2f, target));
        return target.health_;
    }

    private int CalculateAttackDamage()
    {
        int totalDamage = strength_ / 2 + weapon_.attack_;
        bool isCriticalHit = Random.Range(0f, 1f) < 0.05f;
        if (isCriticalHit)
        {
            Debug.Log("Critical Hit!");
            totalDamage *= 2;
        }
        return totalDamage;
    }

    public void Win()
    {
        anim.Play("Win_" + name_);
    }
    public void ReturnToIdle()
    {
        anim.Play("Idle_" + name_);
    }

    public void Death()
    {
        StartCoroutine(DeathCoroutine());
    }

    public void TakeDamage(int damage)
    {
        StartCoroutine(DamageCoroutine(damage));
    }

    #endregion

    #region Coroutines
    private IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(4.2f);
        anim.Play("Death_" + name_);
    }

    private IEnumerator MoveLeftAndReturn()
    {
        Vector3 initialPosition = transform.position;
        Vector3 targetPosition = initialPosition + new Vector3(-3f, 0f, 0f);
        while (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 4);
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        while (transform.position != initialPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, initialPosition, Time.deltaTime * 4);
            yield return null;
        }
    }

    private IEnumerator RestMonsterText(float wait, SC_Monster target)
    {
        yield return new WaitForSeconds(wait);
        target.GetComponentInChildren<TMP_Text>().text = "";
    }

    private IEnumerator RestPlayerText(float wait, SC_Character target)
    {
        yield return new WaitForSeconds(wait);
        target.GetComponentInChildren<TMP_Text>().text = "";
    }


    private IEnumerator DamageCoroutine(int damage)
    {
        yield return new WaitForSeconds(2.2f);
        GetComponentInChildren<TMP_Text>().text = "-" + damage.ToString();
        anim.Play("DamageTaken_" + name_);
        StartCoroutine(DamageTextCoroutine());
    }

    private IEnumerator DamageTextCoroutine()
    {
        yield return new WaitForSeconds(1.5f);
        GetComponentInChildren<TMP_Text>().text = "";
    }

}
#endregion

#region Jobs
public enum Jobs
{
    Swordmen,
    Mage,
    Monk,
    Thieve,
    WhiteMage,
    BlackMage,
    Knight,
    Dragoon,
    Summoner,
    Bard,
    Ninja,
    RedMage,
    Samurai,
    DarkKnight,
    Scholar
}
#endregion