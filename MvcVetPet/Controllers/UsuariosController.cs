using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using MvcVetPet.Services;
using MvcVetPet.Filters;
using System.Security.Claims;
using NugetVetPet.Models;
using MvcVetPet.Helpers;
using Newtonsoft.Json.Linq;

namespace MvcVetPet.Controllers
{
    public class UsuariosController : Controller
    {

        private ServiceUsuarios service;
        private ServiceApp serviceApp;
        private ServiceStorageBlobs serviceblob;
        private HelperClaims helperClaims;

        public UsuariosController(ServiceUsuarios service, ServiceApp serviceApp, 
            ServiceStorageBlobs serviceblob, HelperClaims helperClaims)
        {
            this.service = service;
            this.serviceApp = serviceApp;
            this.serviceblob = serviceblob;
            this.helperClaims = helperClaims;
        }

        [AuthorizeUsuarios]
        public async Task<IActionResult> Home()
        {
            string token =
                HttpContext.Session.GetString("TOKEN");
            Usuario usuario = await
                this.service.GetPerfilUsuarioAsync(token);

            if(token != null)
            {
                BlobModel blobPerfil = await this.serviceblob.FindBlobPerfil("usuariosimages", usuario.Imagen, usuario.Nombre);
                ViewData["IMAGEN_PERFIL"] = blobPerfil;
            } else
            {
                return RedirectToAction("Logout","Usuarios");
            }
            
            
            return View();
        }

        #region MANAGED

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            string token = await this.service.GetTokenAsync(username, password);

            if (token == null)
            {
                ViewData["MENSAJE"] = "Usuario/Password incorrectos";
            }
            else
            {
                HttpContext.Session.SetString("TOKEN", token);
                Usuario usuario = await
                    this.service.GetPerfilUsuarioAsync(token);
                this.helperClaims.GetClaims(usuario);

                return RedirectToAction("Home", "Usuarios");
            }

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("TOKEN");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string apodo, string email, string password, IFormFile file)
        {
            string blobName = file.FileName;
            using (Stream stream = file.OpenReadStream())
            {
                await this.serviceblob.UploadBlobAsync
                ("usuariosimages", blobName, stream);

            }

            await this.service.GetRegisterUserAsync(apodo, email, password, blobName);

            return RedirectToAction("Login");
        }

        #endregion


        #region USUARIOS

        [AuthorizeUsuarios]
        public async Task<IActionResult> UserZone()
        {
            string token =
                HttpContext.Session.GetString("TOKEN");
            Usuario usuario = await
                this.service.GetPerfilUsuarioAsync(token);
            BlobModel blobPerfil = await this.serviceblob.FindBlobPerfil("usuariosimages", usuario.Imagen, usuario.Nombre);
            ViewData["IMAGEN_PERFIL"] = blobPerfil;

            List<Mascota> mascotas = await this.service.GetMascotas(token);
            return View(mascotas);
        }

        [AuthorizeUsuarios]
        public async Task<IActionResult> Calendar(int idusuario)
        {
            string token =
                HttpContext.Session.GetString("TOKEN");
            Usuario usuario = await
                this.service.GetPerfilUsuarioAsync(token);

            List<Evento> eventos = await this.service.GetEventos(token);

            ViewData["EVENTOS"] = HelperJson.SerializeObject<List<Evento>>(eventos);
            return View();

        }

        #endregion

        [AuthorizeUsuarios]
        public async Task<IActionResult> FAQs()
        {
            List<FAQ> faqs = await this.serviceApp.GetFaqsAsync();
            return View(faqs);
        }


        #region MASCOTAS

        [AuthorizeUsuarios]
        public async Task<IActionResult> Tratamientos()
        {
            string token =
                HttpContext.Session.GetString("TOKEN");

            List<Tratamiento> tratamientos = await this.service.GetTratamientos(token);
            return View(tratamientos);
        }

        [AuthorizeUsuarios]
        public async Task<IActionResult> Vacunas(int? pagina)
        {
            int elementosPorPagina = 4; 
            int paginaActual = pagina ?? 1; 


            string token =
                HttpContext.Session.GetString("TOKEN");

            List<Vacuna> vacunas = await this.service.GetVacunas(token);

            int totalElementos = vacunas.Count;
            int totalPaginas = (int)Math.Ceiling((double)totalElementos / elementosPorPagina);
            paginaActual = paginaActual < 1 ? 1 : paginaActual;
            paginaActual = paginaActual > totalPaginas ? totalPaginas : paginaActual;

            // Obtener los elementos para la página actual
            int indiceInicio = (paginaActual - 1) * elementosPorPagina;
            List<Vacuna> elementosPaginaActual = vacunas.Skip(indiceInicio).Take(elementosPorPagina).ToList();

            ViewData["Elementos"] = elementosPaginaActual;
            ViewData["PaginaActual"] = paginaActual;
            ViewData["TotalPaginas"] = totalPaginas;


            return View();
        }

        [AuthorizeUsuarios]
        public async Task<IActionResult> Pruebas()
        {
            string token =
                HttpContext.Session.GetString("TOKEN");

            List<Prueba> pruebas = await this.service.GetPruebas(token);

            return View(pruebas);
        }

        [AuthorizeUsuarios]
        public async Task<IActionResult> HistorialVeterinario(int? pagina)
        {
            int elementosPorPagina = 3;
            int paginaActual = pagina ?? 1;


            string token =
                HttpContext.Session.GetString("TOKEN");

            List<Procedimiento> procedimientos = await this.service.GetProcedimientos(token);

            int totalElementos = procedimientos.Count;
            int totalPaginas = (int)Math.Ceiling((double)totalElementos / elementosPorPagina);
            paginaActual = paginaActual < 1 ? 1 : paginaActual;
            paginaActual = paginaActual > totalPaginas ? totalPaginas : paginaActual;

            // Obtener los elementos para la página actual
            int indiceInicio = (paginaActual - 1) * elementosPorPagina;
            List<Procedimiento> elementosPaginaActual = procedimientos.Skip(indiceInicio).Take(elementosPorPagina).ToList();

            ViewData["Elementos"] = elementosPaginaActual;
            ViewData["PaginaActual"] = paginaActual;
            ViewData["TotalPaginas"] = totalPaginas;


            return View();
        }

        #endregion

    }
}
