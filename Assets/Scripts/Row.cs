using UnityEngine;
using System.Collections;

public class Row : MonoBehaviour
{
    private int randomValue;
    private float timeInterval;

    public bool rowStopped;
    public string stoppedSlot;

    private void Start()
    {
        rowStopped = true;
        GameControl.HandlePulled += StartRotating;
    }

    private void StartRotating()
    {
        StopCoroutine("Rotate");
        stoppedSlot = "_";
        StartCoroutine("Rotate");
    }

    private IEnumerator Rotate()
    {
        rowStopped = false;
        timeInterval = 0.025f;

        // --- FIRST LOOP ---
        for (int i = 0; i < 32; i++)
        {
            if (transform.position.y <= -1f)
            {
                transform.position = new Vector2(transform.position.x, 3f); // Fixed: Reset to 3f
            }
            else
            {
                transform.position = new Vector2(transform.position.x, transform.position.y - 0.25f);
            }

            yield return new WaitForSeconds(timeInterval);
        }

        randomValue = Random.Range(60, 100);

        switch (randomValue % 4)
        {
            case 1: randomValue += 3; break;
            case 2: randomValue += 2; break;
            case 3: randomValue += 1; break;
        }

        // --- SECOND LOOP ---
        for (int i = 0; i < randomValue; i++)
        {
            if (transform.position.y <= -1f)
            {
                transform.position = new Vector2(transform.position.x, 3f); // Keeps consistent with 3f
            }
            else
            {
                transform.position = new Vector2(transform.position.x, transform.position.y - 0.25f);
            }

            if (i > Mathf.RoundToInt(randomValue * 0.25f)) timeInterval = 0.05f;
            if (i > Mathf.RoundToInt(randomValue * 0.5f)) timeInterval = 0.1f;
            if (i > Mathf.RoundToInt(randomValue * 0.75f)) timeInterval = 0.15f;
            if (i > Mathf.RoundToInt(randomValue * 0.95f)) timeInterval = 0.2f;

            yield return new WaitForSeconds(timeInterval);
        }

        // --- FIX: SNAP TO NEAREST INTEGER ---
        // This rounds values like 1.75 or 1.99 to clean grid values (like 2.0)
        float roundedY = Mathf.Round(transform.position.y);
        transform.position = new Vector2(transform.position.x, roundedY);

        // Check using the cleanly snapped integer value
        if (Mathf.Approximately(roundedY, -1f))
            stoppedSlot = "Bar";
        else if (Mathf.Approximately(roundedY, 0f))
            stoppedSlot = "Cherry";
        else if (Mathf.Approximately(roundedY, 1f))
            stoppedSlot = "Seven";
        else if (Mathf.Approximately(roundedY, 2f))
            stoppedSlot = "Bell";
        else if (Mathf.Approximately(roundedY, 3f))
            stoppedSlot = "Bar";
        else
            Debug.LogWarning($"Row landed on unexpected position: {roundedY}");

        rowStopped = true;
    }

    private void OnDestroy()
    {
        GameControl.HandlePulled -= StartRotating;
    }
}