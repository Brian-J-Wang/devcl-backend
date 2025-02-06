namespace DevCL.Exceptions;

public class DocumentAlreadyExistsException : Exception {
    public DocumentAlreadyExistsException(string message) : base(message) {}
}