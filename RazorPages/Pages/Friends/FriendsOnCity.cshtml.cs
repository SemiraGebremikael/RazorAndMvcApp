using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Models.DTO;
using Services;
using static Npgsql.PostgresTypes.PostgresCompositeType;

namespace RazorPages.Pages.Friends
{
    public class FriendsOnCityModel : PageModel
    {
        IFriendsService _service = null;
        loginUserSessionDto _usr;
        [BindProperty(SupportsGet = true)]
        public string City { get; set; }

        public List<IFriend> Friends { get; set; } = new List<IFriend>();
        public List<IAddress> Addresses { get; set; } = new List<IAddress>();

        public List<IFriend> FriendsInCity { get; set; } = new List<IFriend>();

        public Dictionary<Guid, int> NumberOfPets { get; set; } = new Dictionary<Guid, int>();
        public Dictionary<Guid, int> NumberOfQuotes { get; set; } = new Dictionary<Guid, int>();

        public async Task<IActionResult> OnGetAsync(string city)
        {
            City = city;
            if (string.IsNullOrEmpty(City))
            {
                return Page();
            }

            var addresses = await _service.ReadAddressesAsync(_usr, true, false, City, 0, 500);

            FriendsInCity = addresses.SelectMany(a => a.Friends).ToList();

            // Calculate the number of pets and quotes for each friend
            foreach (var friend in FriendsInCity)
            {
                var friendDetails = await _service.ReadFriendAsync(_usr, friend.FriendId, false);

                if (friendDetails != null)
                {
                    NumberOfPets[friend.FriendId] = friendDetails.Pets?.Count ?? 0;
                    NumberOfQuotes[friend.FriendId] = friendDetails.Quotes?.Count ?? 0;
                }
            }
            return Page();

        }

        public FriendsOnCityModel(IFriendsService service) => _service = service;

    }
}

