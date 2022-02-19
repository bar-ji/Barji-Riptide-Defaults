using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using RiptideNetworking;

public class GameUI : MonoBehaviour
{
    public static GameUI Singleton;

    void Awake()
    {
        if(Singleton)
        {
            Destroy(this);
        }
        Singleton = this;
        gameObject.SetActive(false);
    }

    public void DisablePauseMenu()
    {
        foreach(Transform t in transform)
        {
            t.gameObject.SetActive(false);
        }
    }

    public void EnablePauseMenu()
    {
        foreach(Transform t in transform)
        {
            t.gameObject.SetActive(true);
        }
    }
}
