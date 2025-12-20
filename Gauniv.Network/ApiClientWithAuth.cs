namespace Gauniv.Network
{
    public partial class ApiClient
    {
        public string? BearerToken { get; set; }

        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
        {
            if (!string.IsNullOrEmpty(BearerToken))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", BearerToken);
            }
        }
    }
}