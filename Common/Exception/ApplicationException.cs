using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace Common.Exception
{
    
    public class ApplicationException : System.Exception
    {

        public ApplicationException() : base()
        {
        }

        public ApplicationException(string Message, bool IsControlExceptionType = true) : base((IsControlExceptionType ? "{" + string.Format("Type:ApplicationException, Message:{0}", Message) + "}" : Message))
        {
        }

        public ApplicationException(string Message, System.Exception InnerException, bool IsControlExceptionType = true) : base((IsControlExceptionType ? "{" + string.Format("Type:ApplicationException, Message:{0}", Message) + "}" : Message), InnerException)
        {
        }
    }
}
