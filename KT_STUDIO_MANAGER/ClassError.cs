using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KT_STUDIO_MANAGER
{
    class ClassError
    {
        public string ErrMsg = "";
        public void BuildErrMsg(string errMsg)
        {
            ErrMsg = ErrMsg + ", " + errMsg;
        }
        public string FormatedErrMsg
        {
            get { return ErrMsg.Substring(2, ErrMsg.Length - 2); }
        }
    }
}
