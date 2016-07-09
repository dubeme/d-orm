using System;

namespace DamnORM.Model.Exceptions
{
    [Serializable]
    public class MaxLengthException : Exception
    {
        private const string MESSAGE = "{0}, exceeds max length of {1} (Actual length: {2}).";

        public string Table { get; set; }
        public string Column { get; set; }
        public int MaxLength { get; set; }
        public int ActualLength { get; set; }
        public string FriendlyName { get; set; }

        public MaxLengthException(string friendlyName, int maxLength, int actualLength)
            : base(string.Format(MESSAGE, friendlyName, maxLength, actualLength))
        {
        }

        protected MaxLengthException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}