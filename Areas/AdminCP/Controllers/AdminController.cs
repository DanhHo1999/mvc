using _06_MvcWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _06_MvcWeb.AdminCP.Controllers
{
    [Area("AdminCP")]
    [Authorize(Roles =RoleName.Administrator)]
    [Route("/admincp")]
    public class AdminController:Controller
    {
        public IActionResult Index() => View();
    }
}
