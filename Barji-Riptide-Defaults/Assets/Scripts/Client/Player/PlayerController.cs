using RiptideNetworking;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private ClientPlayer player;
    [SerializeField] private float speed;
    [SerializeField] private Rigidbody rb;

    float xInput, yInput;

    void Awake()
    {
        player = GetComponent<ClientPlayer>();
    }

    void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        rb.AddForce(((transform.forward * yInput) + (transform.right * xInput)) * speed);
        SendPosition();
    }

    #region Sending
    private void SendPosition()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ClientToServer.playerPosition);
        message.AddVector3(transform.position);
        message.AddVector3(transform.forward);
        NetworkManager.Singleton.Client.Send(message);
    }
    #endregion

    #region Receiving

    #endregion
}
