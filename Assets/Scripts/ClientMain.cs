using UnityEngine;
using System.Net.Sockets;
using System.Text;

public class ClientMain : MonoBehaviour {

    void Start() {
        ConnectToServer();
    }

    void ConnectToServer() {
        try {
            System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient("127.0.0.1", 8080);
            NetworkStream stream = client.GetStream();

            // Receive data from the server
            byte[] data = new byte[256];
            int bytes = stream.Read(data, 0, data.Length);
            string responseData = Encoding.ASCII.GetString(data, 0, bytes);
            Debug.Log("Received: " + responseData);

            // Close everything
            stream.Close();
            client.Close();
        } catch (SocketException e) {
            Debug.Log("SocketException: " + e.ToString());
        }

    }

}