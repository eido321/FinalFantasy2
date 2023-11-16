using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SC_PlayerMovement : MonoBehaviour
{
    #region Variables
    public bool _DisableEncounter;
    public float moveSpeed;
    public LayerMask overworldLayer;
    public GameObject _FadeSprite;
    public AudioSource _mainAudio;
    public AudioSource _encounterAudio;

    private Animator animator;
    private bool _Stop;
    private bool _Over;
    private Rigidbody2D rb;
    private bool isMoving;
    private Vector2 input;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        StartCoroutine(FadeCoroutine(2f, 0f, _FadeSprite.GetComponent<SpriteRenderer>()));
        _DisableEncounter= false;
        _Stop = false;
        _Over = false;
    }

    private void FixedUpdate()
    {
        if (!isMoving && !_Stop && !_Over)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input.x != 0) input.y = 0;

            if (input != Vector2.zero)
            {
                animator.SetBool("Up", false);
                animator.SetBool("Down", false);
                animator.SetBool("Sideways", false);

                if (input.x < 0)
                {
                    animator.SetBool("Sideways", true);
                    transform.localScale = new Vector3(1f, 1f, 1f);
                }else if(input.x > 0)
                {
                    animator.SetBool("Sideways", true);
                    transform.localScale = new Vector3(-1f, 1f, 1f);
                }
                if (input.y < 0)
                {
                    animator.SetBool("Down", true);
                }
                else if (input.y > 0)
                {
                    animator.SetBool("Up", true);
                }

                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                if (isWalkable(targetPos))
                    StartCoroutine(Move(targetPos));
            }
        }
        
        animator.SetBool("isMoving", isMoving);
        if (SC_OptionBattleLogic._Open)
        {
            _Stop = true;
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else
        {
            _Stop = false;
        }
    }
    #endregion

    #region Logic
    IEnumerator Move(Vector3 targetPos)         /* the character moving logic */
    {
        isMoving = true;
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;
        if(!_DisableEncounter)
            CheckForEncounters();
    }

    private bool isWalkable(Vector3 targetPos)      /* checks if the tile the character is going to be in is valid */
    {
        if (Physics2D.OverlapCircle(targetPos, 0.1f, overworldLayer) != null)
        {
            return false;
        }
        return true;
    }

    private void CheckForEncounters()       /* checks if the player encounterd a monster */
    {

        if (Random.Range(1, 101) <= 3)
        {
            _Over = true;
            _mainAudio.Stop();
            _encounterAudio.Play();
            StartCoroutine(FadeCoroutine(2f, 1f, _FadeSprite.GetComponent<SpriteRenderer>()));
            StartCoroutine(ChangeToBattle());
        }
    }
    public void SetEncounter(bool val)
    {
        _DisableEncounter = !val;
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
            elapsedTime += Time.fixedDeltaTime;
            yield return null;
        }
        spriteMaterial.color = targetColor;
    }

    private IEnumerator ChangeToBattle()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(1);
    }
    #endregion
}
