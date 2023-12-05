using System;
namespace Models.DTO
{
	public class gstusrInfoDbDto
	{
        public int nrSeededFriends { get; set; } = 0;
        public int nrUnseededFriends { get; set; } = 0;
        public int nrFriendsWithAddress { get; set; } = 0;

        public int nrSeededAddresses { get; set; } = 0;
        public int nrUnseededAddresses { get; set; } = 0;

        public int nrSeededPets { get; set; } = 0;
        public int nrUnseededPets { get; set; } = 0;

        public int nrSeededQuotes { get; set; } = 0;
        public int nrUnseededQuotes { get; set; } = 0;
    }

    public class gstusrInfoFriendsDto
    {
        public string Country { get; set; } = null;
        public string City { get; set; } = null;
        public int NrFriends { get; set; } = 0;
    }

    public class gstusrInfoPetsDto
    {
        public string Country { get; set; } = null;
        public string City { get; set; } = null;
        public int NrPets { get; set; } = 0;
    }

    public class gstusrInfoQuotesDto
    {
        public string Author { get; set; } = null;
        public int NrQuotes { get; set; } = 0;
    }

    public class gstusrInfoAllDto
    {
        public gstusrInfoDbDto Db { get; set; } = null;
        public List<gstusrInfoFriendsDto> Friends { get; set; } = null;
        public List<gstusrInfoPetsDto> Pets { get; set; } = null;
        public List<gstusrInfoQuotesDto> Quotes { get; set; } = null;
    }
}

