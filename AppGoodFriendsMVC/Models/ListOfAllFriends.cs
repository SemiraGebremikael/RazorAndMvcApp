using System;
using Models;

namespace AppGoodFriendsMVC.Models
{
	public class ListOfAllFriendsModel
	{
        public PaginatedFriendsViewModel PaginatedFriends { get; set; }

        public class PaginatedFriendsViewModel
        {
            public List<IFriend> Friends { get; set; }
            public int PageIndex { get; set; }
            public int TotalPages { get; set; }

            public bool HasPreviousPage => PageIndex > 1;
            public bool HasNextPage => PageIndex < TotalPages;

            public PaginatedFriendsViewModel(List<IFriend> items, int count, int pageIndex, int pageSize)
            {
                PageIndex = pageIndex;
                TotalPages = (int)Math.Ceiling(count / (double)pageSize);

                Friends = items.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
        }

    }
}

