using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.DTO;
using Services;
using static RazorPages.Pages.Friends.ListOfAllFriendsModel;

namespace RazorPages.Pages.Friends
{
    public class ListOfAllFriendsModel : PageModel
    {
        IFriendsService _service = null;
        loginUserSessionDto _usr;


        public List<IFriend> Friends { get; set; } = new List<IFriend>();
        public List<IQuote> Quotes { get; set; } = new List<IQuote>();

        public List<IAddress> Addresses { get; set; } = new List<IAddress>();


        // Pagineringsrelaterade egenskaper
        public PaginatedFriendsViewModel PaginatedFriends { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 4; 


        public async Task<IActionResult> OnGetAsync(int? pageIndex)
        {
            PageIndex = pageIndex ?? 1;

            var allFriends = await _service.ReadFriendsAsync(_usr, true, false, "", 0, 100);

            // Create paginated list for friends
            PaginatedFriends = new PaginatedFriendsViewModel(allFriends, allFriends.Count, PageIndex, PageSize);

            return Page();
        }


        public ListOfAllFriendsModel(IFriendsService service) => _service = service;

        #region PaginatedList
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

        #endregion

    }
}


