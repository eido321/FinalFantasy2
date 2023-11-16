using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SC_CharacterSelectionLogic : MonoBehaviour
{
    #region Variables
    public Dictionary<string, GameObject> numberdSelection_ = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> buttonSelection_ = new Dictionary<string, GameObject>();

    private int maxPlayers = 4;

    public GameObject SelectionBox;
    public GameObject GameBox;
    public GameObject _FadeSprite;
    public SC_BattleLogic battleLogic;
    public GameObject rastartBtn;

    #endregion

    #region MonoBehaviour
    void Awake()
    {
        StartCoroutine(FadeCoroutine(2f, 0f, _FadeSprite.GetComponent<SpriteRenderer>()));
        InitSelection();
    }
    #endregion

    #region Inititiation
    private void InitSelection()
    {
        GameBox.SetActive(false);
        GameObject[] _NumbersTxt = GameObject.FindGameObjectsWithTag("NumberedSelection");
        foreach (GameObject g in _NumbersTxt)
        {
            if (!numberdSelection_.ContainsKey(g.name))
                numberdSelection_.Add(g.name, g);
            else
                Debug.LogError("This key " + g.name + " is already inside the Dictionary!!!");
        }
        GameObject[] _NumbersButton = GameObject.FindGameObjectsWithTag("ButtonsSelection");
        foreach (GameObject g in _NumbersButton)
        {
            if (!buttonSelection_.ContainsKey(g.name))
                buttonSelection_.Add(g.name, g);
            else
                Debug.LogError("This key " + g.name + " is already inside the Dictionary!!!");
        }
        foreach (GameObject obj in buttonSelection_.Values)
        {
            obj.GetComponent<Button>().onClick.AddListener(() => SelectCharacter(obj.GetComponent<TMP_Text>().text));
        }

    }
    #endregion

    #region Logic
    public void SelectCharacter(string name)    /* selects a character */
    {
        int count = 0;
        for (int i = 0; i < maxPlayers; i++)
        {
            if (!GlobalVariables.finalSelection_.ContainsKey(i))
            {
                GlobalVariables.finalSelection_.Add(i, name);
                numberdSelection_["Selection_Number_" + name].GetComponent<TMP_Text>().text = (i + 1).ToString();
                buttonSelection_[name + "_name"].GetComponent<Button>().onClick.RemoveAllListeners();
                buttonSelection_[name + "_name"].GetComponent<Button>().onClick.AddListener(() => Remove(name));
                break;
            }
            else
                count++;
        }
        if (count == maxPlayers)
        {
            Debug.Log("Full Team");
        }
    }

    public void Remove(string name)     /* diselect a character */
    {
        int keyToRemove = GlobalVariables.finalSelection_.FirstOrDefault(x => x.Value == name).Key;

        if (keyToRemove >=0 && keyToRemove < maxPlayers)
        {
            GlobalVariables.finalSelection_.Remove(keyToRemove);
            numberdSelection_["Selection_Number_" + name].GetComponent<TMP_Text>().text = "";
            buttonSelection_[name + "_name"].GetComponent<Button>().onClick.RemoveAllListeners();
            buttonSelection_[name + "_name"].GetComponent<Button>().onClick.AddListener(() => SelectCharacter(name));

        }
        else
        {
            Debug.Log("this character isnt in the team");
        }
    }

    public void StartGameSingleLogic()      /* starts the singleplayer game in case you have a full team */
    {
        if (GlobalVariables.finalSelection_.Count == maxPlayers)
        {
            rastartBtn.GetComponent<Button>().interactable = true;
            foreach (GameObject obj in buttonSelection_.Values)
            {
                obj.GetComponent<TMP_Text>().raycastTarget = false;
            }
            SelectionBox.SetActive(false);
            GameBox.SetActive(true);
            battleLogic.StartGameLogic();
        }
        else
            Debug.Log("Must have a full team to play");
    }

    public void StartGameMultiLogic()       /* starts the multiplayer game in case you have a full team */
    {
        if (GlobalVariables.finalSelection_.Count == maxPlayers)
        {
            foreach (GameObject obj in buttonSelection_.Values)
            {
                obj.GetComponent<TMP_Text>().raycastTarget = false;
            }
            SC_MultiPlayerLogic.Instance.Btn_PlayLogic();

        }
        else
            Debug.Log("Must have a full team to play");
    }
    #endregion

    #region Coroutines

    private IEnumerator FadeCoroutine(float duration, float amount, SpriteRenderer spriteMaterial)
    {
        Color startColor = spriteMaterial.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, amount);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            spriteMaterial.color = Color.Lerp(startColor, targetColor, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        spriteMaterial.color = targetColor;
    }
    #endregion
}
