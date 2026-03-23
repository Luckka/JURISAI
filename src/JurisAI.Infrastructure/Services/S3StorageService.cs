namespace JurisAI.Infrastructure.Services;

using Amazon.S3;
using Amazon.S3.Model;
using JurisAI.Domain.Common;
using JurisAI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class S3Options
{
    public string BucketName { get; set; } = "jurisai-documentos";
}

public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly S3Options _options;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(IAmazonS3 s3, IOptions<S3Options> options, ILogger<S3StorageService> logger)
    {
        _s3 = s3;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<string>> UploadAsync(
        string key, Stream content, string contentType, CancellationToken ct = default)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = key,
                InputStream = content,
                ContentType = contentType
            };

            await _s3.PutObjectAsync(request, ct);
            _logger.LogInformation("Arquivo {Key} enviado para S3", key);
            return Result<string>.Success(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar arquivo {Key} para S3", key);
            return Result<string>.Failure(Error.ExternalService("S3", ex.Message));
        }
    }

    public async Task<Result<Stream>> DownloadAsync(string key, CancellationToken ct = default)
    {
        try
        {
            var request = new GetObjectRequest { BucketName = _options.BucketName, Key = key };
            var response = await _s3.GetObjectAsync(request, ct);
            return Result<Stream>.Success(response.ResponseStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao baixar arquivo {Key} do S3", key);
            return Result<Stream>.Failure(Error.ExternalService("S3", ex.Message));
        }
    }

    public async Task<Result<string>> GetPresignedUrlAsync(
        string key, TimeSpan expiration, CancellationToken ct = default)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _options.BucketName,
                Key = key,
                Expires = DateTime.UtcNow.Add(expiration),
                Verb = HttpVerb.GET
            };

            var url = await _s3.GetPreSignedURLAsync(request);
            return Result<string>.Success(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar URL pré-assinada para {Key}", key);
            return Result<string>.Failure(Error.ExternalService("S3", ex.Message));
        }
    }
}
