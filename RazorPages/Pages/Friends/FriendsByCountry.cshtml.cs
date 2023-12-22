using DbModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Models;
using RazorPages.Pages.Friends;
using Services;

namespace RazorPages.Pages.Friends
{
    public class FriendsByCountryModel : PageModel
    {
        IFriendsService _friendsService = null;

        ILogger<FriendsByCountryModel> _logger = null;

        public List<csFriend> Friend { get; set; } = new List<csFriend>();

        public void OnGet()
        {
            var uri = Request.Path;

            //Use the Service
            Friends = _friendsService.ReadFriendsAsync();
            return Page();
        }
    }

    public class FriendsByCountryModel(IFriendsService service, ILogger<FriendsByCountryModel> logger)
    {
        _logger = logger;
        _friendsService = service;
    }
}

