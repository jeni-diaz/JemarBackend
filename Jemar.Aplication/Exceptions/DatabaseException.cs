using System;
using System.Collections.Generic;
using System.Text;

namespace Jemar.Aplication.Exceptions
{
    public class DatabaseException: Exception
    {
        public DatabaseException(string message, Exception inner) : base(message, inner) { }
    }
}
