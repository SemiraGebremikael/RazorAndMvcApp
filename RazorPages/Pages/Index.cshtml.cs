using Microsoft.AspNetCore.Mvc.RazorPages;
using Services;

namespace RazorPages.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IFriendsService _service;

    public IndexModel(ILogger<IndexModel> logger, IFriendsService service)
    {
        _logger = logger;
        _service = service;
    }

    public async Task OnGet()
    {
        var onget = await _service.InfoAsync;
    }
}

