namespace staff_competencies_backend.Utils;

public class NotFoundException(string message) : Exception(message);
public class BadRequestException(string message) : Exception(message);
