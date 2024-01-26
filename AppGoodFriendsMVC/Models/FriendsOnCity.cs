using System;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace AppGoodFriendsMVC.Models
{
	public class FriendsOnCityModel
	{
        [BindProperty(SupportsGet = true)]
        public string City { get; set; }

        public List<IFriend> Friends { get; set; } = new List<IFriend>();
        public List<IAddress> Addresses { get; set; } = new List<IAddress>();

        public List<IFriend> FriendsInCity { get; set; } = new List<IFriend>();

        public Dictionary<Guid, int> NumberOfPets { get; set; } = new Dictionary<Guid, int>();
        public Dictionary<Guid, int> NumberOfQuotes { get; set; } = new Dictionary<Guid, int>();


    }
}

