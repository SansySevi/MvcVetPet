using Microsoft.AspNetCore.Mvc;
using MvcVetPet.Models;
using MvcVetPet.Services;
using NugetVetPet.Models;
using System.Diagnostics;

namespace MvcVetPet.Controllers
{
    public class HomeController : Controller
    {

        private ServiceApp service;

        public HomeController(ServiceApp service)
        {
            this.service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Servicios()
        {
            List<Servicio> servicios = await this.service.GetServiciosAsync();
            return View(servicios);
        }

    }
}