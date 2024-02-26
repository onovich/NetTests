using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine.UI;

public class ClientMain : MonoBehaviour {

    [SerializeField] Text userName;
    [SerializeField] Text recvText;
    [SerializeField] Button loginButton;
    [SerializeField] Button connectButton;
    [SerializeField] Button closeButton;

    byte[] data;
    Socket client;

    void Start() {
        data = new byte[256];
        loginButton.onClick.AddListener(SendLoginReq);
        connectButton.onClick.AddListener(ConnectToServer);
        closeButton.onClick.AddListener(CloseConnection);
    }

    void Update() {
        if (client == null) {
            return;
        }
        if (client.Poll(0, SelectMode.SelectRead)) {
            byte[] data = new byte[1024];
            int count = client.Receive(data);
            int id = data[0];
            switch (id) {
                case 100:
                    Debug.Log("ConnectResMessage");
                    OnConnectRes();
                    break;
                case 2:
                    OnLoginRes(data);
                    break;
                case 0:
                    break;
                default:
                    Debug.LogError("Unknown Message ID: " + id);
                    break;
            }
        }
    }

    void SendLoginReq() {
        LoginReqMessage loginReq = new LoginReqMessage();
        loginReq.id = 1;
        loginReq.userToken = userName.text;
        byte[] bytes = loginReq.ToBytes();
        client.Send(bytes);
        recvText.text = "Wait For Login Rec...";
    }

    void OnConnectRes() {
        recvText.text = "Connected To Server Success!";
    }

    void OnLoginRes(byte[] data) {
        int offset = 0;
        LoginResMessage loginRes = new LoginResMessage();
        loginRes.FromBytes(data, ref offset);
        recvText.text = "STATUS: " + loginRes.status + ", TOKEN: " + loginRes.userToken;
    }

    void ConnectToServer() {
        try {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));
        } catch (SocketException e) {
            Debug.Log("SocketException: " + e.ToString());
        }
    }

    void SendCloseReq() {
        CloseReqMessage closeReq = new CloseReqMessage();
        closeReq.id = -1;
        byte[] bytes = closeReq.ToBytes();
        client.Send(bytes);
    }

    void CloseConnection() {
        SendCloseReq();
        // client.Shutdown(SocketShutdown.Both);
        // client.Close();
        recvText.text = "Connection Closed!";
    }

    void OnDestroy() {
        CloseConnection();
        loginButton.onClick.RemoveListener(SendLoginReq);
        connectButton.onClick.RemoveListener(ConnectToServer);
        closeButton.onClick.RemoveListener(CloseConnection);
    }

}