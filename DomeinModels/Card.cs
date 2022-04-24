namespace Models
{
    public class Card : ICloneable
    {
        private string name, surname, patronymic;
        private List<string> emails;
        private List<string> phoneNumber;
        private List<string> address;
        private DateTime birthDay;

        public Card()
        {
            Emails = new List<string>();
            PhoneNumber = new List<string>();
            Address = new List<string>();
        }

        public string Name
        {
            get => name;
            set => name = value;
        }

        public string Surname
        {
            get => surname;
            set => surname = value;
        }

        public string Patronymic
        {
            get => patronymic;
            set => patronymic = value;
        }

        public List<string> PhoneNumber
        {
            get => phoneNumber;
            private set => phoneNumber = value;
        }

        public List<string> Emails
        {
            get => emails;
            private set => emails = value;
        }
        public List<string> Address
        {
            get => address;
            private set => address = value;
        }
        public DateTime BirthDay
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
                PhoneNumber = PhoneNumber.ToList(),
                Emails = Emails.ToList(),
                Address = Address.ToList(),
                BirthDay = BirthDay
            };
    }
}
