using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace Common.Exception
{
    public class DatabaseException : System.Exception
    {

        public DatabaseException() : base()
        {
        }

        public DatabaseException(string Message, bool IsControlExceptionType = true) : base((IsControlExceptionType ? "{" + string.Format("Type:DatabaseException, Message:{0}", Message) + "}" : Message))
        {            
        }

        public DatabaseException(string Message, System.Exception InnerException, bool IsControlExceptionType = true) : base((IsControlExceptionType ? "{" + string.Format("Type:DatabaseException, Message:{0}", Message) + "}" : Message), InnerException)
        {            
        }

    }
}



