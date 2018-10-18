using UnityEngine;
using System.Collections;
using LPC;

public class MsgReceiveScoreBonus : MsgHandler
{
    public string GetName()
    {
        return "msg_receive_score_bonus";
    }

    public void Go(LPCValue para)
    {
    }
}
