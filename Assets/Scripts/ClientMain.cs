using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class ClientMain : MonoBehaviour {

    void Start() {
        ConnectToServer();
    }

    void ConnectToServer() {
        try {
            // Create a TCP/IP socket.
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));

            // Receive data from the server
            byte[] data = new byte[256];
            int bytes = client.Receive(data);
            string responseData = Encoding.ASCII.GetString(data, 0, bytes);
            Debug.Log("Received: " + responseData);

            // Close the socket
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        } catch (SocketException e) {
            Debug.Log("SocketException: " + e.ToString());
        }
    }
}