using System;
using System.Collections.Generic;
using System.Text;

namespace UberMediaServer
{
    public class WebPacket
    {
        public enum ErrorCode
        {
            SUCCESS,
            ERROR
        }
        public ErrorCode errorCode;
        public List<string> arguments = null;
        public string errorMessage;
        public WebPacket(string data)
        {
            foreach (string str in data.Split(':'))
            {
                if (arguments == null)
                {
                    arguments = new List<string>();
                    errorCode = str == "SUCCESS" ? ErrorCode.SUCCESS : ErrorCode.ERROR;
                }
                else if(str.Length > 0)
                    arguments.Add(str);
            }
            if (errorCode == ErrorCode.ERROR && arguments.Count > 0)
                errorMessage = arguments[0];
            else
                errorMessage = "Malformed data: '" + data + "'!";
        }
    }
}
