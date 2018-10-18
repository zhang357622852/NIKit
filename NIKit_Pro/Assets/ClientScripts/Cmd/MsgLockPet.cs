/// <summary>
/// MsgLockPet.cs
/// Created by fengsc 2016/11/07
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class MsgLockPet : MsgHandler
{
    public string GetName()
    {
        return "msg_lock_pet";
    }

    public void Go(LPCValue para)
    {
    }
}
