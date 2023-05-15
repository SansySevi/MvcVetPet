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
        private ServiceStorageBlobs serviceBlob;

        public HomeController(ServiceApp service, ServiceStorageBlobs serviceBlob)
        {
            this.service = service;
            this.serviceBlob = serviceBlob;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Servicios()
        {
            List<Servicio> servicios = await this.service.GetServiciosAsync();

            List<BlobModel> listBlobs = new List<BlobModel>();

            foreach(Servicio servicio in servicios)
            {
                BlobModel blob = 
                    await this.serviceBlob.FindBlob("vetcareimages", servicio.Imagen);
                listBlobs.Add(blob);
            }
                
            ViewData["SERVICIOS"] = listBlobs;
            return View(servicios);
        }

    }
}