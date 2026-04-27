namespace Domain.Entities;

/// <summary>
/// Bazowa klasa dla wszystkich encji
/// Każda encja w systemie powinna dziedziczyć z tej klasy
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unikalny identyfikator encji
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Data utworzenia encji
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data ostatniej modyfikacji
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Czy encja została usunięta (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
