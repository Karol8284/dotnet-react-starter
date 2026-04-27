namespace Domain.Interfaces;

/// <summary>
/// Generyczny interfejs repository dla operacji CRUD
/// Każde repository specjalizowane powinno dziedziczyć z tego interfejsu
/// </summary>
/// <typeparam name="T">Typ encji</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Pobierz encję po Id
    /// </summary>
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pobierz wszystkie encje
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Dodaj nową encję
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aktualizuj encję
    /// </summary>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Usuń encję
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
