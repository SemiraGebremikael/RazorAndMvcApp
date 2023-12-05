using System;
namespace Models.DTO
{
    public class adminInfoDbDto
    {
        public int nrSeededFriends { get; set; } = 0;
        public int nrUnseededFriends { get; set; } = 0;

        public int nrSeededAddresses { get; set; } = 0;
        public int nrUnseededAddresses { get; set; } = 0;

        public int nrSeededPets { get; set; } = 0;
        public int nrUnseededPets { get; set; } = 0;

        public int nrSeededQuotes { get; set; } = 0;
        public int nrUnseededQuotes { get; set; } = 0;
    }
}

