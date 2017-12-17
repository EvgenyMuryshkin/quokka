using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Quokka.Public.Tools;
using Quokka.Public.Logging;
using System.Linq;

namespace Quokka.Public.Transformation
{
    public class CloudTransformation : IQuokkaProjectTransformation
    {
        private readonly ILogStream _logStream;
        private readonly RuntimeConfiguration _runtimeConfiguration;

        public CloudTransformation(
            ILogStream logStream,
            RuntimeConfiguration runtimeConfiguration
            )
        {
            _logStream = logStream;
            _runtimeConfiguration = runtimeConfiguration;
        }

        public async Task<TransformationResponse> Transform(TransformationRequest sourceRequest)
        {
            var client = new HttpClient();
            var address = "https://cynxiooa76.execute-api.ap-southeast-2.amazonaws.com";

            Console.WriteLine($"Requesting cloud transform from {address}");

            var request = QuokkaJson.Copy(sourceRequest);
            request.ControllerNames = _runtimeConfiguration.GetControllers?.ToList();

            var payload = JsonConvert.SerializeObject(request);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var data = await client.PostAsync($"{address}/prod", content);
            Console.WriteLine($"Completed with code: {data.StatusCode}");

            if (data.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"Request failed: {data.StatusCode}");
            }

            var responseString = await data.Content.ReadAsStringAsync();
            Console.WriteLine($"Content length: {responseString.Length}");

            var response = JsonConvert.DeserializeObject<TransformationResponse>(responseString);

            foreach(var record in response.Logs)
            {
                _logStream.Log(record);
            }

            return response;
        }
    }
}
