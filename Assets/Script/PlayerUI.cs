using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject crossArrowGameObject;
    [SerializeField] private GameObject circleArrowGameObject;
    [SerializeField] private GameObject crossYouTextGameObject;
    [SerializeField] private GameObject circleYouTextGameObject;
    [SerializeField] private Text crossScoreText;
    [SerializeField] private Text circleScoreText;

    private void Awake()
    {
        crossArrowGameObject.SetActive(false);
        circleArrowGameObject.SetActive(false);
        crossYouTextGameObject.SetActive(false);
        circleYouTextGameObject.SetActive(false);
        crossScoreText.text = "";
        circleScoreText.text = "";
    }

    private void Start()
    {
        GameManager.Instance.OnGameStart += GameManager_OnGameStart;
        GameManager.Instance.OnCurrentPlayablePlayerChanged += (sender, e) =>
        {
            UpdateCurrentArrow();
        };
        GameManager.Instance.OnScoreChanged += GameManager_OnScoreChanged;

    }

    private void GameManager_OnScoreChanged(object sender, EventArgs e)
    {
        GameManager.Instance.GetScore(out int crossScore, out int circleScore);
        crossScoreText.text = crossScore.ToString();
        circleScoreText.text = circleScore.ToString();
    }

    private void GameManager_OnGameStart(object sender, EventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.Cross)
        {
            crossYouTextGameObject.SetActive(true);

        }
        else
        {
            circleYouTextGameObject.SetActive(true);
        }
        UpdateCurrentArrow();
    }

    private void UpdateCurrentArrow()
    {
        if (GameManager.Instance.GetCurrentPlayablePlayerType() == GameManager.PlayerType.Cross)
        {
            crossArrowGameObject.SetActive(true);
            circleArrowGameObject.SetActive(false);
        }
        else
        {
            crossArrowGameObject.SetActive(false);
            circleArrowGameObject.SetActive(true);
        }
    }
}
