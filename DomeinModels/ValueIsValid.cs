using System.ComponentModel;
using System.Runtime.CompilerServices;
using Models.Annotations;

namespace Models;

    public sealed class ValueIsValid<T> : INotifyPropertyChanged, IDataErrorInfo
    {
        private ValidState state = ValidState.NotStated;
        public ValueIsValid(T value, ValidState state = ValidState.NotStated)
        {
            Value = value;
            State = state;
        }

        public T Value { get; set; }

        public ValidState State
        {
            get => state;
            set
            {
                state = value;
                OnPropertyChanged();
            }
        }
        
        public string Comment { get; set; } = String.Empty;

        public static explicit operator ValueIsValid<T>(T value)
        {
            return new ValueIsValid<T>(value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Error
        {
            get => Comment;
        }

        public string this[string columnName]
        {
            get
            {
                if (Error != String.Empty)
                    return Error;

                if (State == ValidState.NotStated)
                    return String.Empty;
                
                return "Value is not valid";
            }
        }
    }

    public enum ValidState
    {
        True,
        False,
        NotStated
    }