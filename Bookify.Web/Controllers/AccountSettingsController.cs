
namespace Bookify.Web.Controllers
{

	[Authorize]
	public class AccountSettingsController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}


	
	}
}
