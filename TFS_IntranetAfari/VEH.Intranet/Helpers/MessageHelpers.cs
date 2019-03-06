using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace VEH.Intranet.Helpers
{
    public enum MessageType { Warning, Info, Error, Success, AExist }

    public class FlashAction
    {
        public String Action { get; set; }
        public String Text { get; set; }
    }

    public class FlashMessage
    {
        public String Title { get; set; }
        public String Body { get; set; }
        public Boolean Inline { get; set; }
        public MessageType Type { get; set; }
        public List<FlashAction> Actions { get; set; }

        public FlashMessage()
        {
            Inline = true;
            Type = MessageType.Info;
            Actions = new List<FlashAction>();
        }
    }

    public static class MessageHelpers
    {
    }
}