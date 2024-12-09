using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FAL.FrontEnd.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            if (Helper.SessionExtensions.IsInRole(1))
            {
                return RedirectToPage("/Admin/Index");
            }
            else
            {
                return RedirectToPage("/Dashboard/Main");
            }
        }
    }
}
