using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using NugetVetPet.Models;
using System.Configuration;
using System.Threading.Tasks;

namespace MvcVetPet.Services
{
    public class ServiceStorageBlobs
    {
        private BlobServiceClient client;
        private string UrlAzureStorage;

        public ServiceStorageBlobs(IConfiguration configuration, BlobServiceClient client)
        {
            this.client = client;
            this.UrlAzureStorage =
                configuration.GetValue<string>("AzureKeys:StorageAccount");
        }


        //METODOD PARA RECUPERAR TODOS LOS BLOBS
        public async Task<List<BlobModel>> GetBlobsAsync(string containerName)
        {
            //RECUPERAMOS UN CLIENT DEL CONTAINER
            BlobContainerClient containerClient =
                this.client.GetBlobContainerClient(containerName);
            List<BlobModel> blobModels = new List<BlobModel>();
            await foreach (BlobItem item in containerClient.GetBlobsAsync())
            {
                //NECESITAMOS UN BLOB CLIENT PARA VISUALIZAR MAS CARACTERISTICAS DEL OBJETO
                BlobClient blobClient =
                    containerClient.GetBlobClient(item.Name);
                BlobModel model = new BlobModel();
                model.Nombre = item.Name;
                model.Contenedor = containerName;
                model.Url = blobClient.Uri.AbsoluteUri;
                blobModels.Add(model);
            };
            return blobModels;
        }

        //METODOD PARA RECUPERAR UN BLOBS
        public async Task<BlobModel> FindBlobPerfil(string containerName, string blobName, string usuario)
        {
            string connectionString = this.UrlAzureStorage;

            // RECUPERAMOS UN CLIENT DEL CONTAINER
            BlobContainerClient containerClient =
                this.client.GetBlobContainerClient(containerName);
            List<BlobModel> blobModels = new List<BlobModel>();
            await foreach (BlobItem item in containerClient.GetBlobsAsync())
            {
                // NECESITAMOS UN BLOB CLIENT PARA VISUALIZAR MAS CARACTERISTICAS DEL OBJETO
                BlobClient blobClient =
                    containerClient.GetBlobClient(item.Name);

                if (item.Name == blobName)
                {
                    // GENERAR UN SAS PARA EL BLOB ACTUAL
                    string sasToken = await this.GenerateBlobSasAsync(connectionString, containerName, item.Name, usuario);

                    // AGREGAR EL BLOB A LA LISTA SOLO SI EL USUARIO TIENE PERMISO
                    if (!string.IsNullOrEmpty(sasToken))
                    {
                        BlobModel model = new BlobModel();
                        model.Nombre = item.Name;
                        model.Contenedor = containerName;
                        model.Url = sasToken;
                        blobModels.Add(model);
                    }
                }

            };
            BlobModel blobPerfil = blobModels.FirstOrDefault();
            return blobPerfil;
        }

        //METODO PARA DAR PERMISOS SAS
        public async Task<string> GenerateBlobSasAsync(string connectionString, string containerName, string blobName, string policyName)
        {
            var blobClient = new BlobClient(connectionString, containerName, blobName);

            var blobSasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };

            blobSasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasToken = blobClient.GenerateSasUri(blobSasBuilder);

            return sasToken.ToString();
        }

        //METODO PARA ELIMINAR BLOBS
        public async Task DeleteBlobAsync(string containerName, string blobName)
        {
            BlobContainerClient containerClient =
                this.client.GetBlobContainerClient(containerName);
            await containerClient.DeleteBlobAsync(blobName);
        }

        //METODO PARA SUBIR UN BLOB
        public async Task UploadBlobAsync(string containerName, string blobName, Stream stream)
        {
            BlobContainerClient containerClient =
                this.client.GetBlobContainerClient(containerName);
            await containerClient.UploadBlobAsync(blobName, stream);
        }

    }
}
