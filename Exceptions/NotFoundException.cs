using System.Reflection.Metadata;

namespace DevCL.Exceptions;

public class CollectionNotInitializedException : Exception {
    public CollectionNotInitializedException(string message) : base(message) {}
}

public class DocumentNotFoundException : Exception {
    public DocumentNotFoundException(string message) : base(message) {}
}

public class CategoryNotFoundException : Exception {
    public CategoryNotFoundException(string message) : base(message) {}
}

public class ItemNotFoundException : Exception {
    public ItemNotFoundException() : base("Item not found") {}
    public ItemNotFoundException(string message) : base(message) {}
}

public class MissingRequestFieldException : Exception {
    public MissingRequestFieldException(string missingField) : base ($"{missingField} was not found in the request body") {
        Console.WriteLine(this.Message);
    }
}