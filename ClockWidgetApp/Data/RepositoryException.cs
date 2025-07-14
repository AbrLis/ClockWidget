namespace ClockWidgetApp.Data
{
    using System;

    /// <summary>
    /// Исключение для ошибок работы репозитория
    /// </summary>
    public class RepositoryException : Exception
    {
        /// <summary>
        /// Создает новое исключение репозитория
        /// </summary>
        public RepositoryException() { }

        /// <summary>
        /// Создает новое исключение репозитория с сообщением
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        public RepositoryException(string message) : base(message) { }

        /// <summary>
        /// Создает новое исключение репозитория с сообщением и внутренним исключением
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        /// <param name="inner">Внутреннее исключение</param>
        public RepositoryException(string message, Exception inner) : base(message, inner) { }
    }
}