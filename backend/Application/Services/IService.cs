namespace Application.Services;

/// <summary>
/// Generyczny interfejs dla serwisów aplikacyjnych
/// Każdy serwis powinien dziedziczyć z tego interfejsu
/// </summary>
/// <typeparam name="TDto">DTO związane z encją</typeparam>
public interface IService<TDto> where TDto : class
{
    /// <summary>
    /// Pobierz element po Id
    /// </summary>
    Task<TDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pobierz wszystkie elementy
    /// </summary>
    Task<IEnumerable<TDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Utwórz nowy element
    /// </summary>
    Task<TDto> CreateAsync(TDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Zaktualizuj element
    /// </summary>
    Task<TDto> UpdateAsync(TDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Usuń element
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
