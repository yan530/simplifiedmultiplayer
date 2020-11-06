using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DrawCard : NetworkBehaviour
{
    public PlayerManager pm;

    public void OnClick()
    {
        NetworkIdentity netID = NetworkClient.connection.identity;
        pm = netID.GetComponent<PlayerManager>();
        pm.CmdDrawCardData();
    }
}
