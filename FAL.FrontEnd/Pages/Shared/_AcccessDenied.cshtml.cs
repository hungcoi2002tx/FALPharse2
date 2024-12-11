using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FAL.FrontEnd.Pages.Shared
{
    public class _AcccessDeniedModel : PageModel
    {
        public void OnGet()
        {
        }
        public IActionResult OnGetDirect()
        {
            if (Helper.SessionExtensions.IsInRole(1))
            {
                return Redirect("/Admin");
            }
            return Redirect("/");
        }
    }
}
