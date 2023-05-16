using Azure.Storage.Blobs;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NugetVetPet.Models;
using System.Net.Http.Headers;
using System.Text;
using Azure.Security.KeyVault.Secrets;
using static System.Net.Mime.MediaTypeNames;

namespace MvcVetPet.Services
{
    public class ServiceUsuarios
    {
        private BlobServiceClient client;

        private MediaTypeWithQualityHeaderValue Header;
        private string UrlApiUsuarios;
        private string urlEmail;

        public ServiceUsuarios(SecretClient secretClient, BlobServiceClient client)
        {
            KeyVaultSecret keyVaultSecret =
                 secretClient.GetSecretAsync("ApiVetPetSecret").Result.Value;
            this.UrlApiUsuarios =
                keyVaultSecret.Value;

            KeyVaultSecret keyVaultSecretEmail =
                 secretClient.GetSecretAsync("EmailUri").Result.Value;
            this.urlEmail =
                keyVaultSecretEmail.Value;

            this.Header =
                new MediaTypeWithQualityHeaderValue("application/json");

            this.client = client;
        }


        public async Task<string> GetTokenAsync(string username, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "/api/auth/login";
                client.BaseAddress = new Uri(this.UrlApiUsuarios);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                LoginModel model = new LoginModel
                {
                    UserName = username,
                    Password = password
                };

                string jsonModel = JsonConvert.SerializeObject(model);
                StringContent content =
                    new StringContent(jsonModel, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
                if (response.IsSuccessStatusCode)
                {
                    string data =
                        await response.Content.ReadAsStringAsync();
                    JObject jsonObject = JObject.Parse(data);
                    string token =
                        jsonObject.GetValue("response").ToString();
                    return token;
                }
                else
                {
                    return null;
                }
            }
        }

        private async Task<T> CallApiAsync<T>(string request)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApiUsuarios);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                HttpResponseMessage response =
                    await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data =
                        await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }

            }
        }

        private async Task<T> CallApiAsync<T>(string request, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApiUsuarios);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add
                    ("Authorization", "bearer " + token);
                HttpResponseMessage response =
                    await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data =
                        await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }

        public async Task SendMailAsync(string email, string asunto, string mensaje)
        {
            
            var model = new
            {
                email = email,
                subject = asunto,
                mensaje = mensaje
            };

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                string json = JsonConvert.SerializeObject(model);
                StringContent content =
                    new StringContent(json, Encoding.UTF8, "application/json");
                await client.PostAsync(this.urlEmail, content);
            }
        }


        #region USUARIOS

        public async Task GetRegisterUserAsync
            (string username, string email, string password, string imagen)
        {

            using (HttpClient client = new HttpClient())
            {
                string request = "/api/auth/register";
                client.BaseAddress = new Uri(this.UrlApiUsuarios);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);

                Usuario usuario = new Usuario();
                usuario.IdUsuario = 0;
                usuario.Nombre = username;
                usuario.Apodo = username;
                usuario.Email = email;
                usuario.Pass = password;
                usuario.Salt = "";
                usuario.Telefono = "666666666";
                usuario.Password = Encoding.UTF8.GetBytes("valor_predeterminado");
                usuario.Imagen = imagen;

                string json = JsonConvert.SerializeObject(usuario);

                StringContent content =
                    new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response =
                    await client.PostAsync(request, content);
            }
        }

        public async Task<Usuario> GetPerfilUsuarioAsync
            (string token)
        {
            string request = "/api/usuarios/perfilusuario";
            Usuario usuario = await
                this.CallApiAsync<Usuario>(request, token);
            return usuario;
        }

        public async Task<Usuario> UpdateUsuario(int idusuario, string nombre, string apodo,
            string email, string telefono, string fileName, string token)
        {

            using (HttpClient client = new HttpClient())
            {
                string request = "/api/usuarios";
                client.BaseAddress = new Uri(this.UrlApiUsuarios);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add
                    ("Authorization", "bearer " + token);

                Usuario user = await GetPerfilUsuarioAsync(token);
                user.Nombre = nombre;
                user.Apodo = apodo;
                user.Email = email;
                user.Telefono = telefono;
                user.Imagen = fileName;

                string json = JsonConvert.SerializeObject(user);

                StringContent content =
                    new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response =
                    await client.PutAsync(request, content);

                return user;
            }
        }

        public async Task<List<Evento>> GetEventos(string token)
        {
            string request = "/api/usuarios/eventos";
            List<Evento> eventos = await
                this.CallApiAsync<List<Evento>>(request, token);
            return eventos;
        }

        public async Task<List<Cita>> GetCitas(string token)
        {
            string request = "/api/usuarios/citas";
            List<Cita> citas = await
                this.CallApiAsync<List<Cita>>(request, token);
            return citas;
        }

        public async Task CreateCita(int idusuario, int idmascota, string tipo, DateTime fecha, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "/api/usuarios/solicitarcita";
                client.BaseAddress = new Uri(this.UrlApiUsuarios);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add
                    ("Authorization", "bearer " + token);

                Cita cita = new Cita();
                cita.IdCita = 0;
                cita.TipoCita = tipo;
                cita.IdMascota = idmascota;
                cita.IdUsuario = idusuario;
                cita.DiaCita = fecha;

                string json = JsonConvert.SerializeObject(cita);

                StringContent content =
                    new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response =
                    await client.PostAsync(request, content);
            }

        }

        #endregion

        #region MASCOTAS

        public async Task<Mascota> FindMascotaAsync
            (string token, int id)
        {
            string request = "/api/mascotas/mascota/" + id;
            Mascota mascota = await
                this.CallApiAsync<Mascota>(request, token);
            return mascota;
        }

        public async Task<Mascota> UpdateMascota(int idusuario, int idmascota, string nombre, string raza,
            string tipo, int peso, DateTime fechanacimiento, string fileName, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "/api/mascotas";
                client.BaseAddress = new Uri(this.UrlApiUsuarios);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add
                    ("Authorization", "bearer " + token);

                Mascota mascota = await FindMascotaAsync(token, idmascota);
                mascota.Nombre = nombre;
                mascota.Raza = raza;
                mascota.Tipo = tipo;
                mascota.Peso = peso;
                mascota.Fecha_Nacimiento = fechanacimiento;
                mascota.Imagen = fileName;

                string json = JsonConvert.SerializeObject(mascota);

                StringContent content =
                    new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response =
                    await client.PutAsync(request, content);

                return mascota;
            }
        }

        public async Task<List<Mascota>> GetMascotas(string token)
        {
            string request = "/api/mascotas";
            List<Mascota> mascotas = await
                this.CallApiAsync<List<Mascota>>(request, token);
            return mascotas;
        }       

        public async Task<List<Tratamiento>> GetTratamientos(string token)
        {
            string request = "/api/mascotas/tratamientos";
            List<Tratamiento> tratamientos = await
                this.CallApiAsync<List<Tratamiento>>(request, token);
            return tratamientos;
        }

        public async Task<List<Vacuna>> GetVacunas(string token)
        {
            string request = "/api/mascotas/vacunas";
            List<Vacuna> vacunas = await
                this.CallApiAsync<List<Vacuna>>(request, token);
            return vacunas;
        }

        public async Task<List<Prueba>> GetPruebas(string token)
        {
            string request = "/api/mascotas/pruebas";
            List<Prueba> pruebas = await
                this.CallApiAsync<List<Prueba>>(request, token);
            return pruebas;
        }

        public async Task<List<Procedimiento>> GetProcedimientos(string token)
        {
            string request = "/api/mascotas/procedimientos";
            List<Procedimiento> procedimientos = await
                this.CallApiAsync<List<Procedimiento>>(request, token);
            return procedimientos;
        }

        #endregion

    }
}
