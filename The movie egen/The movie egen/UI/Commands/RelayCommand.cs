// System-navneområder (standard)
using System;                             // Exception, ArgumentNullException, EventHandler

// UI-komponenter
using System.Windows.Input;               // ICommand, CommandManager

namespace The_movie_egen.UI.Commands
{
    /// <summary>
    /// Minimal ICommand implementation til MVVM-kommandoer.
    /// - Kapsler Execute/CanExecute delegates
    /// - Understøtter CommandManager.RequerySuggested automatisk
    /// - Bruges til at binde knapper til ViewModel-metoder
    /// - Ingen parameter - til simple kommandoer
    /// </summary>
    public sealed class RelayCommand : ICommand
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Private felter (delegates)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Delegate der udfører kommandoen (kræves).
        /// </summary>
        private readonly Action _execute;

        /// <summary>
        /// Delegate der bestemmer om kommandoen kan udføres (valgfrit).
        /// </summary>
        private readonly Func<bool>? _canExecute;

        /// <summary>
        /// Ctor: initialiserer kommando med execute og valgfri canExecute delegate.
        /// </summary>
        /// <param name="execute">Delegate der udfører kommandoen</param>
        /// <param name="canExecute">Valgfri delegate der bestemmer om kommandoen kan udføres</param>
        /// <exception cref="ArgumentNullException">Hvis execute er null</exception>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  ICommand implementation
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Event der rejses når CanExecute-status ændres.
        /// Automatisk koblet til CommandManager.RequerySuggested.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Bestemmer om kommandoen kan udføres.
        /// Ignorerer parameter og kalder canExecute delegate hvis tilgængelig.
        /// </summary>
        /// <param name="parameter">Ignoreres (ingen parameter understøttet)</param>
        /// <returns>True hvis kommandoen kan udføres</returns>
        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        /// <summary>
        /// Udfører kommandoen.
        /// Ignorerer parameter og kalder execute delegate.
        /// </summary>
        /// <param name="parameter">Ignoreres (ingen parameter understøttet)</param>
        public void Execute(object? parameter) => _execute();
    }

    /// <summary>
    /// Generic ICommand implementation til MVVM-kommandoer med parameter.
    /// - Kapsler Execute/CanExecute delegates med type-sikker parameter
    /// - Understøtter CommandManager.RequerySuggested automatisk
    /// - Bruges til at binde knapper til ViewModel-metoder med parameter
    /// - Type-sikker parameter-håndtering
    /// </summary>
    /// <typeparam name="T">Type af parameter der sendes til kommandoen</typeparam>
    public sealed class RelayCommand<T> : ICommand
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  Private felter (delegates)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Delegate der udfører kommandoen med parameter (kræves).
        /// </summary>
        private readonly Action<T> _execute;

        /// <summary>
        /// Delegate der bestemmer om kommandoen kan udføres med parameter (valgfrit).
        /// </summary>
        private readonly Func<T, bool>? _canExecute;

        /// <summary>
        /// Ctor: initialiserer kommando med execute og valgfri canExecute delegate.
        /// </summary>
        /// <param name="execute">Delegate der udfører kommandoen med parameter</param>
        /// <param name="canExecute">Valgfri delegate der bestemmer om kommandoen kan udføres</param>
        /// <exception cref="ArgumentNullException">Hvis execute er null</exception>
        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  ICommand implementation
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Event der rejses når CanExecute-status ændres.
        /// Automatisk koblet til CommandManager.RequerySuggested.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Bestemmer om kommandoen kan udføres med den givne parameter.
        /// Cast parameter til T og kalder canExecute delegate hvis tilgængelig.
        /// </summary>
        /// <param name="parameter">Parameter der sendes til canExecute delegate</param>
        /// <returns>True hvis kommandoen kan udføres</returns>
        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T)parameter!) ?? true;

        /// <summary>
        /// Udfører kommandoen med den givne parameter.
        /// Cast parameter til T og kalder execute delegate.
        /// </summary>
        /// <param name="parameter">Parameter der sendes til execute delegate</param>
        public void Execute(object? parameter) => _execute((T)parameter!);
    }
}
