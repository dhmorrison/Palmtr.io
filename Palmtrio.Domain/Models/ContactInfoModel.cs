namespace Palmtrio.Domain.Models
{
    public class ContactInfoModel
    {
        public string FirstName { get; set; }
        public string MiddleInitial { get; set; }
        public string LastName { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }

        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        public string Message { get; set; }
    }
}
