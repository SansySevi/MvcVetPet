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
                BlobModel blobPerfil = await this.serviceblob.FindBlobPrivado("usuariosimages", usuario.Imagen, usuario.Apodo);
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

                BlobModel blobPerfil = await this.serviceblob.FindBlobPrivado("usuariosimages", usuario.Imagen, usuario.Apodo);

                this.helperClaims.GetClaims(usuario, blobPerfil.Url);

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
            BlobModel blobPerfil = await this.serviceblob.FindBlobPrivado("usuariosimages", usuario.Imagen, usuario.Apodo);
            ViewData["IMAGEN_PERFIL"] = blobPerfil;

            List<Mascota> mascotas = await this.service.GetMascotas(token);

            List<BlobModel> listBlobs = new List<BlobModel>();
            foreach (Mascota mascota in mascotas)
            {
                BlobModel blob =
                    await this.serviceblob.FindBlobPrivado("mascotasimages", mascota.Imagen, usuario.Apodo);
                if(blob != null)
                {
                    listBlobs.Add(blob);
                }
            }

            ViewData["MASCOTAS"] = listBlobs;
            return View(mascotas);
        }

        [AuthorizeUsuarios]
        [HttpPost]
        public async Task<IActionResult> UserZone(string nombre, string apodo,
            string email, string telefono, IFormFile? fichero)
        {

            string token =
                HttpContext.Session.GetString("TOKEN");
            Usuario usuario = await
                this.service.GetPerfilUsuarioAsync(token);

            if (fichero != null)
            {
                await this.serviceblob.DeleteBlobAsync("usuariosimages", usuario.Imagen);

                string fileName = fichero.FileName;
                using (Stream stream = fichero.OpenReadStream())
                {
                    await this.serviceblob.UploadBlobAsync
                    ("usuariosimages", fileName, stream);

                }

                Usuario user = await this.service.UpdateUsuario(usuario.IdUsuario, nombre, apodo,
                    email, telefono, fileName, token);
                BlobModel blobPerfil = await this.serviceblob.FindBlobPrivado("usuariosimages", user.Imagen, user.Apodo);

                this.helperClaims.GetClaims(user, blobPerfil.Url);


                ViewData["MENSAJE"] = "CAMBIOS EFECTUADOS CORRECTAMENTE SE VERAN LA PROXIMA VEZ QUE INICIE";
                List<Mascota> mascotas = await this.service.GetMascotas(token);
                List<BlobModel> listBlobs = new List<BlobModel>();
                foreach (Mascota mascota in mascotas)
                {
                    BlobModel blob =
                        await this.serviceblob.FindBlobPrivado("mascotasimages", mascota.Imagen, usuario.Apodo);
                    if (blob != null)
                    {
                        listBlobs.Add(blob);
                    }
                }

                ViewData["MASCOTAS"] = listBlobs;

                ViewData["IMAGEN_PERFIL"] = blobPerfil;
                return View(mascotas);
            }
            else
            {
                Usuario user = await this.service.UpdateUsuario(usuario.IdUsuario, nombre, apodo,
                    email, telefono, usuario.Imagen, token);

                BlobModel blobPerfil = await this.serviceblob.FindBlobPrivado("usuariosimages", user.Imagen, user.Apodo);

                this.helperClaims.GetClaims(user, blobPerfil.Url);

                ViewData["MENSAJE"] = "CAMBIOS EFECTUADOS CORRECTAMENTE SE VERAN LA PROXIMA VEZ QUE INICIE";
                List<Mascota> mascotas = await this.service.GetMascotas(token);
                List<BlobModel> listBlobs = new List<BlobModel>();
                foreach (Mascota mascota in mascotas)
                {
                    BlobModel blob =
                        await this.serviceblob.FindBlobPrivado("mascotasimages", mascota.Imagen, usuario.Apodo);
                    if (blob != null)
                    {
                        listBlobs.Add(blob);
                    }
                }

                ViewData["MASCOTAS"] = listBlobs;

                ViewData["IMAGEN_PERFIL"] = blobPerfil;
                return View(mascotas);
            }

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

        [AuthorizeUsuarios]
        public async Task<IActionResult> PedirCita(int idusuario)
        {
            string token =
                HttpContext.Session.GetString("TOKEN");

            List<Mascota> mascotas = await this.service.GetMascotas(token);
            ViewData["MASCOTAS"] = new List<Mascota>(mascotas);

            List<Cita> citas = await this.service.GetCitas(token);
            ViewData["CITAS"] = HelperJson.SerializeObject<List<Cita>>(citas);

            return View();
        }

        [AuthorizeUsuarios]
        [HttpPost]
        public async Task<IActionResult> PedirCita(int idmascota, string tipo, string fecha, string hora)
        {
            string token =
                HttpContext.Session.GetString("TOKEN");
            Usuario usuario = await
                this.service.GetPerfilUsuarioAsync(token);

            string dateTimeString = fecha + " " + hora + ":00.00";
            DateTime citaDateTime = DateTime.ParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss.ff", CultureInfo.InvariantCulture);

            await this.service.CreateCita(usuario.IdUsuario, idmascota, tipo, citaDateTime, token);
            
            Mascota mascota = await this.service.FindMascotaAsync(token, idmascota);

            List<Mascota> mascotas = await this.service.GetMascotas(token);
            ViewData["MASCOTAS"] = new List<Mascota>(mascotas);

            List<Cita> citas = await this.service.GetCitas(token);
            ViewData["CITAS"] = HelperJson.SerializeObject<List<Cita>>(citas);

            ViewData["MENSAJE"] = "Cita solicitada Correctamente";
            ViewData["FECHA"] = citaDateTime;

            DateTime fechaFormateada = DateTime.ParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            string email = usuario.Email;
            string asunto = "Cita Solicita Correctamente";
            string mensaje = "Cita solicitada para mascota:" + mascota.Nombre + " en el día " + fechaFormateada.ToString("dd-MM-yyyy");

            await this.service.SendMailAsync(email, asunto, mensaje);


            return View();
        }

        #endregion


        #region MASCOTAS

        [AuthorizeUsuarios]
        public async Task<IActionResult> EditPet(int idmascota)
        {
            string token =
                HttpContext.Session.GetString("TOKEN");
            Usuario usuario = await
                this.service.GetPerfilUsuarioAsync(token);
            Mascota mascota = await this.service.FindMascotaAsync(token, idmascota);

            BlobModel blobPerfil = await this.serviceblob.FindBlobPrivado("usuariosimages", mascota.Imagen, usuario.Apodo);
            ViewData["IMAGEN_MASCOTA"] = blobPerfil;
            return View(mascota);
        }

        [AuthorizeUsuarios]
        [HttpPost]
        public async Task<IActionResult> EditPet(int idusuario, int idmascota, string nombre, string raza,
            string tipo, int peso, DateTime fechanacimiento, IFormFile? fichero)
        {

            string token =
                HttpContext.Session.GetString("TOKEN");
            Usuario usuario = await
                this.service.GetPerfilUsuarioAsync(token);
            Mascota mascota = await this.service.FindMascotaAsync(token, idmascota);

            if (fichero != null)
            {
                await this.serviceblob.DeleteBlobAsync("mascotasimages", mascota.Imagen);

                string fileName = fichero.FileName;
                using (Stream stream = fichero.OpenReadStream())
                {
                    await this.serviceblob.UploadBlobAsync
                    ("mascotasimages", fileName, stream);

                }

                Mascota pet = await this.service.UpdateMascota(idusuario, idmascota, nombre, raza,
                    tipo, peso, fechanacimiento, fileName, token);

                ViewData["MENSAJE"] = "CAMBIOS EFECTUADOS CORRECTAMENTE SE VERAN LA PROXIMA VEZ QUE INICIE";
                BlobModel blobPerfil = await this.serviceblob.FindBlobPrivado("mascotasimages", mascota.Imagen, usuario.Apodo);
                ViewData["IMAGEN_MASCOTA"] = blobPerfil;
                return View(pet);
            }
            else
            {
                Mascota pet = await this.service.UpdateMascota(idusuario, idmascota, nombre, raza,
                    tipo, peso, fechanacimiento, mascota.Imagen, token);

                ViewData["MENSAJE"] = "CAMBIOS EFECTUADOS CORRECTAMENTE SE VERAN LA PROXIMA VEZ QUE INICIE";
                BlobModel blobPerfil = await this.serviceblob.FindBlobPrivado("mascotasimages", mascota.Imagen, usuario.Apodo);
                ViewData["IMAGEN_MASCOTA"] = blobPerfil;
                return View(pet);
            }

        }

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
            Usuario usuario = await
                this.service.GetPerfilUsuarioAsync(token);

            List<BlobModel> listBlobs = new List<BlobModel>();
            foreach (Prueba prueba in pruebas)
            {
                BlobModel blob =
                    await this.serviceblob.FindBlobPrivado("pruebascontainer", prueba.NameFile, usuario.Apodo);
                listBlobs.Add(blob);
            }

            ViewData["PRUEBAS"] = listBlobs;
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



        [AuthorizeUsuarios]
        public async Task<IActionResult> FAQs()
        {
            List<FAQ> faqs = await this.serviceApp.GetFaqsAsync();
            return View(faqs);
        }

    }
}
