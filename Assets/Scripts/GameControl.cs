using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameControl : MonoBehaviour
{
    public static event Action HandlePulled = delegate { };

    [Header("UI Text Fields")]
    [SerializeField] private TextMeshProUGUI prizeText;
    [SerializeField] private GameObject prizeBackground;
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private TextMeshProUGUI payoutText;
    [SerializeField] private TextMeshProUGUI currentBetText;

    [Header("Game Configuration")]
    [SerializeField] private Row[] row;
    [SerializeField] private Transform handle;
    [SerializeField] private int startingBalance = 2000;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip spinSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip handlePullSound;

    [Header("Spin Audio Trim Settings")]
    [SerializeField] private float spinSoundEndTime = 2.5f;

    private AudioSource spinSource;
    private AudioSource winSource;
    private AudioSource handleSource;

    private int prizeValue;
    private int currentPlayerBalance;
    private int currentBetAmount = 200;

    // FIX 1: Prevent checking results right when clicking play
    private bool resultsChecked = true;
    private bool isAnimating = false;

    [SerializeField] private Animator handleAnimator;

    private void Awake()
    {
        spinSource = gameObject.AddComponent<AudioSource>();
        winSource = gameObject.AddComponent<AudioSource>();
        handleSource = gameObject.AddComponent<AudioSource>();

        spinSource.clip = spinSound;
        spinSource.playOnAwake = false;
        spinSource.loop = false;

        currentPlayerBalance = startingBalance;
        UpdateBalanceUI();
        UpdateBetUI();

        if (payoutText != null) payoutText.text = "Choose Bet to Spin";
    }

    private void Update()
    {
        // --- REELS ARE SPINNING ---
        if (!row[0].rowStopped || !row[1].rowStopped || !row[2].rowStopped)
        {
            prizeValue = 0;
            if (prizeBackground != null) prizeBackground.SetActive(false);

            if (spinSource != null && spinSource.clip != null)
            {
                if (!spinSource.isPlaying)
                {
                    spinSource.time = 0f;
                    spinSource.Play();
                }
                else if (spinSource.time >= spinSoundEndTime)
                {
                    spinSource.time = 0f;
                }
            }
        }

        // --- REELS HAVE STOPPED ---
        if (row[0].rowStopped && row[1].rowStopped && row[2].rowStopped)
        {
            if (spinSource != null && spinSource.isPlaying)
            {
                spinSource.Stop();
            }

            if (!resultsChecked)
            {
                CheckResults();

                if (prizeBackground != null) prizeBackground.SetActive(true);

                if (prizeValue > 0)
                {
                    int totalEarnings = prizeValue * (currentBetAmount / 100);
                    currentPlayerBalance += totalEarnings;

                    prizeText.text = "Prize: $" + totalEarnings;
                    if (payoutText != null) payoutText.text = "WIN: +$" + totalEarnings;

                    if (winSource != null && winSound != null)
                    {
                        winSource.PlayOneShot(winSound);
                    }
                }
                else
                {
                    prizeText.text = "Prize: $0";
                    if (payoutText != null) payoutText.text = "LOST -$" + currentBetAmount;
                }

                UpdateBalanceUI();
            }
        }

        // --- HANDLE PULL INPUT DETECTOR ---
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

            Collider2D hit = Physics2D.OverlapPoint(worldPos);

            if (hit != null && hit.transform == transform)
            {
                if (row[0].rowStopped && row[1].rowStopped && row[2].rowStopped)
                {
                    if (currentPlayerBalance >= currentBetAmount)
                    {
                        // Set this to false ONLY when a real spin starts!
                        resultsChecked = false;

                        currentPlayerBalance -= currentBetAmount;
                        UpdateBalanceUI();

                        if (payoutText != null) payoutText.text = "Spinning...";

                        if (handleSource != null && handlePullSound != null)
                        {
                            handleSource.PlayOneShot(handlePullSound);
                        }

                        isAnimating = true;
                        handleAnimator.SetTrigger("Pull");
                        StartCoroutine(WaitForAnimation());
                    }
                    else
                    {
                        if (payoutText != null) payoutText.text = "INSUFFICIENT BALANCE!";
                    }
                }
            }
        }
    }

    // ---Betting System---
    public void SelectBet50() { SetBet(50); }
    public void SelectBet100() { SetBet(100); }
    public void SelectBet150() { SetBet(150); }

    private void SetBet(int amount)
    {
        if (row[0].rowStopped && row[1].rowStopped && row[2].rowStopped)
        {
            currentBetAmount = amount;
            UpdateBetUI();
            Debug.Log("Bet switched to: $" + amount);
        }
    }

    private void UpdateBalanceUI()
    {
        if (balanceText != null) balanceText.text = "Balance: $" + currentPlayerBalance;
    }

    private void UpdateBetUI()
    {
        if (currentBetText != null) currentBetText.text = "Bet: $" + currentBetAmount;
    }

    private IEnumerator WaitForAnimation()
    {
        isAnimating = true;
        yield return null;
        while (!handleAnimator.GetCurrentAnimatorStateInfo(0).IsName("HandlePull")) yield return null;
        while (handleAnimator.GetCurrentAnimatorStateInfo(0).IsName("HandlePull")) yield return null;
        isAnimating = false;
        HandlePulled();
    }

    private static readonly Dictionary<string, int> triplePrizes = new Dictionary<string, int>{
        { "Bar",     200 },
        { "Seven",   150 },
        { "Cherry",  100 },
        { "Bell",    50  }
    };

    private static readonly Dictionary<string, int> doublePrizes = new Dictionary<string, int>{
        { "Bar",     50 },
        { "Seven",   40 },
        { "Cherry",  30 },
        { "Bell",    20 }
    };

    private void CheckResults()
    {
        string a = row[0].stoppedSlot;
        string b = row[1].stoppedSlot;
        string c = row[2].stoppedSlot;

        if (a == b && b == c && triplePrizes.TryGetValue(a, out int triplePrize))
        {
            prizeValue = triplePrize;
        }
        else
        {
            string matchedSymbol = (a == b) ? a : (a == c) ? a : (b == c) ? b : null;
            prizeValue = (matchedSymbol != null && doublePrizes.TryGetValue(matchedSymbol, out int doublePrize)) ? doublePrize : 0;
        }
        resultsChecked = true;
    }
}