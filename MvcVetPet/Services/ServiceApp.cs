using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using NugetVetPet.Models;
using System.Net.Http.Headers;

namespace MvcVetPet.Services
{
    public class ServiceApp
    {

        private BlobServiceClient client;

        private MediaTypeWithQualityHeaderValue Header;
        private string UrlApi;

        public ServiceApp(SecretClient secretClient, BlobServiceClient client)
        {
            KeyVaultSecret keyVaultSecret =
                 secretClient.GetSecretAsync("ApiVetPetSecret").Result.Value;
            this.UrlApi =
                keyVaultSecret.Value;

            this.Header =
                new MediaTypeWithQualityHeaderValue("application/json");

            this.client = client;
        }

        private async Task<T> CallApiAsync<T>(string request)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
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

        public async Task<List<Servicio>> GetServiciosAsync()
        {
            string request = "/api/application/servicios";
            List<Servicio> servicios = await
                this.CallApiAsync<List<Servicio>>(request);
            return servicios;
        }

        public async Task<List<FAQ>> GetFaqsAsync()
        {
            string request = "/api/application/faqs";
            List<FAQ> faqs = await
                this.CallApiAsync<List<FAQ>>(request);
            return faqs;
        }
    }
}
