namespace JurisAI.Domain.Interfaces.Services;

using JurisAI.Domain.Common;

public interface IStorageService
{
    Task<Result<string>> UploadAsync(string key, Stream content, string contentType, CancellationToken ct = default);
    Task<Result<Stream>> DownloadAsync(string key, CancellationToken ct = default);
    Task<Result<string>> GetPresignedUrlAsync(string key, TimeSpan expiration, CancellationToken ct = default);
}
