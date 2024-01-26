using System;
using Models;
using Services;

namespace AppGoodFriendsMVC.Models
{
    public class FriendsByCountryModel
    {
        internal IFriendsService _service;

        public List<CountryFriendCount> FriendsByCountry { get; set; } = new List<CountryFriendCount>();
        public List<IFriend> Friends { get; internal set; }
    }

    public class CountryFriendCount
    {
        public string Country { get; set; }
        public int FriendCount { get; set; }
    }

  

}

