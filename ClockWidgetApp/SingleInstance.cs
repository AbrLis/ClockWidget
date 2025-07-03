namespace ClockWidgetApp
{
    /// <summary>
    /// Класс для контроля единственного экземпляра приложения через глобальный Mutex.
    /// </summary>
    public static class SingleInstance
    {
        private static Mutex? _mutex;
        private const string MutexName = "Global\\ClockWidgetApp_SingleInstance_Mutex";

        /// <summary>
        /// Пытается создать Mutex. Возвращает true, если это первый экземпляр.
        /// </summary>
        public static bool Start()
        {
            bool createdNew;
            _mutex = new Mutex(true, MutexName, out createdNew);
            if (!createdNew)
            {
                _mutex = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Освобождает Mutex при завершении приложения.
        /// </summary>
        public static void Stop()
        {
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
                _mutex = null;
            }
        }
    }
} 