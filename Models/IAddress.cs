using System;
namespace Models
{
	public interface IAddress 
    {
        public Guid AddressId { get; set; }

        public string StreetAddress { get; set; }
        public int ZipCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public List<IFriend> Friends { get; set; }
    }
}

