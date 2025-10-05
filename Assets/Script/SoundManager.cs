using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    [SerializeField] private Transform placeSfxPre;
    [SerializeField] private Transform winSfxPre;
    [SerializeField] private Transform loseSfxPre;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.OnPlaceObject += OnPlacedObject;
        GameManager.Instance.OnGameWin += OnGameWin;
    }

    private void OnGameWin(object sender, GameManager.OnGameWinArgs e)
    {
        if (e.playerType == GameManager.Instance.GetLocalPlayerType())
        {
            GameObject winSfx = Instantiate(winSfxPre, transform.position, Quaternion.identity).gameObject;
            Destroy(winSfx);
        }
        else
        {
            GameObject loseSfx = Instantiate(loseSfxPre, transform.position, Quaternion.identity).gameObject;
            Destroy(loseSfx);
        }

    }

    private void OnPlacedObject(object sender, EventArgs e)
    {
        GameObject placeSfx = Instantiate(placeSfxPre, transform.position, Quaternion.identity).gameObject;
        Destroy(placeSfx);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
