using System;
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
	public class DetailsOfFriendModel : PageModel
    {
        IFriendsService _service = null;
        loginUserSessionDto _usr;


        public IFriend Friend { get; set; }
        public IAddress Address { get; set; }

        public string ErrorMessage { get; set; } = null;

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                //Read a QueryParameter
                var idQuery = Request.Query["id"];
                if (string.IsNullOrEmpty(idQuery))
                {
                    ErrorMessage = "ID query parameter is missing.";
                    return Page();
                }

                Guid _id = Guid.Parse(idQuery);

                //Use the Service
                Friend = await _service.ReadFriendAsync(_usr, _id, false);
                if (Friend == null)
                {
                    ErrorMessage = "No friend found with the provided ID.";
                    return Page();
                }
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
            return Page();
        }
        public DetailsOfFriendModel(IFriendsService service) => _service = service;


    }
}


