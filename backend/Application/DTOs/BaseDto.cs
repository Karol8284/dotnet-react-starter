namespace Application.DTOs;

/// <summary>
/// Bazowy DTO (Data Transfer Object)
/// Wszystkie DTOs powinny dziedziczyć z tej klasy
/// </summary>
public abstract class BaseDto
{
    /// <summary>
    /// Unikalny identyfikator
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Data utworzenia
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data ostatniej modyfikacji
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
