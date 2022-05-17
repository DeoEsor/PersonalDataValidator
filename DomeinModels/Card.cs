using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;

namespace Models
{
    public  class Card : ICloneable, IDataErrorInfo
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
            BirthDay = new ValueIsValid<DateTime>(DateTime.Today);
        }

        public ValueIsValid<string> Name
        {
            get => name;
            set
            {
                name.Value = value.Value;
                name.State = ValidState.NotStated;
            }
        }

        public ValueIsValid<string> Surname
        {
            get => surname;
            set
            {
                surname.Value = value.Value;
                surname.State = ValidState.NotStated;
            }
        }

        public ValueIsValid<string> Patronymic
        {
            get => patronymic;
            set
            {
                patronymic.Value = value.Value;
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

        public string Error { get; }

        public string this[string columnName]
        {
            get
            {
                FieldInfo fi = typeof(Card).GetField(columnName);
                object fieldValue = fi.GetValue(this);
                
                if (fieldValue is ValueIsValid<string> validString)
                    if (validString.State == ValidState.NotStated)
                    {
                        if (validString.Error != String.Empty)
                            return Error;
                        
                        return String.Empty;
                        
                    }
                if (fieldValue is ValueIsValid<DateTime> validDate)
                    if (validDate.State == ValidState.NotStated)
                    {
                        if (validDate.Error != String.Empty)
                            return Error;
                        
                        return String.Empty;
                    }
                
                return "Value is not valid";
            }
        }
    }
}
