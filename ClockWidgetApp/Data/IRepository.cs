// ВНИМАНИЕ: Данный интерфейс IRepository<T> на данный момент не используется в проекте.
// Оставлен для возможного будущего расширения (например, если потребуется универсальный репозиторий для коллекций).

namespace ClockWidgetApp.Data
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Базовый интерфейс репозитория для работы с данными.
    /// <para>ВНИМАНИЕ: На данный момент не используется в проекте. Оставлен для возможного будущего расширения.</para>
    /// </summary>
    /// <typeparam name="T">Тип сущности</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>Получить все элементы</summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>Получить элемент по идентификатору</summary>
        Task<T?> GetByIdAsync(string id);

        /// <summary>Добавить новый элемент</summary>
        Task AddAsync(T entity);

        /// <summary>Обновить существующий элемент</summary>
        Task UpdateAsync(T entity);

        /// <summary>Удалить элемент</summary>
        Task DeleteAsync(string id);

        /// <summary>Сохранить изменения</summary>
        Task SaveChangesAsync();
    }
}