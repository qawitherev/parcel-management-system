using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace ParcelManagement.Test.Integration.Misc
{
    public class IntegrationMisc
    {
        public static StringContent ConvertToStringContent<T>(T dto)
        {
            var json = JsonConvert.SerializeObject(dto);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        public static JsonSerializerOptions GetJsonSerializerOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }
    }
}