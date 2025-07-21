namespace Application.Contracts;

public interface ITokenGenerator<TType> where TType : class
{
    Task<TType> GenerateTokenAsync(HttpClient client, FormUrlEncodedContent formUrlEncodedContent, CancellationToken cancellationToken = default);
    
    Task<TType> SendRequestAsync(HttpClient client, HttpRequestMessage request, CancellationToken cancellationToken = default);
}