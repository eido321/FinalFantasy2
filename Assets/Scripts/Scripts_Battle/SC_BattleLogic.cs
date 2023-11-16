using AssemblyCSharp;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class SC_BattleLogic : MonoBehaviour
{
    #region Variables
    public Dictionary<string, GameObject> unityObjects;
    public List<SC_Character> playerCharacters = new List<SC_Character>();
    public List<SC_Monster> monsterCharacters = new List<SC_Monster>();
    public List<GameObject> playerSlots = new List<GameObject>();
    public AudioSource[] battleMusic_;
    public List<SC_Character> enemyPlayerCharacters = new List<SC_Character>();
    public List<GameObject> enemyPlayerSlots = new List<GameObject>();

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
    private bool RestartOk = false;
    public float BaseEscapeChance = 0.5f;

    private SC_Character activePlayer = null;
    //monster history lists
    private List<int> maxHpMonstersHistory = new List<int>();
    private List<int> maxMpMonstersHistory = new List<int>();
    //charchter lists
    private List<int> maxHpPlayersHistory = new List<int>();
    private List<int> maxMpPlayersHistory = new List<int>();
    private List<int> maxDfPlayersHistory = new List<int>();
    private List<int> maxAtPlayersHistory = new List<int>();
    //enemy ccharacter history lists
    private List<int> maxHpEnemyPlayersHistory = new List<int>();
    private List<int> maxMpEnemyPlayersHistory = new List<int>();
    private List<int> maxDfEnemyPlayersHistory = new List<int>();
    private List<int> maxAtEnemyPlayersHistory = new List<int>();

    private Dictionary<string, GameObject> allCharacters;

    Dictionary<string, object> _toSendMove = new Dictionary<string, object>();
    private SC_Magic chosenSpell = null;
    private int numOfSpells = 4;
    private int maxPlayers = 4;
    private int numSpells = 0;
    private bool startTurns = false;
    private string nextTurn;
    private float startTime;
    private bool isMyTurn = false;
    private string _toJson;


    #endregion

    #region MonoBehaviour

    private void OnEnable()
    {
        Listener.OnGameStarted += OnGameStarted;
        Listener.OnMoveCompleted += OnMoveCompleted;
        Listener.OnPrivateChatReceived += OnPrivateChatReceived;
        Listener.OnGetLiveRoomInfo += OnGetLiveRoomInfo;
        Listener.OnUserJoinRoom += OnUserJoinRoom;
        Listener.OnUserLeftRoom += OnUserLeftRoom;
    }

    private void OnDisable()
    {
        Listener.OnGameStarted -= OnGameStarted;
        Listener.OnMoveCompleted -= OnMoveCompleted;
        Listener.OnPrivateChatReceived -= OnPrivateChatReceived;
        Listener.OnGetLiveRoomInfo -= OnGetLiveRoomInfo;
        Listener.OnUserJoinRoom -= OnUserJoinRoom;
    }

    #endregion

    #region Inititiation

    private void Awake()
    {
        InitAwake();

    }
    public void StartGameLogic()
    {
        if (GlobalVariables.gameState == GlobalVariables.GameState.SinglePlayer)
            InitPlayers(playerCharacters, playerSlots, GlobalVariables.finalSelection_);
        InitBattle();
        InitHistory();
    }

    private void InitAwake()
    {
        unityObjects = new Dictionary<string, GameObject>();
        GameObject[] _obj = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (GameObject g in _obj)
            unityObjects.Add(g.name, g);
        unityObjects["GameUI"].SetActive(true);
        Init();
        if (GlobalVariables.gameState == GlobalVariables.GameState.MultiPlayer)
        {
            unityObjects["Txt_TurnStatus"].GetComponent<TMP_Text>().text = "Enemy player turn";
            unityObjects["Txt_Loading_Title_dots"].SetActive(true);
            UpdateGameAction(false);
        }

        unityObjects["GameUI"].SetActive(false);
    }

    private void Init()
    {
        unityObjects["Finish_box"].SetActive(false);
        unityObjects["Mana_box"].SetActive(false);
        if (GlobalVariables.gameState == GlobalVariables.GameState.MultiPlayer)
            unityObjects["Menu_Restart"].SetActive(false);
        actionTexts_Box1_Spells = unityObjects["battle_box_MagicList"].GetComponentsInChildren<TMP_Text>();
        monsterTexts_Box2 = unityObjects["battle_box_monsters"].GetComponentsInChildren<TMP_Text>();
        actionTexts_Box1 = unityObjects["battle_box_actionBox"].GetComponentsInChildren<TMP_Text>();
        battleTexts_Box3 = unityObjects["battle_box_characters"].GetComponentsInChildren<TMP_Text>();
        unityIndicators = new Dictionary<string, GameObject>();
        GameObject[] _unityInd = GameObject.FindGameObjectsWithTag("Indicators");
        foreach (GameObject g in _unityInd)
        {
            if (unityIndicators.ContainsKey(g.name) == false)
                unityIndicators.Add(g.name, g);
            else Debug.LogError("This key " + g.name + " Is Already inside the Dictionary!!!");
        }
        foreach (GameObject obj in enemyPlayerSlots)
        {
            obj.GetComponent<SpriteRenderer>().flipX = true;

        }
        foreach (var g in unityIndicators)
            g.Value.SetActive(false);
    }

    private void InitPlayers(List<SC_Character> _playerCharacters, List<GameObject> _playerSlots, Dictionary<int, string> Selection_)
    {
        allCharacters = new Dictionary<string, GameObject>();
        GameObject[] _unityChar = GameObject.FindGameObjectsWithTag("Character");
        foreach (GameObject g in _unityChar)
        {
            if (allCharacters.ContainsKey(g.name) == false)
                allCharacters.Add(g.name, g);
            else Debug.LogError("This key " + g.name + " Is Already inside the Dictionary!!!");
        }

        if (_playerCharacters.Count == 0)
        {
            for (int i = 0; i < Selection_.Count; i++)
            {
                if (allCharacters.ContainsKey("Players_Player_" + Selection_[i]))
                {
                    _playerCharacters.Add(allCharacters["Players_Player_" + Selection_[i]].GetComponent<SC_Character>());
                }
            }
        }

        int count = 0;
        foreach (SC_Character pla in _playerCharacters)
        {
            _playerSlots[count].gameObject.AddComponent<SC_Character>().CopyFrom(pla);
            _playerSlots[count].GetComponent<SC_Character>().Init();
            count++;
        }
        _playerCharacters.Clear();
        foreach (GameObject pla in _playerSlots)
        {
            _playerCharacters.Add(pla.GetComponent<SC_Character>());
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
        if (GlobalVariables.gameState == GlobalVariables.GameState.SinglePlayer)
        {
            foreach (SC_Monster mosnter in monsterCharacters)
            {
                maxHpMonstersHistory.Add(mosnter.health_);
                maxMpMonstersHistory.Add(mosnter.mana_);
            }
        }
        else
        {
            foreach (SC_Character character in enemyPlayerCharacters)
            {
                maxHpEnemyPlayersHistory.Add(character.health_);
                maxMpEnemyPlayersHistory.Add(character.mana_);
                maxDfEnemyPlayersHistory.Add(character.defence_);
                maxAtEnemyPlayersHistory.Add(character.strength_);
            }
        }
    }

    private void InitBattle()
    {
        unityObjects["battle_box_MagicList"].SetActive(false);

        int count1 = 0;
        if (GlobalVariables.gameState == GlobalVariables.GameState.SinglePlayer)
        {
            foreach (TMP_Text textComponent in monsterTexts_Box2)
            {
                textComponent.text = monsterCharacters[count1].name_;
                count1++;
                if (count1 == monsterCharacters.Count)
                    break;
            }
            turnSwitch();
        }

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

    public void ResetIndicators()   /* rests all active indicators */
    {
        foreach (var obj in unityIndicators)
        {
            obj.Value.SetActive(false);
        }
    }

    public void RestChoise()  /* rest the player's choise in case they changed thier mind */
    {
        for (int i = 1; i <= numOfSpells; i++)
        {
            ConcealIndicator("box1_MagicList_" + i.ToString());
            actionTexts_Box1_Spells[i - 1].GetComponent<EventTrigger>().enabled = true;
        }
        foreach (TMP_Text txt in actionTexts_Box1_Spells)
            txt.GetComponent<EventTrigger>().enabled = true;
        int placeOfActivePlayerInText = turnOrder_ + 7;
        battleTexts_Box3[placeOfActivePlayerInText].text = playerCharacters[turnOrder_ - 1].mana_.ToString();
        magicAttack = false;
        normalAttack = false;
        for (int i = 0; i < numOfSpells; i++)
            actionTexts_Box1_Spells[i].enabled = true;
        foreach (TMP_Text txt in actionTexts_Box1)
            txt.GetComponent<EventTrigger>().enabled = true;
    }

    #endregion

    #region Run

    private float CalculateEscapeChance(SC_Character character)  /* returns the calculated escape change of each character acording to the final fantasy statistics */
    {
        int avg = 0;
        if (GlobalVariables.gameState == GlobalVariables.GameState.SinglePlayer)
        {
            foreach (SC_Monster monster in monsterCharacters)
            {
                avg += monster.level;
            }
        }
        else
        {
            foreach (SC_Character enemyCharacter in enemyPlayerCharacters)
            {
                avg += character.level;
            }

        }
        avg /= monsterCharacters.Count;
        float escapeChance = BaseEscapeChance + (character.agility_ - avg) * 0.05f;
        escapeChance = Mathf.Clamp01(escapeChance);
        return escapeChance;
    }
    private bool AttemptEscape(SC_Character character)          /* checks if the chosen character managed to escape */
    {
        float escapeChance = CalculateEscapeChance(character);
        float randomValue = UnityEngine.Random.value;
        if (randomValue <= escapeChance)
            return true;
        else
            return false;
    }
    public void Run()       /* managing the running functionality in case of a success */
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
                if (GlobalVariables.gameState == GlobalVariables.GameState.MultiPlayer)
                {
                    if (!_toSendMove.ContainsKey("Type " + turnOrder_.ToString()))
                        _toSendMove.Add("Type " + turnOrder_.ToString(), "Running");
                    else
                        _toSendMove["Type " + turnOrder_.ToString()] = "Running";

                    _toJson = MiniJSON.Json.Serialize(_toSendMove);
                    WarpClient.GetInstance().sendMove(_toJson);
                    _toSendMove.Clear();
                }
                StartCoroutine(DelayedGameExit());
            }
            else
            {
                if (GlobalVariables.gameState == GlobalVariables.GameState.MultiPlayer)
                    _toSendMove.Add("Type " + turnOrder_.ToString(), "Running Failed");
                foreach (TMP_Text txt in actionTexts_Box1)
                    txt.GetComponent<EventTrigger>().enabled = true;
                Debug.Log("Not escaped!");
                StartCoroutine(ReturnToAttack());
                turnSwitch();
            }
        }
    }

    private IEnumerator DelayedGameExit()       /* Exits the game but in a delay so the player may agknowledge his victory/lose */
    {
        yield return new WaitForSeconds(4f);
        GlobalVariables.finalSelection_.Clear();
        if (GlobalVariables.gameState == GlobalVariables.GameState.MultiPlayer)
        {
            WarpClient.GetInstance().UnsubscribeRoom(GlobalVariables.roomId);
            WarpClient.GetInstance().LeaveRoom(GlobalVariables.roomId);
        }
        SceneManager.LoadScene(0);
    }

    private IEnumerator ReturnToAttack()        /* if the charachters status changed to runinng it will return to the default "Attack" */
    {
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < playerCharacters.Count; i++)
        {
            battleTexts_Box3[i + 12].text = "Attack";
        }
    }
    #endregion

    #region Attack

    public void DeclareAttack(string attackType)        /* the player choses his first action normal attack / magic attack */
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

    public void NextSpellOnList()       /* this will go down in the list of spells a charachter has */
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
            actionTexts_Box1_Spells[i].text = activePlayer.spells_[i + numSpells].name_;

    }

    public void PreviousSpelOnlList()   /* this will go up in the list of spells a charachter has */
    {
        updateActivePlayer();
        if (activePlayer.spells_.Count <= 4)
            return;
        numSpells--;
        if (numSpells < 0)
        {
            numSpells++;
            return;
        }
        for (int i = 0; i < activePlayer.spells_.Count && i < numOfSpells; i++)
            actionTexts_Box1_Spells[i].text = activePlayer.spells_[i + numSpells].name_;
    }

    public void ChooseSpell(int num)        /* in case the player chose to attack with magic he will need to choose a spell */
    {
        chosenSpell = null;
        for(int i=0; i < numOfSpells; i++)
        {
            monsterTexts_Box2[i].raycastTarget = true;
            battleTexts_Box3[i].raycastTarget = false;
        }
        int tmp = num;
        if (num > activePlayer.spells_.Count)
            return;
        updateActivePlayer();
        chosenSpell = activePlayer.spells_[num + numSpells];
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

    public void Back()          /* in case the player doesnt want to atttack with magic and want to choose a different action it can go back */
    {
        for (int i = 1; i <= numOfSpells; i++)
        {
            ConcealIndicator("box1_MagicList_" + i.ToString());
            actionTexts_Box1_Spells[i - 1].GetComponent<EventTrigger>().enabled = true;
            battleTexts_Box3[i-1].raycastTarget = false;
        }
        foreach(TMP_Text txt in actionTexts_Box1)
            txt.GetComponent<EventTrigger>().enabled = true;
        ConcealIndicator("box_1_magic");
        unityObjects["battle_box_actionBox"].SetActive(true);
        unityObjects["battle_box_MagicList"].SetActive(false);
        magicAttack = false;
        normalAttack = false;
        chosenSpell = null;
        numSpells = 0;
        if (GlobalVariables.gameState == GlobalVariables.GameState.SinglePlayer)
        {
            for (int i = 0; i < maxPlayers; i++)
            {
                if (monsterCharacters[i].Alive)
                    monsterTexts_Box2[i].raycastTarget = true;
                else
                    monsterTexts_Box2[i].raycastTarget = false;
            }
        }
        else
        {
            for (int i = 0; i < maxPlayers; i++)
            {
                if (enemyPlayerCharacters[i].Alive)
                    monsterTexts_Box2[i].raycastTarget = true;
                else
                    monsterTexts_Box2[i].raycastTarget = false;
            }
        }
    }
    public void Attack(int num)         /* after the player chose to normal attack or magic attack he chooses the target he wants to attack and commences it  */
    {
        int tmp = 0;
        if (normalAttack)
        {
            updateActivePlayer();
            if (GlobalVariables.gameState == GlobalVariables.GameState.SinglePlayer)
            {
                tmp = activePlayer.NormalAttack(monsterCharacters[num]);
                monsterCharacters[num].health_ = tmp;
                monsterCharacters[num].DamageTaken();
            }
            else
            {
                tmp = activePlayer.NormalAttack(enemyPlayerCharacters[num], -1);
                enemyPlayerCharacters[num].TakeDamage(tmp);
                _toSendMove.Add("Type " + turnOrder_.ToString(), "Attack");
                _toSendMove.Add("AttackType " + turnOrder_.ToString(), "normalAttack");
                _toSendMove.Add("Points " + turnOrder_.ToString(), tmp);
                _toSendMove.Add("PlayerNum " + turnOrder_.ToString(), turnOrder_ - 1);
                _toSendMove.Add("TargetNum " + turnOrder_.ToString(), num);

            }

        }
        if (magicAttack)
        {
            updateActivePlayer();
            unityObjects["battle_box_actionBox"].SetActive(true);
            unityObjects["battle_box_MagicList"].SetActive(false);
            if (activePlayer.mana_ - chosenSpell.mpCost_ < 0)
            {
                unityObjects["Mana_box"].SetActive(true);
                StartCoroutine(ManaMassage());
                numSpells = 0;
                tmp = num + 1;
                ConcealIndicator("box1_MagicList_" + tmp);
                ConcealIndicator("box_1_magic");
                RestChoise();
                return;
            }
            if (GlobalVariables.gameState == GlobalVariables.GameState.SinglePlayer)
                tmp = activePlayer.CastSpell(chosenSpell, monsterCharacters[num]);
            else
            {
                tmp = activePlayer.CastSpell(chosenSpell, enemyPlayerCharacters[num]);
                enemyPlayerCharacters[num].TakeDamage(tmp);
            }
            int placeOfActivePlayerInText = turnOrder_ + 7;
            battleTexts_Box3[placeOfActivePlayerInText].text = playerCharacters[turnOrder_ - 1].mana_.ToString();
            foreach (TMP_Text txt in actionTexts_Box1_Spells)
                txt.GetComponent<EventTrigger>().enabled = true;
            for (int i = 0; i < numOfSpells; i++)
                actionTexts_Box1_Spells[i].enabled = true;
            if (GlobalVariables.gameState == GlobalVariables.GameState.MultiPlayer)
            {
                _toSendMove.Add("Type " + turnOrder_.ToString(), "Attack");
                _toSendMove.Add("AttackType " + turnOrder_.ToString(), "magicAttack");
                _toSendMove.Add("Points " + turnOrder_.ToString(), tmp);
                _toSendMove.Add("PlayerNum " + turnOrder_.ToString(), turnOrder_ - 1);
                _toSendMove.Add("TargetNum " + turnOrder_.ToString(), num);
                _toSendMove.Add("SpellName " + turnOrder_.ToString(), chosenSpell.name_);
            }
        }
        if (magicAttack || normalAttack)
        {

            updateMonsterDeath(num);
            magicAttack = false;
            normalAttack = false;
            foreach (TMP_Text txt in actionTexts_Box1)
                txt.GetComponent<EventTrigger>().enabled = true;
            CheckGameStatus();
        }
    }

    public void Support(int num)        /* in case the player choses a magic attack that supports it needs to target a teamate and activate the spell  */
    {
        int tmp;
        foreach (TMP_Text txt in monsterTexts_Box2)
            txt.raycastTarget = true;
        for (int i = 0; i < playerCharacters.Count; i++)
            battleTexts_Box3[i].raycastTarget = false;
        updateActivePlayer();
        if (activePlayer.mana_ - chosenSpell.mpCost_ < 0)
        {
            unityObjects["Mana_box"].SetActive(true);
            StartCoroutine(ManaMassage());
            numSpells = 0;
            tmp = num + 1;
            ConcealIndicator("box1_MagicList_" + tmp);
            ConcealIndicator("box_1_magic");
            RestChoise();
            return;
        }
        if (chosenSpell.type_ == Attributes.Healing)
        {
            tmp = activePlayer.Heal(chosenSpell, playerCharacters[num]);
            if (playerCharacters[num].health_ > maxHpPlayersHistory[num])
                playerCharacters[num].health_ = maxHpPlayersHistory[num];
            int startBoxHP = 4;
            int endBoxHP = 8;
            int endBoxMP = 12;
            for (int i = startBoxHP; i < endBoxHP; i++)
                UpdateTextValue(battleTexts_Box3[i], playerCharacters[i - 4].health_.ToString());
            for (int i = endBoxHP; i < endBoxMP; i++)
                battleTexts_Box3[i].text = playerCharacters[i - 8].mana_.ToString(); _toSendMove.Add("Type " + turnOrder_.ToString(), "Support");
            _toSendMove.Add("SupportType " + turnOrder_.ToString(), "Healing");
            _toSendMove.Add("Points " + turnOrder_.ToString(), tmp);
            _toSendMove.Add("PlayerNum " + turnOrder_.ToString(), turnOrder_ - 1);
            _toSendMove.Add("TargetNum " + turnOrder_.ToString(), num);
            _toSendMove.Add("SpellName " + turnOrder_.ToString(), chosenSpell.name_);
            _toSendMove.Add("MaxHp " + turnOrder_.ToString(), maxHpPlayersHistory[num]);

        }
        if (chosenSpell.type_ == Attributes.Resurrection)
        {
            if (playerCharacters[num].Alive)
            {
                for (int i = 0; i < numOfSpells; i++)
                {
                    ConcealIndicator("box1_MagicList_" + i.ToString());
                    battleTexts_Box3[i].raycastTarget = false;
                }
                chosenSpell = null;
                RestChoise();
                return;
            }
            tmp = activePlayer.Revive(chosenSpell, playerCharacters[num], maxHpPlayersHistory[num]);
            battleTexts_Box3[num].color = Color.white;
            battleTexts_Box3[num].raycastTarget = true;
            battleTexts_Box3[num + 4].color = Color.white;
            battleTexts_Box3[num + 8].color = Color.white;
            battleTexts_Box3[num + 12].color = Color.white;
            int startBoxHP = 4;
            int endBoxHP = 8;
            int endBoxMP = 12;
            for (int i = startBoxHP; i < endBoxHP; i++)
                UpdateTextValue(battleTexts_Box3[i], playerCharacters[i - 4].health_.ToString());
            for (int i = endBoxHP; i < endBoxMP; i++)
                battleTexts_Box3[i].text = playerCharacters[i - 8].mana_.ToString();
            _toSendMove.Add("Type " + turnOrder_.ToString(), "Support");
            _toSendMove.Add("SupportType " + turnOrder_.ToString(), "Resurrection");
            _toSendMove.Add("Points " + turnOrder_.ToString(), tmp);
            _toSendMove.Add("PlayerNum " + turnOrder_.ToString(), turnOrder_ - 1);
            _toSendMove.Add("TargetNum " + turnOrder_.ToString(), num);
            _toSendMove.Add("SpellName " + turnOrder_.ToString(), chosenSpell.name_);
        }
        if (chosenSpell.type_ == Attributes.Buff)
        {
            activePlayer.Buff(chosenSpell, playerCharacters[num]);
            _toSendMove.Add("Type " + turnOrder_.ToString(), "Support");
            _toSendMove.Add("SupportType " + turnOrder_.ToString(), "Buff");
            _toSendMove.Add("PlayerNum " + turnOrder_.ToString(), turnOrder_ - 1);
            _toSendMove.Add("TargetNum " + turnOrder_.ToString(), num);
            _toSendMove.Add("SpellName " + turnOrder_.ToString(), chosenSpell.name_);
        }
        RestChoise();
        CheckGameStatus();
        StartCoroutine(delayedTurnSwitch());
    }

    private void MonsterAttack()        /* this will start the monster turn to attack */
    {

        foreach (TMP_Text txt in actionTexts_Box1)
        {
            txt.raycastTarget = false;
        }
        foreach (TMP_Text txt in monsterTexts_Box2)
        {
            txt.raycastTarget = false;
        }
        if (GlobalVariables.gameState == GlobalVariables.GameState.SinglePlayer)
        {
            int[] DamageSummery = new int[4];
            foreach (SC_Monster monster in monsterCharacters)
            {
                if (monster.Alive)
                {
                    int randomNumber = UnityEngine.Random.Range(0, 4);
                    while (!playerCharacters[randomNumber].Alive)
                    {
                        randomNumber = UnityEngine.Random.Range(0, 4);
                    }
                    DamageSummery[randomNumber] += monster.strength_ - playerCharacters[randomNumber].defence_ / 5;
                }
            }
            for (int i = 0; i < maxPlayers; i++)
            {
                if (DamageSummery[i] > 0)
                {
                    playerCharacters[i].TakeDamage(DamageSummery[i]);
                    if (playerCharacters[i].health_ < 0)
                    {
                        playerCharacters[i].health_ = 0;
                        battleTexts_Box3[i].raycastTarget = false;
                    }
                }
            }
        }
        
        int startBoxHP = 4;
        int endBoxHP = 8;
        for (int i = startBoxHP; i < endBoxHP; i++)
            UpdateTextValue(battleTexts_Box3[i], playerCharacters[i - 4].health_.ToString());
        if (GlobalVariables.gameState == GlobalVariables.GameState.SinglePlayer)
            StartCoroutine(bringBackRyacast());
    }

    private IEnumerator delayedTurnSwitch()         /* this will switch the turn but after a delay so the monsters may finish their attack */
    {
        yield return new WaitForSeconds(1f);
        unityObjects["battle_box_actionBox"].SetActive(true);
        unityObjects["battle_box_MagicList"].SetActive(false);
        turnSwitch();
    }

    private IEnumerator bringBackRyacast()      /* the button for the players are disabled during the enemy attack this will bring back that functionality */
    {
        yield return new WaitForSeconds(4f);
        UpdateGameAction(true);
    }

    private IEnumerator ManaMassage()       /* in case the character doesnt have enough mana for a spell it will display a massage and the will be disabled by this function */
    {
        yield return new WaitForSeconds(3f);
        unityObjects["Mana_box"].SetActive(false);
    }
    private void UpdateTextValue(TMP_Text myText, string newHealth)         /* update the characters health points in the display */
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
    private void turnSwitch()           /* manages the switch of the turn between characters */
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
    private void TurnValidity()     /* checks that the turn number is on a valid character (example: it cant turn to a dead character) */
    {
        if (turnOrder_ >= 4)
        {
            if (GlobalVariables.gameState == GlobalVariables.GameState.MultiPlayer && _toSendMove.Count > 0)
            {
                _toJson = MiniJSON.Json.Serialize(_toSendMove);
                WarpClient.GetInstance().sendMove(_toJson);
                _toSendMove.Clear();
                UpdateGameAction(false);
                unityObjects["Txt_TurnStatus"].GetComponent<TMP_Text>().text = "Enemy player turn...";
                unityObjects["Txt_Loading_Title_dots"].SetActive(true);
            }
            MonsterAttack();
            CheckPlayersDeath();
            turnOrder_ = turnOrder_ % 4;
        }

    }
    #endregion

    #region GameOver Logic
    private IEnumerator FadeCoroutine(float duration, float amount, SpriteRenderer spriteMaterial)      /* fades a sprite in or out */
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

    private void updateMonsterDeath(int num)     /* in case a monster is killed this function update its information and display */
    {
        if (GlobalVariables.gameState == GlobalVariables.GameState.SinglePlayer && monsterCharacters[num].health_ <= 0)
        {
            monsterCharacters[num].Alive = false;
            StartCoroutine(FadeCoroutine(2f, 0f, monsterCharacters[num].GetComponent<SpriteRenderer>()));
            updateMonsterDeathText(num);
        }
        if (GlobalVariables.gameState == GlobalVariables.GameState.MultiPlayer && enemyPlayerCharacters[num].health_ <= 0)
        {
            enemyPlayerCharacters[num].Alive = false;
            enemyPlayerCharacters[num].Death();
            updateMonsterDeathText(num);
        }
        turnSwitch();
    }

    private void updateMonsterDeathText(int num)       /* updates the monster text displayed on the screen in case its killed */
    {
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

    private void CheckPlayersDeath()
    {
        for (int i = 0; i < 4; i++)
        {
            if (playerCharacters[i].health_ <= 0)
            {
                playerCharacters[i].health_ = 0;
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

    private void CheckGameStatus()      /* checks if the game ended */
    {
        int count1 = 0;
        foreach (SC_Character character in playerCharacters)
        {
            if (character.Alive == false)
                count1++;
        }
        int count2 = 0;
        if (GlobalVariables.gameState == GlobalVariables.GameState.SinglePlayer)
        {
            foreach (SC_Monster monster in monsterCharacters)
            {
                if (monster.Alive == false)
                    count2++;
            }
        }
        else
        {
            foreach (SC_Character enemyCharacter in enemyPlayerCharacters)
            {
                if (enemyCharacter.Alive == false)
                    count2++;
            }
        }
        if (count1 == playerCharacters.Count || count2 == monsterCharacters.Count || (count2 == enemyPlayerCharacters.Count && GlobalVariables.gameState == GlobalVariables.GameState.MultiPlayer))
        {
            ResetIndicators();
            Over = true;
            Debug.Log("Game is over");
            battleMusic_[0].Stop();
            unityObjects["Finish_box"].SetActive(true);
            UpdateGameAction(false);
            if (AlivePlayers() == 0)
            {
                battleMusic_[1].Play();
                unityObjects["Finish_box"].GetComponentInChildren<TMP_Text>().text = "You Lose";
                if (GlobalVariables.gameState == GlobalVariables.GameState.MultiPlayer)
                {
                    foreach (SC_Character player in enemyPlayerCharacters)
                        player.Win();
                }
            }
            else
            {
                battleMusic_[2].Play();
                unityObjects["Finish_box"].GetComponentInChildren<TMP_Text>().text = "You Win";
                _toJson = MiniJSON.Json.Serialize(_toSendMove);
                WarpClient.GetInstance().sendMove(_toJson);
                _toSendMove.Clear();
                foreach (SC_Character player in playerCharacters)
                    player.Win();
            }
        }
    }

    private void UpdateGameAction(bool state)       /* update the funcionality of the buttons in play (in case it is or not the player's turn*/
    {
        foreach (TMP_Text txt in actionTexts_Box1)
            txt.raycastTarget = state;
        if (GlobalVariables.gameState == GlobalVariables.GameState.SinglePlayer)
        {
            if (monsterCharacters.Count > 0)
            {
                if (state)
                {
                    for (int i = 0; i < maxPlayers; i++)
                    {
                        if (monsterCharacters[i].Alive)
                            monsterTexts_Box2[i].raycastTarget = true;
                        else
                            monsterTexts_Box2[i].raycastTarget = false;
                    }
                }
                else
                {
                    foreach (TMP_Text txt in monsterTexts_Box2)
                        txt.raycastTarget = false;
                }
            }

        }
        else
        {
            if (enemyPlayerCharacters.Count > 0)
            {
                if (state)
                {
                    for (int i = 0; i < maxPlayers; i++)
                    {
                        if (enemyPlayerCharacters[i].Alive)
                            monsterTexts_Box2[i].raycastTarget = true;
                        else
                            monsterTexts_Box2[i].raycastTarget = false;
                    }
                }
                else
                {
                    foreach (TMP_Text txt in monsterTexts_Box2)
                        txt.raycastTarget = false;
                }
            }
        }
        normalAttack = false;
        magicAttack = false;
    }

    private int AlivePlayers()          /* checks how many characters are alive in play */
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

    private void updateActivePlayer()       /* update the character in the current turn */
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
    public void Restart()       /* acts as a conroller restart */
    {
        unityObjects["Menu_box"].SetActive(false);
        if (GlobalVariables.gameState == GlobalVariables.GameState.SinglePlayer)
        {
            StartCoroutine(RestartLogic());
        }
        else if (Over)
        {
            Dictionary<string, object> _restart = new Dictionary<string, object>()
            {
                {"Restart",RestartOk },
            };
            _toJson = MiniJSON.Json.Serialize(_restart);
            WarpClient.GetInstance().GetLiveRoomInfo(GlobalVariables.roomId);

        }
    }

    public void AcceptRestart(bool status)      /* checks if the enemy player accepted the restart request */
    {
        RestartOk = status;
        unityObjects["Menu_Restart"].SetActive(false);
        if (RestartOk)
        {
            StartCoroutine(RestartLogic());
            Dictionary<string, object> _restart = new Dictionary<string, object>()
            {
                {"Restart",RestartOk },
            };
            _toJson = MiniJSON.Json.Serialize(_restart);
            WarpClient.GetInstance().GetLiveRoomInfo(GlobalVariables.roomId);
        }
    }


    private IEnumerator RestartLogic()      /* restarts the game so the player may start another game  */
    {
        yield return new WaitForSeconds(3f);
        if (unityObjects["battle_box_MagicList"].activeSelf == true)
            Back();
        unityObjects["Finish_box"].SetActive(false);
        Over = false;
        magicAttack = false;
        normalAttack = false;
        battleMusic_[1].Stop();
        battleMusic_[2].Stop();
        battleMusic_[0].Play();
        for (int i = 0; i < playerCharacters.Count; i++)
        {
            foreach (GameObject characterObj in allCharacters.Values)
            {
                SC_Character character = characterObj.GetComponent<SC_Character>();
                if (playerCharacters[i].name_ == character.GetComponent<SC_Character>().name_)
                {
                    playerCharacters[i].health_ = character.health_;
                    playerCharacters[i].mana_ = character.mana_;
                    playerCharacters[i].defence_ = character.defence_;
                    playerCharacters[i].strength_ = character.strength_;
                }
            }
            playerCharacters[i].Alive = true;
            playerCharacters[i].ReturnToIdle();

        }
        if (GlobalVariables.gameState == GlobalVariables.GameState.SinglePlayer)
        {
            for (int i = 0; i < monsterCharacters.Count; i++)
            {
                monsterCharacters[i].health_ = maxHpMonstersHistory[i];
                monsterCharacters[i].mana_ = maxMpMonstersHistory[i];
                monsterCharacters[i].Alive = true;
                StartCoroutine(FadeCoroutine(2f, 100f, monsterCharacters[i].GetComponent<SpriteRenderer>()));
            }
        }
        else
        {
            for (int i = 0; i < enemyPlayerCharacters.Count; i++)
            {
                foreach (GameObject characterObj in allCharacters.Values)
                {
                    SC_Character character = characterObj.GetComponent<SC_Character>();
                    if (enemyPlayerCharacters[i].name_ == character.GetComponent<SC_Character>().name_)
                    {
                        enemyPlayerCharacters[i].health_ = character.health_;
                        enemyPlayerCharacters[i].mana_ = character.mana_;
                        enemyPlayerCharacters[i].defence_ = character.defence_;
                        enemyPlayerCharacters[i].strength_ = character.strength_;
                    }
                }
                enemyPlayerCharacters[i].Alive = true;
                enemyPlayerCharacters[i].ReturnToIdle();
            }
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
            txt.raycastTarget = false;
            txt.color = Color.white;
        }
        turnOrder_ = 0;
        StartGameLogic();
    }
    #endregion

    #region Events
    private void OnGameStarted(string _Sender, string _RoomId, string _NextTurn)        
    {
        Debug.Log("_Sender " + _Sender + ", _RoomId: " + _RoomId + ", _NextTurn: " + _NextTurn);
        unityObjects["SelectionUI"].SetActive(false);
        unityObjects["GameUI"].SetActive(true);
        InitPlayers(playerCharacters, playerSlots, GlobalVariables.finalSelection_);
        StartGameLogic();
        Dictionary<string, object> _toSend = new Dictionary<string, object>() {
            { "Character_1", playerCharacters[0].GetComponent<SC_Character>().name_ },
            { "Character_2", playerCharacters[1].GetComponent<SC_Character>().name_ },
            { "Character_3", playerCharacters[2].GetComponent<SC_Character>().name_ },
            { "Character_4", playerCharacters[3].GetComponent<SC_Character>().name_ },
        };
        _toJson = MiniJSON.Json.Serialize(_toSend);
        WarpClient.GetInstance().GetLiveRoomInfo(_RoomId);
        nextTurn = _NextTurn;
        startTime = Time.time;
        //My turn
        if (GlobalVariables.userId == _NextTurn)
        {
            isMyTurn = true;
            UpdateGameAction(true);
            unityObjects["Txt_TurnStatus"].GetComponent<TMP_Text>().text = "Choose your action";
            unityObjects["Txt_Loading_Title_dots"].SetActive(false);
        }
        else //Opponent turn
        {
            isMyTurn = false;
        }
    }

    private void OnGetLiveRoomInfo(LiveRoomInfoEvent eventObj)
    {
        Debug.Log("OnGetLiveRoomInfo ");
        if (eventObj != null && eventObj.getProperties() != null && _toJson != null)
        {
            foreach (string user in eventObj.getJoinedUsers())
            {
                WarpClient.GetInstance().sendPrivateChat(user, _toJson);

            }
        }
    }
    private void OnUserJoinRoom(RoomData eventObj, string _UserName)
    {
        Debug.Log("User Joined Room " + _UserName);
        unityObjects["SelectionUI"].SetActive(false);
        unityObjects["GameUI"].SetActive(true);

        InitPlayers(playerCharacters, playerSlots, GlobalVariables.finalSelection_);

        Dictionary<string, object> _toSend = new Dictionary<string, object>() {
            { "Character_1", playerCharacters[0].GetComponent<SC_Character>().name_ },
            { "Character_2", playerCharacters[1].GetComponent<SC_Character>().name_ },
            { "Character_3", playerCharacters[2].GetComponent<SC_Character>().name_ },
            { "Character_4", playerCharacters[3].GetComponent<SC_Character>().name_ },
        };
        _toJson = MiniJSON.Json.Serialize(_toSend);
        WarpClient.GetInstance().GetLiveRoomInfo(eventObj.getId());
    }


    private void OnMoveCompleted(MoveEvent _Move)
    {
        int delayIndex = 1;
        Debug.Log("OnMoveCompleted " + _Move.getNextTurn() + " " + _Move.getMoveData() + " " + _Move.getSender());
        if (_Move.getSender() != GlobalVariables.userId && _Move.getMoveData() != null)
        {
            delayIndex = SetMove(_Move.getMoveData());
        }

        if (_Move.getNextTurn() == GlobalVariables.userId)
        {
            isMyTurn = true;
            unityObjects["Txt_TurnStatus"].GetComponent<TMP_Text>().text = "Choose your action";
            unityObjects["Txt_Loading_Title_dots"].SetActive(false);
        }
        else
        {
            isMyTurn = false;
            turnOrder_ = 0;
            turnSwitch();
            unityObjects["Txt_TurnStatus"].GetComponent<TMP_Text>().text = "Enemy player turn";
            unityObjects["Txt_Loading_Title_dots"].SetActive(true);
            if (_toSendMove.Count > 0)
            {
                _toJson = MiniJSON.Json.Serialize(_toSendMove);
                WarpClient.GetInstance().GetLiveRoomInfo(GlobalVariables.roomId);
                _toSendMove.Clear();

            }
        }
        StartCoroutine(DelayUpdateGameAction(delayIndex));
    }

    private int SetMove(string _senderJson)     /* update the enemy player turn and informs you on their actions */
    {
        int delayIndex = 1;
        Dictionary<string, object> _data = (Dictionary<string, object>)MiniJSON.Json.Deserialize(_senderJson);
        if (_data.Count != 0)
        {

            for (int i = 1; i <= maxPlayers; i++, delayIndex += 3)
            {
                if (_data.ContainsKey("Type " + i.ToString()))
                {
                    string actionType = (string)_data["Type " + i.ToString()];
                    if (actionType != "Running" && actionType != "Running Failed")
                    {
                        int EnemyPlayerNum = int.Parse(_data["PlayerNum " + i.ToString()].ToString());
                        int TargetPlayerNum = int.Parse(_data["TargetNum " + i.ToString()].ToString());
                        if (actionType == "Attack")
                        {
                            string attackType = _data["AttackType " + i.ToString()].ToString();
                            if (attackType == "normalAttack")
                            {
                                int hitPoints = int.Parse(_data["Points " + i.ToString()].ToString());
                                StartCoroutine(OnMoveCompletedNormalAttack(hitPoints, EnemyPlayerNum, TargetPlayerNum, delayIndex));
                            }
                            else if (attackType == "magicAttack")
                            {
                                int hitPoints = int.Parse(_data["Points " + i.ToString()].ToString());
                                string spellName = _data["SpellName " + i.ToString()].ToString();
                                SC_Magic enemySpell = null;
                                foreach (SC_Magic spell in SC_MagicController.Allspells_)
                                {
                                    if (spell.name_ == spellName)
                                        enemySpell = spell;
                                }
                                StartCoroutine(OnMoveCompletedMagicAttack(hitPoints, EnemyPlayerNum, TargetPlayerNum, enemySpell, delayIndex));
                            }
                        }
                        else if (actionType == "Support")
                        {
                            string supportType = _data["SupportType " + i.ToString()].ToString();
                            string spellName = _data["SpellName " + i.ToString()].ToString();
                            SC_Magic enemySpell = null;
                            foreach (SC_Magic spell in SC_MagicController.Allspells_)
                            {
                                if (spell.name_ == spellName)
                                    enemySpell = spell;
                            }
                            if (supportType == "Healing")
                            {
                                int hitPoints = int.Parse(_data["Points " + i.ToString()].ToString());
                                int maxHp = int.Parse(_data["MaxHp " + i.ToString()].ToString());
                                StartCoroutine(OnMoveCompletedSupportHealing(hitPoints, maxHp, EnemyPlayerNum, TargetPlayerNum, enemySpell, delayIndex));

                            }
                            else if (supportType == "Resurrection")
                            {
                                int hitPoints = int.Parse(_data["Points " + i.ToString()].ToString());
                                StartCoroutine(OnMoveCompletedSupportResurrection(hitPoints, EnemyPlayerNum, TargetPlayerNum, enemySpell, delayIndex));
                            }
                            else if (supportType == "Buff")
                            {
                                StartCoroutine(OnMoveCompletedSupportBuff(EnemyPlayerNum, TargetPlayerNum, enemySpell, delayIndex));
                            }
                        }
                    }
                    else
                    {
                        if (actionType == "Running")
                        {
                            MultiplayerWinFinish();
                        }
                        else
                            delayIndex -= 3;
                    }
                }
                else
                    delayIndex -= 3;
            }
            CheckGameStatus();
        }
        if (unityObjects["battle_box_MagicList"].activeSelf == true)
            Back();
        return delayIndex;
    }


    private void OnPrivateChatReceived(string sender, string message)
    {
        Debug.Log("onPrivateChatReceived : " + sender);
        Debug.Log("message : " + message);
        if (GlobalVariables.userId != sender)
        {
            StartGameLogic();
            if (!startTurns)
            {
                startTurns = true;
                turnSwitch();
            }
            Dictionary<string, object> _data = (Dictionary<string, object>)MiniJSON.Json.Deserialize(message);
            if (_data.ContainsKey("Character_1"))
            {
                Dictionary<int, string> enemyNames = new Dictionary<int, string>()
            {
            {0,_data["Character_1"].ToString() },
            {1,_data["Character_2"].ToString() },
            {2,_data["Character_3"].ToString() },
            {3,_data["Character_4"].ToString() },
            };
                InitPlayers(enemyPlayerCharacters, enemyPlayerSlots, enemyNames);
                int count = 0;
                foreach (TMP_Text textComponent in monsterTexts_Box2)
                {
                    textComponent.text = enemyPlayerCharacters[count].name_;
                    count++;
                    if (count == enemyPlayerCharacters.Count)
                        break;
                }
            }
            else if (_data.ContainsKey("Type 1") || _data.ContainsKey("Type 2") || _data.ContainsKey("Type 3") || _data.ContainsKey("Type 4"))
            {
                SetMove(message);
            }
            else if (_data.ContainsKey("Restart"))
            {
                if (!(bool)_data["Restart"])
                {
                    unityObjects["Menu_Restart"].SetActive(true);
                }
                else
                {
                    StartCoroutine(RestartLogic());
                }
            }
        }
    }

    private void OnUserLeftRoom(RoomData eventObj, string username)
    {
        Debug.Log("onUserLeftRoom : " + username);
        MultiplayerWinFinish();
        StartCoroutine(DelayedGameExit());
    }

    private void MultiplayerWinFinish()     /* win functionality in multiplayer */
    {
        Over = true;
        isMyTurn = false;
        Debug.Log("Game is over");
        if (unityObjects["Finish_box"].activeSelf == false)
        {
            unityObjects["Finish_box"].SetActive(true);
            unityObjects["Finish_box"].GetComponentInChildren<TMP_Text>().text = "You Win";
        }
        UpdateGameAction(false);
        battleMusic_[2].Play();
        foreach (SC_Character player in playerCharacters)
            player.Win();
    }

    private IEnumerator OnMoveCompletedNormalAttack(int hitPoints, int EnemyPlayerNum, int TargetPlayerNum, int delayIndex)
    {
        yield return new WaitForSeconds(1f * delayIndex);
        enemyPlayerCharacters[EnemyPlayerNum].NormalAttack(playerCharacters[TargetPlayerNum], 1);
        playerCharacters[TargetPlayerNum].TakeDamage(hitPoints);
        OnMoveCompletedEndAttack();
    }

    private IEnumerator OnMoveCompletedMagicAttack(int hitPoints, int EnemyPlayerNum, int TargetPlayerNum, SC_Magic spell, int delayIndex)
    {
        yield return new WaitForSeconds(1f * delayIndex);
        enemyPlayerCharacters[EnemyPlayerNum].CastSpell(spell, playerCharacters[TargetPlayerNum]);
        playerCharacters[TargetPlayerNum].TakeDamage(hitPoints);
        OnMoveCompletedEndAttack();
    }

    private IEnumerator OnMoveCompletedSupportHealing(int hitPoints, int maxHp, int EnemyPlayerNum, int TargetPlayerNum, SC_Magic spell, int delayIndex)
    {
        yield return new WaitForSeconds(1f * delayIndex);
        enemyPlayerCharacters[EnemyPlayerNum].Heal(spell, enemyPlayerCharacters[TargetPlayerNum], hitPoints);
        if (enemyPlayerCharacters[EnemyPlayerNum].health_ > maxHp)
            enemyPlayerCharacters[EnemyPlayerNum].health_ = maxHp;
        OnMoveCompletedEndAttack();
    }

    private IEnumerator OnMoveCompletedSupportResurrection(int hitPoints, int EnemyPlayerNum, int TargetPlayerNum, SC_Magic spell, int delayIndex)
    {
        yield return new WaitForSeconds(1f * delayIndex);
        enemyPlayerCharacters[EnemyPlayerNum].Revive(spell, enemyPlayerCharacters[TargetPlayerNum], hitPoints);
        monsterTexts_Box2[TargetPlayerNum].color = Color.white;
        monsterTexts_Box2[TargetPlayerNum].raycastTarget = true;
        StartCoroutine(FadeCoroutine(2f, 1f, enemyPlayerCharacters[TargetPlayerNum].GetComponent<SpriteRenderer>()));
        OnMoveCompletedEndAttack();
    }

    private IEnumerator OnMoveCompletedSupportBuff(int EnemyPlayerNum, int TargetPlayerNum, SC_Magic spell, int delayIndex)
    {
        yield return new WaitForSeconds(1f * delayIndex);
        enemyPlayerCharacters[EnemyPlayerNum].Buff(spell, enemyPlayerCharacters[TargetPlayerNum]);
        OnMoveCompletedEndAttack();
    }

    private void OnMoveCompletedEndAttack()
    {
        int startBoxHP = 4;
        int endBoxHP = 8;
        for (int i = startBoxHP; i < endBoxHP; i++)
            UpdateTextValue(battleTexts_Box3[i], playerCharacters[i - 4].health_.ToString());
        CheckPlayersDeath();
    }

    private IEnumerator DelayUpdateGameAction(int delayIndex)       /* update the button functionality in play but with a delay */
    {
        yield return new WaitForSeconds(delayIndex);
        updateActivePlayer();
        while (!activePlayer.Alive && !Over)
        {
            turnSwitch();
            updateActivePlayer();
        }
        UpdateGameAction(isMyTurn);
    }

    #endregion
}


