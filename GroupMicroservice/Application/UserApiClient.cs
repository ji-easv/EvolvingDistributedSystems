using System.Net;
using GroupMicroservice.Domain.DTOs;

namespace GroupMicroservice.Application;

public class UserApiClient(IHttpClientFactory httpClientFactory)
{
    public async Task<GetUserDto?> GetUserAsync(Guid userId)
    {
        using var httpClient = httpClientFactory.CreateClient("UserApi");
        var response = await httpClient.GetFromJsonAsync<GetUserDto>($"/api/v1/user/{userId}");
        return response;
    }
    
    public async Task<List<GetUserDto>> GetUsersByIdsAsync(List<Guid> userIds)
    {
        using var httpClient = httpClientFactory.CreateClient("UserApi");
        var response = await httpClient.PostAsJsonAsync("/api/v1/user/batch", userIds);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new HttpRequestException("Users not found", null, HttpStatusCode.NotFound);
        }
        return await response.Content.ReadFromJsonAsync<List<GetUserDto>>();
    }
}