namespace ClockWidgetApp.Views
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using ClockWidgetApp.Helpers;

    /// <summary>
    /// Окно для выбора даты и времени длинного таймера.
    /// </summary>
    public partial class LongTimerInputWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Выбранная дата (год, месяц, день).
        /// </summary>
        public DateTime? SelectedDate { get => _selectedDate; set { _selectedDate = value; OnPropertyChanged(nameof(SelectedDate)); } }
        private DateTime? _selectedDate = DateTime.Now.Date;

        /// <summary>
        /// Введённые часы.
        /// </summary>
        public string SelectedHour { get => _selectedHour; set { _selectedHour = value; OnPropertyChanged(nameof(SelectedHour)); } }
        private string _selectedHour = "0";

        /// <summary>
        /// Введённые минуты.
        /// </summary>
        public string SelectedMinute { get => _selectedMinute; set { _selectedMinute = value; OnPropertyChanged(nameof(SelectedMinute)); } }
        private string _selectedMinute = "0";

        /// <summary>
        /// Введённые секунды.
        /// </summary>
        public string SelectedSecond { get => _selectedSecond; set { _selectedSecond = value; OnPropertyChanged(nameof(SelectedSecond)); } }
        private string _selectedSecond = "0";

        /// <summary>
        /// Итоговая выбранная дата и время.
        /// </summary>
        public DateTime SelectedDateTime
        {
            get
            {
                var date = SelectedDate ?? DateTime.Now.Date;
                int h = int.TryParse(SelectedHour, out var th) ? th : 0;
                int m = int.TryParse(SelectedMinute, out var tm) ? tm : 0;
                int s = int.TryParse(SelectedSecond, out var ts) ? ts : 0;
                return new DateTime(date.Year, date.Month, date.Day, h, m, s);
            }
        }

        private string _timerName = string.Empty;
        public string TimerName { get => _timerName; set { _timerName = value; OnPropertyChanged(nameof(TimerName)); } }

        private string _errorText = string.Empty;
        public string ErrorText { get => _errorText; set { _errorText = value; OnPropertyChanged(nameof(ErrorText)); } }
        private bool _errorVisible = false;
        public bool ErrorVisible { get => _errorVisible; set { _errorVisible = value; OnPropertyChanged(nameof(ErrorVisible)); } }
        private System.Windows.Threading.DispatcherTimer? _errorTimer;

        public LocalizedStrings Localized { get; } = LocalizationManager.GetLocalizedStrings();

        public LongTimerInputWindow()
        {
            InitializeComponent();
            _selectedDate = DateTime.Now.Date;
            _selectedHour = "0";
            _selectedMinute = "0";
            _selectedSecond = "0";
            OnPropertyChanged(nameof(SelectedDate));
            OnPropertyChanged(nameof(SelectedHour));
            OnPropertyChanged(nameof(SelectedMinute));
            OnPropertyChanged(nameof(SelectedSecond));
            DataContext = this;
        }

        /// <summary>
        /// Обработчик кнопки OK. Закрывает окно с результатом true.
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDateTime <= DateTime.Now)
            {
                ErrorText = Localized.LongTimerInput_ErrorInPast;
                ErrorVisible = true;
                _errorTimer?.Stop();
                _errorTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Helpers.Constants.LongTimerInputErrorDurationMs) };
                _errorTimer.Tick += (s, args) => { ErrorVisible = false; _errorTimer.Stop(); };
                _errorTimer.Start();
                return;
            }
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Корректирует значение времени при потере фокуса: если больше максимума — выставляет максимум, если меньше 0 или не число — 0.
        /// </summary>
        private void TimeBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb && tb.Tag is string tag)
            {
                int value = 0;
                int max = 0;
                switch (tag)
                {
                    case "Hour": max = 23; break;
                    case "Minute": max = 59; break;
                    case "Second": max = 59; break;
                    default: return;
                }
                if (!int.TryParse(tb.Text, out value) || value < 0)
                    value = 0;
                if (value > max)
                    value = max;
                tb.Text = value.ToString();
                switch (tag)
                {
                    case "Hour": SelectedHour = tb.Text; break;
                    case "Minute": SelectedMinute = tb.Text; break;
                    case "Second": SelectedSecond = tb.Text; break;
                }
            }
        }

        private void TimerNameBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Разрешаем только буквы, цифры и знаки препинания
            foreach (char c in e.Text)
            {
                if (!char.IsLetterOrDigit(c) && !char.IsPunctuation(c) && !char.IsWhiteSpace(c))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}