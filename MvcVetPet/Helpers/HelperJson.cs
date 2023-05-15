using Newtonsoft.Json;

namespace MvcVetPet.Helpers
{
    public class HelperJson
    {
        //INTERNAMENTE TRABAJAREMOS CON GetString
        //DEBEMOS RECIBIR UN OBJETO Y TRANSFORMARLO A STRING JSON
        public static T DeserializeObject<T>(string data)
        {
            //MEDIANTE NEWTONSOFT DESERIALIZAMOS EL OBJETO
            T objeto =
                JsonConvert.DeserializeObject<T>(data);
            return objeto;
        }

        public static string SerializeObject<T>(T data)
        {
            string json =
                JsonConvert.SerializeObject(data);
            return json;
        }

    }
}
