namespace dawazonBackend.Cart.Exceptions;

public class CartException(string message) : Exception(message);

public class CartNotFoundException(string message) : CartException(message);
