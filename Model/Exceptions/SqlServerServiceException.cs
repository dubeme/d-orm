using System;

namespace DamnORM.Model.Exceptions
{
    [Serializable]
    public class SqlServerServiceException : Exception
    {
        public SqlServerServiceException()
        {
        }

        public SqlServerServiceException(string message) : base(message)
        {
        }

        public SqlServerServiceException(string message, Exception inner) : base(message, inner)
        {
        }

        protected SqlServerServiceException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}