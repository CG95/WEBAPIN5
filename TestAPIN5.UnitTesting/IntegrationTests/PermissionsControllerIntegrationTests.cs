using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using WebAPIN5;
using WebAPIN5.Models;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace TestAPIN5.IntegrationTests
{
    public class PermissionsControllerIntegrationTests : IClassFixture<WebApplicationFactory<WebAPIN5.Program>>
    {
        private readonly HttpClient _client;

        public PermissionsControllerIntegrationTests(WebApplicationFactory<WebAPIN5.Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task RequestPermission_ValidModel_ReturnsOkResult()
        {
            // Arrange
            var permissionDto = new PermissionDTO
            {
                EmployeeForename = "John",
                EmployeeSurname = "Doe",
                PermissionTypeId = 1,
                PermissionDate = DateTime.UtcNow
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(permissionDto),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/permissions", jsonContent);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetPermissions_ReturnsOkResult()
        {
            // Act
            var response = await _client.GetAsync("/api/permissions");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

    }
}
