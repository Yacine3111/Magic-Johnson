namespace Magik_Johnson.CustomException;

using System;

public class RecordNotFoundException : Exception
{
    public RecordNotFoundException() { }

    public RecordNotFoundException(string msg)
        : base(msg) { }

    public RecordNotFoundException(string msg, Exception innerException)
        : base(msg, innerException) { }
}
