using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Cokee.ClassService.Shared;

namespace Cokee.ClassService.Helper
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        public static string AccessToken = "";

        public ApiClient()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://api.cokee.tech:15043/api/"); // API基地址
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoic3RyaW5nIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbW9iaWxlcGhvbmUiOiIwIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJleHAiOjE3NDA1Mzk3OTgsImlzcyI6IkNva2VlIiwiYXVkIjoiQ29rZWVDbGFzc1NlcnZpY2UifQ.3Fda7QoBHWmfnPSN8kBnP4Va34TTub4E6eRaCVQUvCo");
        }

        /*public async Task<LoginResult> LoginAsync(string userName, string password)
        {
            var content = new StringContent(JsonSerializer.Serialize(new LoginRequest { Username = userName, Password = password }), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync($"user/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var model = JsonSerializer.Deserialize<LoginResult>(responseContent);
                return model;
            }
            else
            {
                throw new Exception($"Error: {response.StatusCode}");
            }
        }*/

        public async Task<string> RegisterAsync(RegisterRequest registerRequest)
        {
            var content = new StringContent(JsonSerializer.Serialize(registerRequest), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("user/register", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                // 处理注册成功后的逻辑
                return responseContent;
            }
            else
            {
                return $"Error: {response.StatusCode}";
            }
        }

        public async Task<bool> CheckSystemStatus()
        {
            var content = new StringContent("", Encoding.UTF8, "application/text");
            HttpResponseMessage response = await _httpClient.PostAsync("system/status", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<ClassInfoRequest> GetClassInfo(int classId)
        {
            var response = await _httpClient.GetAsync($"Class/GetClassInfo?classId={classId}");
            response.EnsureSuccessStatusCode(); // Throw if not a success code

            var jsonString = await response.Content.ReadAsStringAsync();
            var classInfo = JsonSerializer.Deserialize<ClassInfoRequest>(jsonString);

            return classInfo;
        }

        public async Task<IEnumerable<Student>> GetStudents(int classId)
        {
            var response = await _httpClient.GetAsync($"Class/GetStudents?classId={classId}");
            response.EnsureSuccessStatusCode(); // Throw if not a success code

            var jsonString = await response.Content.ReadAsStringAsync();
            var students = JsonSerializer.Deserialize<List<Student>>(jsonString);

            return students;
        }

        // 创建学生
        public async Task<string> CreateStudentAsync(string jsonContent)
        {
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("Students/CreateStudent", content);
            var a = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return "Student created successfully";
            }
            else
            {
                return $"Error: {response.StatusCode} {a}";
            }
        }

        public async Task<string> UpdateStudentAsync(string jsonContent)
        {
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("Students/CreateStudent", content);
            var a = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return "Student created successfully";
            }
            else
            {
                return $"Error: {response.StatusCode} {a}";
            }
        }
    }
}