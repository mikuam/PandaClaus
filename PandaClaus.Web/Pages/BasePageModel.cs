using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PandaClaus.Web.Pages;

public class BasePageModel : PageModel
{
    public bool IsAdmin => CheckIsAdmin();
    
    protected bool CheckIsAdmin()
    {
        var isAdminValue = HttpContext.Session.GetString("IsAdmin");
        return isAdminValue is not null && isAdminValue == "true";
    }
}