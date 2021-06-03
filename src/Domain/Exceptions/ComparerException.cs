using System;
using Domain.Models;

namespace Domain.Exceptions
{
    public class ComparerException : Exception
    {
        public ComparerException()
                : base()
        {
        }

        public ComparerException(string message)
            : base(message)
        {
        }
        public ComparerException(ResponseType responseType)
          : base($"Message: {(Enum.GetName(typeof(ResponseType), responseType)).Replace("_", " ")}")
        {
        }
    }
}
