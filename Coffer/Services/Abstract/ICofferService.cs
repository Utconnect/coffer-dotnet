using Utconnect.Common.Models;

namespace Utconnect.Coffer.Services.Abstract;

/// <summary>
/// Defines methods for retrieving configuration secrets from a key storage system.
/// </summary>
public interface ICofferService
{
    /// <summary>
    /// Retrieves a secret key associated with the specified application and secret name.
    /// </summary>
    /// <param name="app">The name of the application for which the secret is requested.</param>
    /// <param name="secretName">The name of the secret to retrieve.</param>
    /// <returns>A <see cref="Task"/> of <see cref="Result{T}"/> representing the asynchronous operation, with a <see cref="Result{T}"/> containing the secret key as a string.</returns>
    Task<Result<string>> GetKey(string app, string secretName);
}