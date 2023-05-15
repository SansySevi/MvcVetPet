using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using MvcVetPet.Services;
using MvcVetPet.Filters;
using System.Security.Claims;
using NugetVetPet.Models;
using MvcVetPet.Helpers;

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
        public async Task<IActionResult> Register(string nombre, string email, string username, string password, IFormFile file)
        {
            string blobName = file.FileName;
            using (Stream stream = file.OpenReadStream())
            {
                await this.serviceblob.UploadBlobAsync
                ("usuariosimages", blobName, stream);

            }

            await this.service.GetRegisterUserAsync(nombre, email, username, password, blobName);

            return RedirectToAction("Login");
        }

        #endregion


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
        public async Task<IActionResult> FAQs()
        {
            List<FAQ> faqs = await this.serviceApp.GetFaqsAsync();
            return View(faqs);
        }
    }
}
