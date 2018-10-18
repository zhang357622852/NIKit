using LPC;

public class MsgExpressDetails : MsgHandler
{
    public string GetName() { return "msg_express_details"; }

    public void Go(LPCValue para)
    {
#if false
        if (MailUI.instance != null)
        {
            MailUI.instance.GetMailDetails(para);
        }
#endif
    }
}