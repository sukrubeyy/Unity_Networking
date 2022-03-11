using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

public class ClientHandle : MonoBehaviour
{
   public static void Welcome(Packet _packet){


       string _msg = _packet.ReadString();
       int myId = _packet.ReadInt();

       Debug.Log($"Message From Server : {_msg}");
       Client.instance.myId = myId;

       //TODO SEND WELCOME RECEIVE PACKET 
       ClientSend.WelcomeReceived();

        //UDP
       Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
   }

   public static void UDPTest(Packet packet)
    {
        string _msg = packet.ReadString();
        Debug.Log($"Received packet via UDP. Contains message : {_msg}");
        ClientSend.UDPTestReceived();
    }
}
