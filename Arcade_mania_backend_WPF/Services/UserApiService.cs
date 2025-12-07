using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Arcade_mania_backend_webAPI.Models.Dtos.Users;

namespace Arcade_mania_backend_WPF.Services
{
    public class UserApiService
    {
        private readonly HttpClient _httpClient;

        // később configból jöhet, most maradhat localhost
        private const string BaseUrl = "http://localhost:5118/api/Users";

        public UserApiService()
        {
            _httpClient = new HttpClient();
        }

        // Összes user admin nézettel (ID + plain jelszó + score-ok)
        public async Task<List<UserDataAdminDto>> GetAllUsersAdminAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<UserDataAdminDto>>>(
                $"{BaseUrl}/admin"
            );

            return response?.Result ?? new List<UserDataAdminDto>();
        }

        // Új user létrehozása (POST: api/users/admin)
        public async Task<UserDataAdminDto?> CreateUserAdminAsync(UserCreateAdminDto dto)
        {
            var httpResponse = await _httpClient.PostAsJsonAsync($"{BaseUrl}/admin", dto);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var error = await httpResponse.Content.ReadAsStringAsync();
                throw new Exception($"API hiba ({(int)httpResponse.StatusCode}): {error}");
            }

            var apiResponse =
                await httpResponse.Content.ReadFromJsonAsync<ApiResponse<UserDataAdminDto>>();

            return apiResponse?.Result;
        }

        // Egy user lekérése ID alapján (GET: api/users/admin/{id})
        public async Task<UserDataAdminDto?> GetUserAdminByIdAsync(Guid id)
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<UserDataAdminDto>>(
                $"{BaseUrl}/admin/{id}"
            );

            return response?.Result;
        }

        // User + score-ok módosítása (PUT: api/users/admin/{id})
        public async Task UpdateUserAdminAsync(Guid id, UserUpdateAdminDto dto)
        {
            var httpResponse = await _httpClient.PutAsJsonAsync($"{BaseUrl}/admin/{id}", dto);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var error = await httpResponse.Content.ReadAsStringAsync();
                throw new Exception($"API hiba ({(int)httpResponse.StatusCode}): {error}");
            }
        }

        // User törlése (DELETE: api/users/admin/{id})
        public async Task DeleteUserAdminAsync(Guid id)
        {
            var httpResponse = await _httpClient.DeleteAsync($"{BaseUrl}/admin/{id}");

            if (!httpResponse.IsSuccessStatusCode)
            {
                var error = await httpResponse.Content.ReadAsStringAsync();
                throw new Exception($"API hiba ({(int)httpResponse.StatusCode}): {error}");
            }
        }
    }

    // WebAPI válasz wrapper: { message: "...", result: ... }
    public class ApiResponse<T>
    {
        public string? Message { get; set; }
        public T? Result { get; set; }
    }
}
