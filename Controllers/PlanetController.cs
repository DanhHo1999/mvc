using _06_MvcWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace _06_MvcWeb.Controllers
{
    public class PlanetController:Controller
    {
        private readonly PlanetService _planetService;
        private readonly ILogger<PlanetController> _logger;
        public PlanetController(PlanetService planetService, ILogger<PlanetController> logger)
        {
            _planetService = planetService;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
