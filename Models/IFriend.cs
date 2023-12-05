using System;
namespace Models
{
	public interface IFriend
	{
        public Guid FriendId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        public IAddress Address { get; set; }
        public DateTime? Birthday { get; set; }

        public List<IPet> Pets { get; set; }
        public List<IQuote> Quotes { get; set; }
    }
}

