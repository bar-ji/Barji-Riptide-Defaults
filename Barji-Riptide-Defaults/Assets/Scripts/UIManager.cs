using RiptideNetworking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _singleton;
    public static UIManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(UIManager)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject gameMenu;
    [SerializeField] private TMP_InputField roomIdField;
    [SerializeField] private TMP_Text roomIdDisplayField;
    [SerializeField] private GameObject menuCamera;

    private void Awake()
    {
        Singleton = this;
    }

    #region CreateLobby

    public void HostClicked()
    {
        mainMenu.SetActive(false);
        menuCamera.SetActive(false);
        LobbyManager.Singleton.CreateLobby();
    }

    internal void LobbyCreationFailed()
    {
        mainMenu.SetActive(true);
    }

    internal void LobbyCreationSucceeded(ulong lobbyId)
    {
        roomIdDisplayField.text = lobbyId.ToString();
        roomIdDisplayField.gameObject.SetActive(true);
        gameMenu.SetActive(true);
    }

    #endregion

    #region JoinLobby

    public void JoinClicked()
    {
        if (string.IsNullOrEmpty(roomIdField.text))
        {
            Debug.LogWarning("A room ID is required to join!");
            return;
        }

        LobbyManager.Singleton.JoinLobby(ulong.Parse(roomIdField.text));
    }
    #endregion

    #region LobbyActive

    internal void LobbyEntered()
    {
        roomIdDisplayField.gameObject.SetActive(false);
        gameMenu.SetActive(true);
        mainMenu.SetActive(false);
        menuCamera.SetActive(false);
    }

    public void CopyRoomCode()
    {
        GUIUtility.systemCopyBuffer = roomIdDisplayField.text;
    }

    #endregion


    #region LeaveLobby

    public void LeaveClicked()
    {   
        if(NetworkManager.Singleton.Client.Id == 1)
            LobbyManager.Singleton.LeaveLobby();
        else
            LobbyManager.DoDisconnect(LobbyManager.Singleton.lobbyId);
        BackToMain();
    }

    internal void BackToMain()
    {
        menuCamera.SetActive(true);
        mainMenu.SetActive(true);
        gameMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    #endregion

    internal void UpdateUIVisibility()
    {
        if (Cursor.lockState == CursorLockMode.None)
            gameMenu.SetActive(true);
        else
            gameMenu.SetActive(false);
    }

    private void OnApplicationQuit() => LeaveClicked();
}