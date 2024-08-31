using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Utconnect.Coffer.Models;
using Utconnect.Coffer.Services.Abstract;
using Utconnect.Common.Models;
using Utconnect.Common.Models.Errors;

namespace Utconnect.Coffer.Services.Implementations
{
    public class CofferService : ICofferService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IOptions<CofferConfig> _config;

        public CofferService(IHttpClientFactory clientFactory, IOptions<CofferConfig> config)
        {
            _clientFactory = clientFactory;
            _config = config;
        }

        public async Task<Result<string>> GetKey(string app, string secretName)
        {
            string cofferUrl = _config.Value.Url;

            if (string.IsNullOrEmpty(cofferUrl))
            {
                return Result<string>.Failure(new InternalServerError("Coffer URL is empty"));
            }

            var requestUrl = $"{cofferUrl}/secret/be/{app}/{secretName}";
            HttpClient client = _clientFactory.CreateClient();
            HttpResponseMessage response = await client.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                return Result<string>.Failure(new InternalServerError("Response status is not success"));
            }

            try
            {
                string content = await response.Content.ReadAsStringAsync();
                CofferResponse? jwtKey = JsonConvert.DeserializeObject<CofferResponse>(content);
                return jwtKey != null
                    ? Result<string>.Succeed(jwtKey.Data)
                    : Result<string>.Failure(new InternalServerError("Retrieved data is null"));
            }
            catch (Exception)
            {
                return Result<string>.Failure(new InternalServerError("Cannot decode response"));
            }
        }
    }
}