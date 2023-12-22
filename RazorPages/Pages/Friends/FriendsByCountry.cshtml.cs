using DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Models;
using RazorPages.Pages.Friends;
using Services;

namespace RazorPages.Pages.Friends
{
    public class FriendsByCountryModel : PageModel
    {
        IFriendsService _service = null;

        ILogger<FriendsByCountryModel> _logger = null;

        public List<csFriend> Friend { get; set; } = new List<csFriend>();

        public IActionResult OnGet()
        {
            var uri = Request.Path;

            //Use the Service
            _friendsService = _service.ReadFriendsAsync();
            return Page();
        }
    }

    public FriendsByCountryModel(IFriendsService service, ILogger<FriendsByCountryModel> logger)
    {
        _logger = logger;
        _service = service;
    }

    
}

