using System;

namespace DamnORM.Model.Exceptions
{
    [Serializable]
    public class MalformedSqlException : Exception
    {
        public MalformedSqlException(object source, string reason)
            : base(reason)
        {
            base.Data.Add("source", source);
        }

        public MalformedSqlException(object source, string reason, Exception inner)
            : base(reason, inner)
        {
            base.Data.Add("source", source);
        }

        protected MalformedSqlException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}