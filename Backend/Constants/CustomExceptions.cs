public class AccessDeniedException(string? message) : Exception(message) { }

public class NotFoundException(string message) : Exception(message) { }

public class NonUniqueNameException(string? message) : Exception(message) { }


// I tried to create a custom error with: public class NotFoundException(string message) : Exception(message) { }
// But that doesn't handle
