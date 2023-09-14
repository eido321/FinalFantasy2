using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class SC_OptionBattleLogic : MonoBehaviour
{
    #region Variables
    public GameObject menuBox_;
    public GameObject optionBox_;
    bool menuSeen = false;
    bool optionSeen = false;

    public Slider musicSlider;
    public TMP_Text musicSliderNumber;
    public Slider SFXSlider;
    public TMP_Text SFXSliderNumber;
    public List<AudioSource> gameMusic;
    public List<AudioSource> gameSFX;

    public static bool _Open = false;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        LoadMusic();
        LoadSFX();
    }

    private void Start()
    {
        InitSound();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            if (menuSeen)
            {
                _Open = false;
                menuBox_.SetActive(false);
                menuSeen = false;
            }
            else
            {
                _Open = true;
                menuBox_.SetActive(true);
                menuSeen = true;
            }
    }
    #endregion

    #region Inititiation
    private void InitSound()
    {
        optionBox_.SetActive(false);
        menuBox_.SetActive(false);
        setVolumeAudioSource(gameMusic, musicSlider.value / 100);
        setVolumeAudioSource(gameSFX, SFXSlider.value / 100);
        musicSlider.onValueChanged.AddListener((e) =>
        {
            musicSliderNumber.text = e.ToString("0");
            setVolumeAudioSource(gameMusic, e / 100);
        });
        SFXSlider.onValueChanged.AddListener((e) =>
        {
            SFXSliderNumber.text = e.ToString("0");
            setVolumeAudioSource(gameSFX, e / 100);
        });
    }
    #endregion

    #region Logic

    private void setVolumeAudioSource(List<AudioSource> audio, float volume)
    {
        foreach (AudioSource audioSource in audio)
        {
            audioSource.volume = volume;
        }
    }

    public void SaveMusic()
    {
        PlayerPrefs.SetFloat("MusicValue", musicSlider.value);
        LoadMusic();
    }

    private void LoadMusic()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicValue");
        musicSliderNumber.text = PlayerPrefs.GetFloat("MusicValue").ToString("0");
    }

    public void SaveSFX()
    {
        PlayerPrefs.SetFloat("SFXValue", SFXSlider.value);
        LoadSFX();
    }

    private void LoadSFX()
    {
        SFXSlider.value = PlayerPrefs.GetFloat("SFXValue");
        SFXSliderNumber.text = PlayerPrefs.GetFloat("SFXValue").ToString("0");
    }

    public void Options()
    {
        if (optionSeen)
        {
            menuBox_.SetActive(true);
            optionBox_.SetActive(false);
            optionSeen = false;
        }
        else
        {
            menuBox_.SetActive(false);
            optionBox_.SetActive(true);
            optionSeen = true;
        }
    }

    public void Quit()
    {
        GlobalVariables.finalSelection_.Clear();
        SceneManager.LoadScene(0);
    }
    #endregion



}
