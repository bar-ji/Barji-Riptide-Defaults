using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using RiptideNetworking;

public class VisualMessageManager : MonoBehaviour
{
    public static VisualMessageManager Singleton;
    [SerializeField] private TMP_Text errorText;

    void Awake()
    {
        if(Singleton) Destroy(this);
        Singleton = this;

        Singleton.errorText.color = new Color(255, 255, 255, 0);
    }

    public static void DisplayVisualMessage(string message, float duration = 2)
    {
        Singleton.errorText.color = new Color(255, 255, 255, 0);
        Singleton.StopAllCoroutines();
        Singleton.StartCoroutine(Singleton.Delay(message, duration));
    }

    IEnumerator Delay(string message, float duration)
    {
        Singleton.errorText.DOFade(1, duration / 10).SetEase(Ease.InSine);
        errorText.text = message;
        yield return new WaitForSeconds(duration);
        Singleton.errorText.DOFade(0, duration / 10).SetEase(Ease.OutSine);
    }

    [MessageHandler((ushort)ClientToServer.visualMessage)]
    private static void ReceiveVisualMessage(Message message)
    {
        DisplayVisualMessage(message.GetString(), message.GetFloat());
    }

    public static void SendNetworkVisualMessageAll(string _message, float duration)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServer.visualMessage);
        message.Add(_message);
        message.Add(duration);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
}
