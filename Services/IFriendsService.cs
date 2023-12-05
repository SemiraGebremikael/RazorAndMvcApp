using System;
using Models;
using Models.DTO;

namespace Services
{
    public interface IFriendsService
	{
        //To test the overall layered structure
        public string InstanceHello { get; }

        //For inital test only, so a limited set on non-async methods in this example
        public gstusrInfoAllDto Info { get; }
        public adminInfoDbDto RemoveSeed(loginUserSessionDto usr, bool seeded);
        public adminInfoDbDto Seed(loginUserSessionDto usr, int nrOfItems);

        public List<IFriend> ReadFriends(loginUserSessionDto usr, bool seeded, bool flat, string filter, int pageNumber, int pageSize);


        //Full set of async methods
        public Task<gstusrInfoAllDto> InfoAsync { get; }
        public Task<adminInfoDbDto> SeedAsync(loginUserSessionDto usr, int nrOfItems);
        public Task<adminInfoDbDto> RemoveSeedAsync(loginUserSessionDto usr, bool seeded);

        public Task<List<IFriend>> ReadFriendsAsync(loginUserSessionDto usr, bool seeded, bool flat, string filter, int pageNumber, int pageSize);
        public Task<IFriend> ReadFriendAsync(loginUserSessionDto usr, Guid id, bool flat);
        public Task<IFriend> DeleteFriendAsync(loginUserSessionDto usr, Guid id);
        public Task<IFriend> UpdateFriendAsync(loginUserSessionDto usr, csFriendCUdto item);
        public Task<IFriend> CreateFriendAsync(loginUserSessionDto usr, csFriendCUdto item);

        public Task<List<IAddress>> ReadAddressesAsync(loginUserSessionDto usr, bool seeded, bool flat, string filter, int pageNumber, int pageSize);
        public Task<IAddress> ReadAddressAsync(loginUserSessionDto usr, Guid id, bool flat);
        public Task<IAddress> DeleteAddressAsync(loginUserSessionDto usr, Guid id);
        public Task<IAddress> UpdateAddressAsync(loginUserSessionDto usr, csAddressCUdto item);
        public Task<IAddress> CreateAddressAsync(loginUserSessionDto usr, csAddressCUdto item);

        public Task<List<IQuote>> ReadQuotesAsync(loginUserSessionDto usr, bool seeded, bool flat, string filter, int pageNumber, int pageSize);
        public Task<IQuote> ReadQuoteAsync(loginUserSessionDto usr, Guid id, bool flat);
        public Task<IQuote> DeleteQuoteAsync(loginUserSessionDto usr, Guid id);
        public Task<IQuote> UpdateQuoteAsync(loginUserSessionDto usr, csQuoteCUdto item);
        public Task<IQuote> CreateQuoteAsync(loginUserSessionDto usr, csQuoteCUdto item);

        public Task<List<IPet>> ReadPetsAsync(loginUserSessionDto usr, bool seeded, bool flat, string filter, int pageNumber, int pageSize);
        public Task<IPet> ReadPetAsync(loginUserSessionDto usr, Guid id, bool flat);
        public Task<IPet> DeletePetAsync(loginUserSessionDto usr, Guid id);
        public Task<IPet> UpdatePetAsync(loginUserSessionDto usr, csPetCUdto item);
        public Task<IPet> CreatePetAsync(loginUserSessionDto usr, csPetCUdto item);
    }
}

