using Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Validation.Client.Proto;
using WPF.MVVM;
using WPF.MVVM.Commands;
namespace Validation.ViewModels
{
    public sealed class MainViewModel : ViewModelBase
    {
        private ObservableCollection<Card> cards = new ObservableCollection<Card>();

        public ObservableCollection<Card> Cards
        {
            get => cards;
        }

        private GreeterClient _greeterClient;

        private Lazy<ICommand> _addCommand;
        private Lazy<ICommand> _startValidationCommand;

        public ICommand AddCommand => _addCommand.Value;

        public ICommand StartValidationCommand => _startValidationCommand.Value;

        public Card Card { get; set; }

        public MainViewModel(/*GreeterClient greeterClient*/)
        {
            _greeterClient = new GreeterClient();

            Card = new Card
            {
                Name = "Вася",
                Surname = "Пупкин",
                Patronymic = "Петрович"
            };

            _addCommand = new Lazy<ICommand>(() => new RelayCommand(_ => Add()));
            _startValidationCommand = new Lazy<ICommand>(() => new RelayCommand(
                async _ => await StartValidationAsync()));
        }

        private void Add()
        {
            Cards.Add((Card)Card.Clone());
        }

        private async Task StartValidationAsync()
        {
            await _greeterClient.ValidateCardsAsync(Cards);
        }
    }
}
