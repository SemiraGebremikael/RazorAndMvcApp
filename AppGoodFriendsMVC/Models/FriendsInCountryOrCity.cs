using System;
using Models;

namespace AppGoodFriendsMVC.Models
{
	public class FriendsInCountryOrCityModel
    {
        public List<IAddress> Addresses { get; set; }
        public List<IFriend> Friends { get; set; }
        public string Country { get; set; }
    }
}

