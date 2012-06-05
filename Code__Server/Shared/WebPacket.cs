/*
 * UBERMEAT FOSS
 * ****************************************************************************************
 * License:                 Creative Commons Attribution-ShareAlike 3.0 unported
 *                          http://creativecommons.org/licenses/by-sa/3.0/
 * 
 * Project:                 Uber Media
 * File:                    /WebPacket.cs
 * Author(s):               limpygnome						limpygnome@gmail.com
 * To-do/bugs:              none
 * 
 * A class used for storing the response of a web-request to an Uber Media site.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace UberMediaServer
{
    public class WebPacket
    {
        #region "Enums"
        public enum ErrorCode
        {
            SUCCESS,
            ERROR
        }
        #endregion

        #region "Variables"
        public ErrorCode errorCode;
        public List<string> arguments = null;
        public string errorMessage;
        #endregion

        #region "Methods - Constructors"
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
        #endregion
    }
}