using System;
using Models;

namespace AppGoodFriendsMVC.Models
{
	public class DetailsOfFriendModel
	{
        public IFriend Friend { get; set; }
        public IAddress Address { get; set; }
        public string ErrorMessage { get; set; }
      
    }
}

