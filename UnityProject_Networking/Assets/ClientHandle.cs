using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
   public static void Welcome(Packet _packet){
       string _msg = _packet.ReadString();
       int myId = _packet.ReadInt();

       Debug.Log($"Message From Server : {_msg}");
       Client.instance.myId = myId;

       //TODO SEND WELCOME RECEIVE PACKET 
       ClientSend.WelcomeReceived();
   }
}
