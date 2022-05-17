using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        private Validation.Client.Proto.MediatorClient _mediatorClient;

        private Lazy<ICommand> _addCommand;
        private Lazy<ICommand> _startValidationCommand;

        public ICommand AddCommand => _addCommand.Value;
        public ICommand AddPhoneCommand { get; set; }
        public ICommand AddEmailCommand { get; set; }
        public ICommand AddAddressCommand { get; set; }

        public ICommand DeletePhoneCommand { get; set; }
        public ICommand DeleteEmailCommand { get; set; }
        public ICommand DeleteAddressCommand { get; set; }
        public ICommand StartValidationCommand => _startValidationCommand.Value;

        public Card Card { get; set; }
        
        public string InputPhoneNumber { get; set; }
        public string InputEmail { get; set; }
        public string InputAddress { get; set; }

        public MainViewModel()
        {
            _mediatorClient = new MediatorClient(null);

            Card = new Card
            {
                Name =  (ValueIsValid<string>)"Вася",
                Surname = (ValueIsValid<string>)"Пупкин",
                Patronymic = (ValueIsValid<string>)"Петрович"
            };

            _addCommand = new Lazy<ICommand>(() => new RelayCommand(_ => Add()));
            _startValidationCommand = new Lazy<ICommand>(() => new RelayCommand(
                async _ => await StartValidationAsync()));
            AddPhoneCommand = new RelayCommand(AddPhone);
            AddEmailCommand = new RelayCommand(AddEmail);
            AddAddressCommand = new RelayCommand(AddAddress);
        }

        private void AddPhone(object _)
        {
            if (!string.IsNullOrEmpty(InputPhoneNumber))
                Card.PhoneNumber.Add((ValueIsValid<string>)InputPhoneNumber);
        }

        private void AddAddress(object _)
        {
            if (!string.IsNullOrEmpty(InputAddress))
                Card.Address.Add((ValueIsValid<string>)InputAddress);
        }

        private void AddEmail(object _)
        {
            if (!string.IsNullOrEmpty(InputEmail))
                Card.Emails.Add((ValueIsValid<string>)InputEmail);
        }

        private void Add() 
            => Cards.Add((Card)Card.Clone());

        private async Task StartValidationAsync()
        {
           var a = await _mediatorClient.ValidateCardsAsync(Cards);
        }
    }
}
