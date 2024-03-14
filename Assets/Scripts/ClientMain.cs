using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using MortiseFrame.LitIO;

public class ClientMain : MonoBehaviour {

    [SerializeField] Text userName;
    [SerializeField] Text recvText;
    [SerializeField] Button loginButton;
    [SerializeField] Button connectButton;
    [SerializeField] Button closeButton;

    Queue<IMessage> messageQueue;

    byte[] data;
    Socket client;

    void Start() {
        data = new byte[256];
        loginButton.onClick.AddListener(SendLoginReq);
        connectButton.onClick.AddListener(ConnectToServer);
        closeButton.onClick.AddListener(CloseConnection);
        messageQueue = new Queue<IMessage>();
    }

    void Update() {
        if (client == null) {
            return;
        }
        TickOn();
        TickSend();
    }

    void TickOn() {
        if (client.Poll(0, SelectMode.SelectRead)) {
            byte[] data = new byte[1024];
            int count = client.Receive(data);
            int offset = 0;
            int msgCount = ByteReader.Read<int>(data, ref offset);

            for (int i = 0; i < msgCount; i++) {
                int len = ByteReader.Read<int>(data, ref offset);
                if (len < 5) {
                    break;
                }
                byte id = ByteReader.Read<byte>(data, ref offset);
                Debug.Log("Recv Message ID: " + id + ", Length: " + len);
                On(id, data, ref offset);
            }
        }
    }

    void TickSend() {
        byte[] data = new byte[1024];
        int offset = 0;
        int msgCount = messageQueue.Count;
        ByteWriter.Write<int>(data, msgCount, ref offset);
        while (messageQueue.TryDequeue(out IMessage message)) {
            byte[] src = message.ToBytes();
            int len = src.Length + 5;
            byte id = ProtocolDict.GetID(message);

            ByteWriter.Write<int>(data, len, ref offset);
            ByteWriter.Write<byte>(data, id, ref offset);
            ByteWriter.WriteArray<byte>(data, src, ref offset);
            Debug.Log("Send Message ID: " + id + ", Length: " + len + ",  Type: " + message.GetType());
        }
        if (offset > 0) {
            client.Send(data, 0, offset, SocketFlags.None);
        }
    }

    void On(int id, byte[] data, ref int offset) {
        switch (id) {
            case 102:
                Debug.Log("On ConnectResMessage");
                OnConnectRes(data, ref offset);
                break;
            case 106:
                Debug.Log("On LoginResMessage");
                OnLoginRes(data, ref offset);
                break;
            default:
                Debug.LogError("Unknown Message ID: " + id);
                break;
        }
    }

    void SendLoginReq() {
        LoginReqMessage loginReq = new LoginReqMessage();
        loginReq.userToken = userName.text;

        messageQueue.Enqueue(loginReq);
    }

    void OnConnectRes(byte[] data, ref int offset) {
        recvText.text = "Connected To Server Success!";
    }

    void OnLoginRes(byte[] data, ref int offset) {
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
        messageQueue.Enqueue(closeReq);
    }

    void CloseConnection() {
        SendCloseReq();
        client.Shutdown(SocketShutdown.Both);
        client.Close();
        recvText.text = "Connection Closed!";
    }

    void OnDestroy() {
        CloseConnection();
        loginButton.onClick.RemoveListener(SendLoginReq);
        connectButton.onClick.RemoveListener(ConnectToServer);
        closeButton.onClick.RemoveListener(CloseConnection);
    }

}