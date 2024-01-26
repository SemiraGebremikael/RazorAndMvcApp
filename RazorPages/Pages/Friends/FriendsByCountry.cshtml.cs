using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Net;
using Azure;
using DbModels;
using DbRepos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Models;
using Models.DTO;
using RazorPages.Pages.Friends;
using Services;
using static Npgsql.PostgresTypes.PostgresCompositeType;

namespace RazorPages.Pages.Friends
{
    public class FriendsByCountryModel : PageModel
    {
        IFriendsService _service;
        public List<IFriend> Friends { get; set; } = new List<IFriend>();
        public List<IAddress> Addresses { get; set; } = new List<IAddress>();
        loginUserSessionDto _usr;

        public List<CountryFriendCount> FriendsByCountry { get; set; } = new List<CountryFriendCount>();

        public async Task<IActionResult> OnGetAsync(string country)
        {
            Friends = await _service.ReadFriendsAsync(_usr, true, false, "", 0, 500);

            // Gruppera vänner efter land och räkna antalet vänner i varje land
            FriendsByCountry = Friends
                .Where(f => f.Address != null && !string.IsNullOrEmpty(f.Address.Country))
                .GroupBy(f => f.Address.Country)
                .Select(g => new CountryFriendCount
                {
                    Country = g.Key,
                    FriendCount = g.Count()
                })
                .ToList();

            return Page();

        }
        public FriendsByCountryModel(IFriendsService service) => _service = service;

        public class CountryFriendCount
        {
            public required string Country { get; set; }
            public int FriendCount { get; set; }
        }
    }
}