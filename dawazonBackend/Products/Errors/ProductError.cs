namespace dawazonBackend.Products.Errors;

public record ProductError (string Message)
{
    public string Message { get; set; } = Message;
}

public record ProductNotFoundError (string Message) : ProductError (Message);

public record ProductValidationError (string Message) : ProductError (Message);

public record ProductBadRequestError (string Message) : ProductError (Message);

public record ProductConflictError (string Message) : ProductError (Message);

public record ProductStorageError (string Message) : ProductError (Message);