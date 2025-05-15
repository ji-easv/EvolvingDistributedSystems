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
}