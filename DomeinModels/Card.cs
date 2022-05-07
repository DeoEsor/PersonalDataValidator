using System.Collections.ObjectModel;

namespace Models
{
    public  class Card : ICloneable
    {
        private ValueIsValid<string> name = new ValueIsValid<string>(""), surname = new ValueIsValid<string>(""), patronymic = new ValueIsValid<string>("");
        
        private ObservableCollection<ValueIsValid<string>> emails;
        private ObservableCollection<ValueIsValid<string>> phoneNumber;
        private ObservableCollection<ValueIsValid<string>> address;
        private ValueIsValid<DateTime> birthDay;

        public Card()
        {
            Emails = new ObservableCollection<ValueIsValid<string>>();
            PhoneNumber = new ObservableCollection<ValueIsValid<string>>();
            Address = new ObservableCollection<ValueIsValid<string>>();
        }

        public string Name
        {
            get => name.Value;
            set
            {
                name.Value = value;
                name.State = ValidState.NotStated;
            }
        }

        public ValidState NameValid
        {
            get => name.State;
            set => name.State = value;
        }
        public ValidState SurnameValid
        {
            get => surname.State;
            set => surname.State = value;
        }
        public ValidState PatronymicValid
        {
            get => patronymic.State;
            set => patronymic.State = value;
        }

        public string Surname
        {
            get => surname.Value;
            set
            {
                surname.Value = value;
                surname.State = ValidState.NotStated;
            }
        }

        public string Patronymic
        {
            get => patronymic.Value;
            set
            {
                patronymic.Value = value;
                patronymic.State = ValidState.NotStated;
            }
        }

        public ObservableCollection<ValueIsValid<string>> PhoneNumber
        {
            get => phoneNumber;
            set => phoneNumber = value;
        }

        public ObservableCollection<ValueIsValid<string>> Emails
        {
            get => emails;
            set => emails = value;
        }
        public ObservableCollection<ValueIsValid<string>> Address
        {
            get => address;
            set => address = value;
        }
        public ValueIsValid<DateTime> BirthDay
        {
            get => birthDay;
            set => birthDay = value;
        }

        public object Clone()
            => new Card()
            {
                Name = Name,
                Surname = Surname,
                Patronymic = Patronymic,
                PhoneNumber = new ObservableCollection<ValueIsValid<string>>(PhoneNumber.ToList()),
                Emails = new ObservableCollection<ValueIsValid<string>>( Emails.ToList()),
                Address = new ObservableCollection<ValueIsValid<string>>(Address.ToList()),
                BirthDay = BirthDay
            };
    }
}
