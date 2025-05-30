using System;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Text resultText;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loseColor;
    [SerializeField] private Button rematchButton;

    private void Awake()
    {
        rematchButton.onClick.AddListener(() =>
        {
            GameManager.Instance.RematchRpc();
        });
    }
    private void Start()
    {
        Hide();
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnRematch += GameManager_OnRematch;
        GameManager.Instance.OnGameTie += GameManager_OnGameTie;
    }

    private void GameManager_OnGameTie(object sender, EventArgs e)
    {
        resultText.text = "Tie";
        resultText.color = Color.white;
        Show();
    }

    private void GameManager_OnRematch(object sender, EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinArgs e)
    {
        if (e.playerType == GameManager.Instance.GetLocalPlayerType())
        {
            resultText.text = "YouWin!";
            resultText.color = winColor;
            Show();
        }
        else
        {
            resultText.text = "You Lose!";
            resultText.color = loseColor;
            Show();
        }

    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
