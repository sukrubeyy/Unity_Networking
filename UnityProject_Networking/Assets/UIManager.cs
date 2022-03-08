using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
   public  InputField _userName;
   public GameObject _menu;
   public static UIManager instance;

   private void Awake(){
       if(instance==null)
        {
            instance=this;
        }
        else if(instance!=this){
            Destroy(this);
        }
   }

   public void ConnectToServer()
    {
        _menu.SetActive(false);
        _userName.interactable = false;
        Client.instance.ConnectToServer();
    }
   
}
