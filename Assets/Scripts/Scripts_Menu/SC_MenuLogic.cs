using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using com.shephertz.app42.gaming.multiplayer.client;
using AssemblyCSharp;

public class SC_MenuLogic : MonoBehaviour
{
    
    public enum Screens
    {
        MainMenu, Loading, Options, StudentInfo, Multiplayer
    };

    #region Variables
    public Slider musicSlider;
    public TMP_Text musicSliderNumber;
    public Slider SFXSlider;
    public TMP_Text SFXSliderNumber;
    public Slider dollarSlider;
    public TMP_Text dollarSliderNumber;
    public AudioSource gameMusic;
    public AudioSource gameSFX;

    private Dictionary<string, GameObject> unityObjects;
    private Dictionary<string, GameObject> unityIndicators;

    private Stack<Screens> screensHistory;
    private Screens curScreen;
    private Screens prevScreen;
    bool back = false;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        Init();
        LoadMusic();
        LoadSFX();
    }

    private void Start()
    {
        InitLogic();
        InitSound();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    #endregion
    #region MultiPlayer
    
    #endregion

    #region Music

    private void InitSound()
    {
        gameMusic.volume = musicSlider.value / 100;
        gameSFX.volume = SFXSlider.value / 100;
        musicSlider.onValueChanged.AddListener((e) =>
        {
            musicSliderNumber.text = e.ToString("0");
            gameMusic.volume = e / 100;
        });
        SFXSlider.onValueChanged.AddListener((e) =>
        {
            SFXSliderNumber.text = e.ToString("0");
            gameSFX.volume = e / 100;
        });
        dollarSlider.onValueChanged.AddListener((e) =>
        {
            dollarSliderNumber.text = e.ToString("0") + "$";
        });

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


    #endregion

    #region Logic

    private void Init()
    {
        curScreen = Screens.MainMenu;
        prevScreen = Screens.MainMenu;
        screensHistory = new Stack<Screens>();
        unityObjects = new Dictionary<string, GameObject>();
        GameObject[] _unityObj = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (GameObject g in _unityObj)
        {
            if (unityObjects.ContainsKey(g.name) == false)
                unityObjects.Add(g.name, g);
            else Debug.LogError("This key " + g.name + " Is Already inside the Dictionary!!!");
        }
        unityIndicators = new Dictionary<string, GameObject>();
        GameObject[] _unityInd = GameObject.FindGameObjectsWithTag("Indicators");
        foreach (GameObject g in _unityInd)
        {
            if (unityIndicators.ContainsKey(g.name) == false)
                unityIndicators.Add(g.name, g);
            else Debug.LogError("This key " + g.name + " Is Already inside the Dictionary!!!");
        }
    }

    private void InitLogic()
    {
        if (unityObjects.ContainsKey("Screen_Loading"))
            unityObjects["Screen_Loading"].SetActive(false);
        if (unityObjects.ContainsKey("Screen_Options"))
            unityObjects["Screen_Options"].SetActive(false);
        if (unityObjects.ContainsKey("Screen_StudentInfo"))
            unityObjects["Screen_StudentInfo"].SetActive(false);
        if (unityObjects.ContainsKey("Screen_Multiplayer"))
            unityObjects["Screen_Multiplayer"].SetActive(false);
        foreach (var g in unityIndicators)
        {
            g.Value.SetActive(false);
        }
    }

    public void ChangeScreen(Screens _ToScreen)
    {
        RestIndicators();
        prevScreen = curScreen;
        if (!back)
            screensHistory.Push(prevScreen);
        curScreen = _ToScreen;

        if (unityObjects.ContainsKey("Screen_" + prevScreen))
            unityObjects["Screen_" + prevScreen].SetActive(false);

        if (unityObjects.ContainsKey("Screen_" + curScreen))
            unityObjects["Screen_" + curScreen].SetActive(true);
        back = false;
    }

    #endregion

    #region Controller


    public void Btn_BackLogic()
    {
        back = true;
        ChangeScreen(screensHistory.Pop());
    }

    public void Btn_MainMenu_PlayLogic()
    {
        ChangeScreen(Screens.Loading);
        GlobalVariables.gameState = GlobalVariables.GameState.SinglePlayer;
        SceneManager.LoadScene(2);
    }

    public void Btn_MainMenu_MultiPlayerLogic()
    {
        ChangeScreen(Screens.Loading);
        SC_MultiPlayerLogic._SetPass = Mathf.Round(unityObjects["Slider_Multiplayer_Dollar"].GetComponent<Slider>().value).ToString();
        GlobalVariables.gameState = GlobalVariables.GameState.MultiPlayer;
        SceneManager.LoadScene(3);
    }

    #endregion

    #region Indicators
    public void ShowIndicator(string _name)
    {
        if (unityIndicators.ContainsKey("Indicators_" + _name))
            unityIndicators["Indicators_" + _name].SetActive(true);
    }

    public void ConcealIndicator(string _name)
    {
        if (unityIndicators.ContainsKey("Indicators_" + _name))
            unityIndicators["Indicators_" + _name].SetActive(false);
    }

    public void RestIndicators()
    {
        foreach (var obj in unityIndicators)
        {
            obj.Value.SetActive(false);
        }
    }

    #endregion
}
