using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_AudioPlay : MonoBehaviour
{
    #region Variables
    public AudioSource audioSourceAttack;
    public AudioSource audioSourceDamage;
    #endregion

    #region Logic
    public void PlayAudio_Attack()
    {
        audioSourceAttack.Play();
    }

    public void PlayAudio_DamageTaken()
    {
        audioSourceDamage.Play();
    }
    #endregion

}
