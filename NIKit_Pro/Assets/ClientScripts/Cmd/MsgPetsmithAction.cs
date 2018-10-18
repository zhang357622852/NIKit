using LPC;

public partial class Operation
{
    public class MsgPetsmithAction : MsgHandler
    {
        public string GetName()
        {
            return "msg_petsmith_action";
        }

        /// <summary>
        /// 宠物工坊
        /// </summary>
        public void Go(LPCValue para)
        {
        }
    }
}
