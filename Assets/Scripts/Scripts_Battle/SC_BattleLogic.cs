using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

public class SC_BattleLogic : MonoBehaviour
{
    #region Variables
    public Dictionary<string, GameObject> unityObjects;
    public List<SC_Character> playerCharacters = new List<SC_Character>();
    public List<SC_Monster> monsterCharacters = new List<SC_Monster>();
    public List<GameObject> playerSlots = new List<GameObject>();
    public AudioSource[] battleMusic_;

    private TMP_Text[] battleTexts_Box3;
    private TMP_Text[] monsterTexts_Box2;
    private TMP_Text[] actionTexts_Box1;
    private TMP_Text[] actionTexts_Box1_Spells;
    
    private int battleTexts_Box3_part1 = 3;
    private int battleTexts_Box3_part2 = 7;
    private int battleTexts_Box3_part3 = 12;

    private Dictionary<string, GameObject> unityIndicators;

    private bool magicAttack = false;
    private bool normalAttack = false;

    private int turnOrder_ = 0;
    private bool Over = false;

    private SC_Character activePlayer = null;

    private List<int> maxHpPlayersHistory = new List<int>();
    private List<int> maxMpPlayersHistory = new List<int>();
    private List<int> maxHpMonstersHistory = new List<int>();
    private List<int> maxMpMonstersHistory = new List<int>();
    private List<int> maxDfPlayersHistory = new List<int>();
    private List<int> maxAtPlayersHistory = new List<int>();

    private Dictionary<string, GameObject> allCharacters;

    private SC_Magic chosenSpell = null;
    private int numOfSpells = 4;
    private int numSpells = 0;

    #endregion

    #region Inititiation
    public void StartGameLogic()
    {
        InitAwake();
        InitPlayers();
        Init();
        InitBattle();
        InitHistory();
    }

    private void InitAwake()
    {
        unityObjects = new Dictionary<string, GameObject>();
        GameObject[] _obj = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (GameObject g in _obj)
            unityObjects.Add(g.name, g);
    }

    private void Init()
    {
        unityObjects["Finish_box"].SetActive(false);
        unityIndicators = new Dictionary<string, GameObject>();
        GameObject[] _unityInd = GameObject.FindGameObjectsWithTag("Indicators");
        foreach (GameObject g in _unityInd)
        {
            if (unityIndicators.ContainsKey(g.name) == false)
                unityIndicators.Add(g.name, g);
            else Debug.LogError("This key " + g.name + " Is Already inside the Dictionary!!!");
        }

        foreach (var g in unityIndicators)
            g.Value.SetActive(false);
    }

    private void InitPlayers()
    {
        allCharacters = new Dictionary<string, GameObject>();
        GameObject[] _unityChar = GameObject.FindGameObjectsWithTag("Character");
        foreach (GameObject g in _unityChar)
        {
            if (allCharacters.ContainsKey(g.name) == false)
                allCharacters.Add(g.name, g);
            else Debug.LogError("This key " + g.name + " Is Already inside the Dictionary!!!");
        }
        foreach (GameObject obj in _unityChar)
        {
            obj.SetActive(false);
        }
        for (int i = 0; i < GlobalVariables.finalSelection_.Count; i++)
        {
            if (allCharacters.ContainsKey("Players_Player_" + GlobalVariables.finalSelection_[i]))
            {
                playerCharacters.Add(allCharacters["Players_Player_" + GlobalVariables.finalSelection_[i]].GetComponent<SC_Character>());
            }
        }

        int count = 0;
        foreach (SC_Character pla in playerCharacters)
        {
            playerSlots[count].gameObject.AddComponent<SC_Character>().CopyFrom(pla);
            playerSlots[count].GetComponent<SC_Character>().Init();
            count++;
        }
        playerCharacters.Clear();
        foreach (GameObject pla in playerSlots)
        {
            playerCharacters.Add(pla.GetComponent<SC_Character>());
        }
    }
    private void InitHistory()
    {
        foreach (SC_Character character in playerCharacters)
        {
            maxHpPlayersHistory.Add(character.health_);
            maxMpPlayersHistory.Add(character.mana_);
            maxDfPlayersHistory.Add(character.defence_);
            maxAtPlayersHistory.Add(character.strength_);
        }
        foreach (SC_Monster mosnter in monsterCharacters)
        {
            maxHpMonstersHistory.Add(mosnter.health_);
            maxMpMonstersHistory.Add(mosnter.mana_);
        }
    }


    private void InitBattle()
    {
        unityObjects["battle_box_MagicList"].SetActive(false);
        actionTexts_Box1_Spells = unityObjects["battle_box_MagicList"].GetComponentsInChildren<TMP_Text>();
        monsterTexts_Box2 = unityObjects["battle_box_monsters"].GetComponentsInChildren<TMP_Text>();
        int count1 = 0;
        foreach (TMP_Text textComponent in monsterTexts_Box2)
        {
            textComponent.text = monsterCharacters[count1].name_;
            count1++;
            if (count1 == monsterCharacters.Count)
                break;
        }
        actionTexts_Box1 = unityObjects["battle_box_actionBox"].GetComponentsInChildren<TMP_Text>();
        battleTexts_Box3 = unityObjects["battle_box_characters"].GetComponentsInChildren<TMP_Text>();
        count1 = 0;
        int count2 = 0;
        foreach (TMP_Text textComponent in battleTexts_Box3)
        {
            if (count1 <= battleTexts_Box3_part1)
                textComponent.text = playerCharacters[count2].name_;
            if (count1 <= battleTexts_Box3_part2 && count1 > battleTexts_Box3_part1)
                textComponent.text = playerCharacters[count2].health_.ToString() + "/" + playerCharacters[count2].health_.ToString();
            if (count1 > battleTexts_Box3_part2 && count1 < battleTexts_Box3_part3)
                textComponent.text = playerCharacters[count2].mana_.ToString();
            if (count1 >= battleTexts_Box3_part3)
                textComponent.text = "Attack";

            count1++;
            count2++;
            if (count2 == 4)
                count2 = 0;
        }
        turnSwitch();
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

    public void ResetIndicators()
    {
        foreach (var obj in unityIndicators)
        {
            obj.Value.SetActive(false);
        }
    }

    public void RestChoise()
    {
        foreach (TMP_Text txt in actionTexts_Box1)
            txt.GetComponent<EventTrigger>().enabled = true;
    }

    #endregion

    #region Run
    public float BaseEscapeChance = 0.5f;

    private float CalculateEscapeChance(SC_Character character)
    {
        int avg = 0;
        foreach (SC_Monster monster in monsterCharacters)
        {
            avg += monster.level;
        }
        avg /= monsterCharacters.Count;
        float escapeChance = BaseEscapeChance + (character.agility_ - avg) * 0.05f;
        escapeChance = Mathf.Clamp01(escapeChance);
        return escapeChance;
    }
    private bool AttemptEscape(SC_Character character)
    {
        float escapeChance = CalculateEscapeChance(character);
        float randomValue = Random.value;
        if (randomValue <= escapeChance)
            return true;
        else
            return false;
    }
    public void Run()
    {
        if (!Over)
        {
            foreach (TMP_Text txt in actionTexts_Box1)
                txt.GetComponent<EventTrigger>().enabled = false;
            int count = 0;
            bool check = false;
            for (int i = 0; i < playerCharacters.Count; i++)
            {
                check = AttemptEscape(playerCharacters[i]);
                if (check)
                {
                    battleTexts_Box3[i + 12].text = "Runinng";
                    count++;
                }
            }
            if (count == playerCharacters.Count)
            {
                Over = true;
                Debug.Log("Escaped!");
                GlobalVariables.finalSelection_.Clear();
                unityObjects["Finish_box"].SetActive(true);
                unityObjects["Finish_box"].GetComponentInChildren<TMP_Text>().text = "You Escaped";
                StartCoroutine(EscapeFinish());
            }
            else
            {
                foreach (TMP_Text txt in actionTexts_Box1)
                    txt.GetComponent<EventTrigger>().enabled = true;
                Debug.Log("Not escaped!");
                StartCoroutine(ReturnToAttack());
                turnSwitch();
            }
        }
    }

    private IEnumerator EscapeFinish()
    {
        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene(0);
    }

    private IEnumerator ReturnToAttack()
    {
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < playerCharacters.Count; i++)
        {
            battleTexts_Box3[i + 12].text = "Attack";
        }
    }
    #endregion

    #region Attack
    
    public void DeclareAttack(string attackType)
    {
        if (!Over)
        {
            if (attackType == "Normal")
            {
                normalAttack = true;
                magicAttack = false;
                actionTexts_Box1[0].GetComponent<EventTrigger>().enabled = false;
                ConcealIndicator("box_1_magic");
                ShowIndicator("box_1_attack");
            }
            if (attackType == "Magic")
            {
                updateActivePlayer();
                normalAttack = false;
                ConcealIndicator("box_1_attack");
                unityObjects["battle_box_actionBox"].SetActive(false);
                unityObjects["battle_box_MagicList"].SetActive(true);
                for (int i = 0; i < numOfSpells; i++)
                    actionTexts_Box1_Spells[i].enabled = true;
                for (int i = 0; i < activePlayer.spells_.Count && i < numOfSpells; i++)
                    actionTexts_Box1_Spells[i].text = activePlayer.spells_[i].name_;
                for (int i = activePlayer.spells_.Count; i < numOfSpells; i++)
                {
                    actionTexts_Box1_Spells[i].text = "";
                    actionTexts_Box1_Spells[i].enabled = false;
                }
            }
        }
    }

    public void NextSpellOnList()
    {
        updateActivePlayer();
        if (activePlayer.spells_.Count <= 4)
            return;
        numSpells++;
        if (numSpells + numOfSpells > activePlayer.spells_.Count)
        {
            numSpells--;
            return;
        }
        for (int i = 0; i < activePlayer.spells_.Count && i < numOfSpells; i++)
            actionTexts_Box1_Spells[i].text = activePlayer.spells_[i+numSpells].name_;

    }

    public void PreviousSpelOnlList()
    {
        updateActivePlayer();
        if (activePlayer.spells_.Count <= 4)
            return;
        numSpells--;
        if(numSpells < 0)
        {
            numSpells++;
            return;
        }
        for (int i = 0; i < activePlayer.spells_.Count && i < numOfSpells; i++)
            actionTexts_Box1_Spells[i].text = activePlayer.spells_[i + numSpells].name_;
    }

    public void ChooseSpell(int num)
    {
        int tmp = num;
        if (num > activePlayer.spells_.Count)
            return;
        updateActivePlayer();
        chosenSpell = activePlayer.spells_[num+numSpells];
        magicAttack = true;
        for (int i = 0; i < activePlayer.spells_.Count && i < numOfSpells; i++)
        {
            actionTexts_Box1_Spells[i].GetComponent<EventTrigger>().enabled = true;
            tmp = i + 1;
            ConcealIndicator("box1_MagicList_" + tmp);
        }

        actionTexts_Box1_Spells[num].GetComponent<EventTrigger>().enabled = false;
        tmp = num + 1;
        ShowIndicator("box1_MagicList_" + tmp);
        if (chosenSpell.type_ == Attributes.Healing || chosenSpell.type_ == Attributes.Buff || chosenSpell.type_ == Attributes.Resurrection)
        {
            foreach (TMP_Text txt in monsterTexts_Box2)
            {
                txt.raycastTarget = false;
            }
            for (int i = 0; i < playerCharacters.Count; i++)
            {
                battleTexts_Box3[i].raycastTarget = true;
            }

        }
        if (chosenSpell.type_ == Attributes.Healing || chosenSpell.type_ == Attributes.Buff)
            CheckPlayersDeath();
    }

    public void Back()
    {
        ConcealIndicator("box_1_magic");
        unityObjects["battle_box_actionBox"].SetActive(true);
        unityObjects["battle_box_MagicList"].SetActive(false);
    }
    public void Attack(int num)
    {
        int tmp = monsterCharacters[num].health_;

        if (normalAttack)
        {
            updateActivePlayer();
            tmp = activePlayer.NormalAttack(monsterCharacters[num]);
            monsterCharacters[num].health_ = tmp;
            monsterCharacters[num].DamageTaken();
        }
        if (magicAttack)
        {
            updateActivePlayer();
            unityObjects["battle_box_actionBox"].SetActive(true);
            unityObjects["battle_box_MagicList"].SetActive(false);
            if (activePlayer.mana_ - chosenSpell.mpCost_ < 0)
            {
                Debug.Log("Not enough mana points!");
                tmp = num + 1;
                ConcealIndicator("box1_MagicList_" + tmp);
                return;
            }
            tmp = activePlayer.CastSpell(chosenSpell, monsterCharacters[num]);
            int placeOfActivePlayerInText = turnOrder_ + 7;
            battleTexts_Box3[placeOfActivePlayerInText].text = playerCharacters[turnOrder_ - 1].mana_.ToString();
            foreach (TMP_Text txt in actionTexts_Box1_Spells)
                txt.GetComponent<EventTrigger>().enabled = true;
            for (int i = 0; i < numOfSpells; i++)
                actionTexts_Box1_Spells[i].enabled = true;
        }
        if (magicAttack || normalAttack)
        {
            updateMonsterDeath(num, tmp);
            magicAttack = false;
            normalAttack = false;
            RestChoise();
            CheckGameStatus();
        }
    }

    public void Support(int num)
    {
        int tmp;
        foreach (TMP_Text txt in monsterTexts_Box2)
            txt.raycastTarget = true;
        for (int i = 0; i < playerCharacters.Count; i++)
            battleTexts_Box3[i].raycastTarget = false;
        updateActivePlayer();
        if (activePlayer.mana_ - chosenSpell.mpCost_ < 0)
        {
            Debug.Log("Not enough mana points!");
            tmp = num + 1;
            ConcealIndicator("box1_MagicList_" + tmp);
            return;
        }
        if (chosenSpell.type_ == Attributes.Healing)
        {
            activePlayer.Heal(chosenSpell, playerCharacters[num]);
            if (playerCharacters[num].health_ > maxHpPlayersHistory[num])
                playerCharacters[num].health_ = maxHpPlayersHistory[num];
            UpdateTextValue(battleTexts_Box3[num + 4], playerCharacters[num].health_.ToString());
        }
        if (chosenSpell.type_ == Attributes.Resurrection)
        {
            if (playerCharacters[num].Alive)
                return;
            activePlayer.Revive(chosenSpell, playerCharacters[num], maxHpPlayersHistory[num]);
            battleTexts_Box3[num].color = Color.white;
            battleTexts_Box3[num].raycastTarget = true;
            battleTexts_Box3[num + 4].color = Color.white;
            battleTexts_Box3[num + 8].color = Color.white;
            battleTexts_Box3[num + 12].color = Color.white;
        }
        if (chosenSpell.type_ == Attributes.Buff)
            activePlayer.Buff(chosenSpell, playerCharacters[num]);
        foreach (TMP_Text txt in actionTexts_Box1_Spells)
            txt.GetComponent<EventTrigger>().enabled = true;
        int placeOfActivePlayerInText = turnOrder_ + 7;
        battleTexts_Box3[placeOfActivePlayerInText].text = playerCharacters[turnOrder_ - 1].mana_.ToString();
        magicAttack = false;
        for (int i = 0; i < numOfSpells; i++)
            actionTexts_Box1_Spells[i].enabled = true;
        RestChoise();
        CheckGameStatus();
        StartCoroutine(delayedTurnSwitch());
    }

    private void MonsterAttack()
    {
        foreach (TMP_Text txt in actionTexts_Box1)
        {
            txt.raycastTarget = false;
        }
        foreach (TMP_Text txt in monsterTexts_Box2)
        {
            txt.raycastTarget = false;
        }
        foreach (SC_Monster monster in monsterCharacters)
        {
            if (monster.Alive)
            {
                int randomNumber = Random.Range(0, 4);
                while (!playerCharacters[randomNumber].Alive)
                {
                    randomNumber = Random.Range(0, 4);
                }
                playerCharacters[randomNumber].health_ -= monster.strength_ - playerCharacters[randomNumber].defence_/5;
                playerCharacters[randomNumber].TakeDamage(monster.strength_);
                if (playerCharacters[randomNumber].health_ < 0)
                {
                    playerCharacters[randomNumber].health_ = 0;
                    battleTexts_Box3[randomNumber].raycastTarget = false;
                }
            }
        }
        int startBoxHP = 4;
        int endBoxHP = 8;
        for (int i = startBoxHP; i < endBoxHP; i++)
            UpdateTextValue(battleTexts_Box3[i], playerCharacters[i - 4].health_.ToString());
        StartCoroutine(bringBackRyacast());
    }

    private IEnumerator delayedTurnSwitch()
    {
        yield return new WaitForSeconds(1f);
        unityObjects["battle_box_actionBox"].SetActive(true);
        unityObjects["battle_box_MagicList"].SetActive(false);
        turnSwitch();
    }

    private IEnumerator bringBackRyacast()
    {
        yield return new WaitForSeconds(4f);
        foreach (TMP_Text txt in actionTexts_Box1)
            txt.raycastTarget = true;
        foreach (TMP_Text txt in monsterTexts_Box2)
            txt.raycastTarget = true;
    }

    private void UpdateTextValue(TMP_Text myText, string newHealth)
    {
        string originalText = myText.text;
        if (myText != null)
        {
            string[] parts = originalText.Split('/');
            if (parts.Length == 2)
                myText.text = newHealth + "/" + parts[1];
        }
    }
    #endregion

    #region Turn Logic
    private void turnSwitch()
    {
        numSpells = 0;
        TurnValidity();
        while (!Over && !playerCharacters[turnOrder_].Alive)
        {
            turnOrder_++;
            TurnValidity();
        }
        if (Over)
        {
            foreach (TMP_Text txt in monsterTexts_Box2)
                txt.raycastTarget = false;
            foreach (TMP_Text txt in actionTexts_Box1)
                txt.raycastTarget = false;
            return;
        }
        ResetIndicators();
        switch (turnOrder_)
        {
            case 0:
                ShowIndicator("box_3_battle1");
                break;

            case 1:
                ShowIndicator("box_3_battle2");
                break;

            case 2:
                ShowIndicator("box_3_battle3");
                break;

            case 3:
                ShowIndicator("box_3_battle4");
                break;
            default:
                break;
        }

        turnOrder_++;
    }
    private void TurnValidity()
    {
        if (turnOrder_ >= 4)
        {
            MonsterAttack();
            CheckPlayersDeath();
            turnOrder_ = turnOrder_ % 4;
        }

    }
    #endregion

    #region GameOver Logic
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

    private void updateMonsterDeath(int num, int tmp)
    {
        if (tmp <= 0)
        {
            monsterCharacters[num].Alive = false;
            StartCoroutine(FadeCoroutine(2f, 0f, monsterCharacters[num].GetComponent<SpriteRenderer>()));
            switch (num)
            {
                case 0:
                    monsterTexts_Box2[0].color = Color.gray;
                    monsterTexts_Box2[0].raycastTarget = false;
                    break;

                case 1:
                    monsterTexts_Box2[1].color = Color.gray;
                    monsterTexts_Box2[1].raycastTarget = false;
                    break;

                case 2:
                    monsterTexts_Box2[2].color = Color.gray;
                    monsterTexts_Box2[2].raycastTarget = false;
                    break;

                case 3:
                    monsterTexts_Box2[3].color = Color.gray;
                    monsterTexts_Box2[3].raycastTarget = false;
                    break;
                default:
                    break;
            }
        }
        turnSwitch();
    }

    private void CheckPlayersDeath()
    {
        for (int i = 0; i < 4; i++)
        {
            if (playerCharacters[i].health_ <= 0)
            {
                battleTexts_Box3[i].raycastTarget = false;
                playerCharacters[i].Alive = false;
                playerCharacters[i].Death();
                battleTexts_Box3[i].color = Color.gray;
                battleTexts_Box3[i].raycastTarget = false;
                battleTexts_Box3[i + 4].color = Color.gray;
                battleTexts_Box3[i + 8].color = Color.gray;
                battleTexts_Box3[i + 12].color = Color.gray;
            }
        }
        CheckGameStatus();
    }

    private void CheckGameStatus()
    {
        int count1 = 0;
        foreach (SC_Character character in playerCharacters)
        {
            if (character.Alive == false)
                count1++;
        }
        int count2 = 0;
        foreach (SC_Monster monster in monsterCharacters)
        {
            if (monster.Alive == false)
                count2++;
        }
        if (count1 == playerCharacters.Count || count2 == monsterCharacters.Count)
        {
            ResetIndicators();
            Over = true;
            Debug.Log("Game is over");
            battleMusic_[0].Stop();
            unityObjects["Finish_box"].SetActive(true);
            if (AlivePlayers() == 0)
            {
                battleMusic_[1].Play();
                unityObjects["Finish_box"].GetComponentInChildren<TMP_Text>().text = "You Lose";
            }
            else
            {
                battleMusic_[2].Play();
                unityObjects["Finish_box"].GetComponentInChildren<TMP_Text>().text = "You Win";
                foreach (SC_Character player in playerCharacters)
                    player.Win();
            }
            unityObjects["Menu_box"].SetActive(true);

        }
    }

    private int AlivePlayers()
    {
        int count = 0;
        foreach (SC_Character character in playerCharacters)
        {
            if (character.Alive == false)
                count++;
        }
        count = playerCharacters.Count - count;
        return count;
    }

    private void updateActivePlayer()
    {
        switch (turnOrder_ - 1)
        {
            case 0:
                activePlayer = playerCharacters[0];
                break;

            case 1:
                activePlayer = playerCharacters[1];
                break;

            case 2:
                activePlayer = playerCharacters[2];
                break;

            case 3:
                activePlayer = playerCharacters[3];
                break;
            default:
                break;
        }
    }
    #endregion

    #region Restart
    public void Restart()
    {
        unityObjects["Menu_box"].SetActive(false);
        StartCoroutine(RestartLogic());
    }

    private IEnumerator RestartLogic()
    {
        yield return new WaitForSeconds(3f);
        unityObjects["Finish_box"].SetActive(false);
        battleMusic_[1].Stop();
        battleMusic_[2].Stop();
        battleMusic_[0].Play();
        for (int i = 0; i < playerCharacters.Count; i++)
        {
            playerCharacters[i].health_ = maxHpPlayersHistory[i];
            playerCharacters[i].mana_ = maxMpPlayersHistory[i];
            playerCharacters[i].defence_ = maxDfPlayersHistory[i];
            playerCharacters[i].strength_ = maxAtPlayersHistory[i];
            playerCharacters[i].Alive = true;
            playerCharacters[i].ReturnToIdle();

        }
        for (int i = 0; i < monsterCharacters.Count; i++)
        {
            monsterCharacters[i].health_ = maxHpPlayersHistory[i];
            monsterCharacters[i].mana_ = maxMpPlayersHistory[i];
            monsterCharacters[i].Alive = true;
            StartCoroutine(FadeCoroutine(2f, 100f, monsterCharacters[i].GetComponent<SpriteRenderer>()));
        }
        int startBoxHP = 4;
        int endBoxHP = 8;
        int endBoxMP = 12;
        for (int i = startBoxHP; i < endBoxHP; i++)
            UpdateTextValue(battleTexts_Box3[i], playerCharacters[i - 4].health_.ToString());
        for (int i = endBoxHP; i < endBoxMP; i++)
            battleTexts_Box3[i].text = playerCharacters[i - 8].mana_.ToString();
        foreach (TMP_Text txt in monsterTexts_Box2)
        {
            txt.raycastTarget = true;
            txt.color = Color.white;
        }
        foreach (TMP_Text txt in battleTexts_Box3)
        {
            txt.raycastTarget = true;
            txt.color = Color.white;
        }

    }
    #endregion
}


