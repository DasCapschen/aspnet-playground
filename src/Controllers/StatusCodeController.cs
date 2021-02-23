using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace src.Controllers {
    public class HttpStatusController: Controller
    {
        [HttpGet("Error/{code}")]
        public IActionResult Index(HttpStatusCode code)
        {
            return View(code);
        }
    }
}
