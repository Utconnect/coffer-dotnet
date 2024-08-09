using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Utconnect.Coffer.Models;
using Utconnect.Coffer.Services.Abstract;
using Utconnect.Common.Models;
using Utconnect.Common.Models.Errors;

namespace Utconnect.Coffer.Services.Implementations;

public class CofferService(IHttpClientFactory clientFactory, IOptions<CofferConfig> config)
    : ICofferService
{
    public async Task<Result<string>> GetKey(string app, string secretName)
    {
        string cofferUrl = config.Value.Url;

        if (string.IsNullOrEmpty(cofferUrl))
        {
            return Result<string>.Failure(new InternalServerError("Coffer URL is empty"));
        }

        var requestUrl = $"{cofferUrl}/secret/be/{app}/{secretName}";
        HttpClient client = clientFactory.CreateClient();
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