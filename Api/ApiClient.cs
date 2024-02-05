using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Cokee.ClassService.Api
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://your-api-base-url.com/api/"); // 替换为您的API基地址
        }

        // 获取所有学生
        public async Task<string> GetStudentsAsync()
        {
            HttpResponseMessage response = await _httpClient.GetAsync("students");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return $"Error: {response.StatusCode}";
            }
        }

        // 创建学生
        public async Task<string> CreateStudentAsync(string jsonContent)
        {
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("students", content);

            if (response.IsSuccessStatusCode)
            {
                return "Student created successfully";
            }
            else
            {
                return $"Error: {response.StatusCode}";
            }
        }

        // 将学生添加到班级
        public async Task<string> AddStudentToClassAsync(int classId, int studentId)
        {
            HttpResponseMessage response = await _httpClient.PostAsync($"classes/{classId}/students/{studentId}", null);

            if (response.IsSuccessStatusCode)
            {
                return "Student added to class successfully";
            }
            else
            {
                return $"Error: {response.StatusCode}";
            }
        }
    }
}