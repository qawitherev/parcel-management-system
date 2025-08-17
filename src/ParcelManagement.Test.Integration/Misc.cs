using System.Text;
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
    }
}