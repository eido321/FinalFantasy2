using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Monster : SC_Entity
{
    #region MonoBehaviour
    private void Start()
    {
        if (GlobalVariables.gameState == GlobalVariables.GameState.SinglePlayer)
            GetComponent<SpriteRenderer>().sprite = sprite_;
    }
    #endregion

    #region
    public void DamageTaken()
    {
        StartCoroutine(WaitForAttack());
    }
    #endregion

    #region Coroutines
    private IEnumerator WaitForAttack()
    {
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(MoveLeftAndReturn());
    }
    private IEnumerator MoveLeftAndReturn()
    {
        Vector3 initialPosition = transform.position;
        Vector3 targetPosition = initialPosition + new Vector3(-0.2f, 0f, 0f);
        while (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(1f);
        while (transform.position != initialPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, initialPosition, Time.deltaTime);
            yield return null;
        }
    }
    #endregion
}
