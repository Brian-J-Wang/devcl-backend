[System.Serializable]
public class UserNotFoundExceptionException : Exception
{
    public UserNotFoundExceptionException() { }
    public UserNotFoundExceptionException(string message) : base(message) { }
    public UserNotFoundExceptionException(string message, System.Exception inner) : base(message, inner) { }
}