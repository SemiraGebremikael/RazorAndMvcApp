using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Models.DTO;
using Services;

namespace RazorPages.Pages.Friends
{
    public class FriendsInCountryOrCityModel : PageModel
    {
        private readonly IFriendsService _service;
        public List<IAddress> Addresses { get; set; } = new List<IAddress>();
        public List<IFriend> Friends { get; set; } = new List<IFriend>();
        loginUserSessionDto _usr;

        public Dictionary<string, int> FriendsInCity { get; set; } = new Dictionary<string, int>();

        public async Task<IActionResult> OnGetAsync(string country)
        {
            if (string.IsNullOrEmpty(country))
            {
                return RedirectToPage("/Error");
            }

            Addresses = await _service.ReadAddressesAsync(_usr, true, false, country, 0, 500);
            Friends = await _service.ReadFriendsAsync(_usr, true, false, "", 0, 500);

            return Page();
        }

        public FriendsInCountryOrCityModel(IFriendsService service) => _service = service;
       
    }
}
