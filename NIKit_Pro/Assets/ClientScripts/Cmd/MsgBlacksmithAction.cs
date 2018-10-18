using LPC;

public partial class Operation
{
    public class MsgBlacksmithAction : MsgHandler
    {
        public string GetName()
        {
            return "msg_blacksmith_action";
        }

        /// <summary>
        /// 装备熔炼
        /// </summary>
        public void Go(LPCValue para)
        {
        }
    }
}
