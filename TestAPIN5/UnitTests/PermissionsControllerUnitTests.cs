using Moq;
using System.Threading.Tasks;
using Xunit;
using WebAPIN5.Controllers;
using WebAPIN5.Commands;
using WebAPIN5.Queries;
using WebAPIN5.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using WebAPIN5.Models;
using WebAPIN5.Repositories;
using Nest;
using Confluent.Kafka;
using NuGet.Protocol;

namespace TestAPIN5.UnitTests
{
    public class PermissionsControllerUnitTests
    {

        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IElasticClient> _mockElasticClient;
        private readonly Mock<WebAPIN5.Commands.IRequest<Permission>> _mockCreateCommand;
        private readonly Mock<WebAPIN5.Commands.IRequest<Permission>> _mockModifyCommand;
        private readonly Mock<IQuery<IEnumerable<Permission>>> _mockGetQuery;
        //private readonly Mock<GetAllPermissionsQuery> _mockGetQuery;

        private readonly Mock<IProducer<Null, string>> _mockKafkaProducerClient; // Mock Kafka Producer client
        private readonly KafkaProducer _kafkaProducer;

        private readonly Mock<IElasticsearchService> _mockElasticsearchService;
        private readonly Mock<ILogger<PermissionsController>> _mockLogger;

        private readonly PermissionsController _controller;

        public PermissionsControllerUnitTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();


            // Create a real instance of KafkaProducer with the mocked IProducer and a dummy topic
            _mockKafkaProducerClient = new Mock<IProducer<Null, string>>();
            _kafkaProducer = new KafkaProducer(_mockKafkaProducerClient.Object, "test-topic");


            _mockElasticClient = new Mock<IElasticClient>();

            _mockCreateCommand = new Mock<WebAPIN5.Commands.IRequest<Permission>>();
            _mockModifyCommand = new Mock<WebAPIN5.Commands.IRequest<Permission>>();
            _mockGetQuery = new Mock<IQuery<IEnumerable<Permission>>>();
            _mockElasticsearchService = new Mock<IElasticsearchService>();
            _mockLogger = new Mock<ILogger<PermissionsController>>();

            _controller = new PermissionsController(
                _mockCreateCommand.Object,
                _mockModifyCommand.Object,
                _mockGetQuery.Object,
                _mockElasticsearchService.Object,
                _kafkaProducer,
                _mockLogger.Object
                );
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

            _mockCreateCommand
                .Setup(c => c.HandleAsync(It.IsAny<Permission>()))
                .Returns(Task.CompletedTask);

            _mockKafkaProducerClient
               .Setup(k => k.ProduceAsync(
                   It.IsAny<string>(),
                   It.IsAny<Message<Null, string>>(),
                     It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new DeliveryResult<Null, string>()));


            _mockElasticsearchService
                .Setup(e => e.IndexPermissionAsync(It.IsAny<Permission>()))
                .Returns(Task.CompletedTask);




            // Act
            var result = await _controller.RequestPermission(permissionDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);

        }

        [Fact]
        public async Task RequestPermission_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("EmployeeForename", "Required");

            var permissionDto = new PermissionDTO
            {
                EmployeeForename = "",
                EmployeeSurname = "Doe",
                PermissionTypeId = 1,
                PermissionDate = DateTime.UtcNow
            };

            // Act
            var result = await _controller.RequestPermission(permissionDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ModifyPermission_ReturnsOkResult()
        {
            // Arrange
            var permissionDTO = new PermissionDTO
            {
                EmployeeForename = "John",
                EmployeeSurname = "Doe",
                PermissionTypeId = 1,
                PermissionDate = DateTime.UtcNow
            };

            // Act
            var result = await _controller.ModifyPermission(1, permissionDTO);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var response = okResult.Value as dynamic;

            // Check the properties in the response object
            Assert.Equal("Permission modified successfully", (string)response.Message);
        }

        [Fact]
        public async Task ModifyPermission_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("EmployeeForename", "Required");

            var permissionDto = new PermissionDTO
            {
                EmployeeForename = "",
                EmployeeSurname = "Doe",
                PermissionTypeId = 1,
                PermissionDate = DateTime.UtcNow
            };

            // Act
            var result = await _controller.ModifyPermission(1, permissionDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = badRequestResult.Value as SerializableError;

            // Check that the error message is related to the EmployeeForename field
            Assert.NotNull(errorResponse);
            Assert.True(errorResponse.ContainsKey("EmployeeForename"));
        }


        [Fact]
        public async Task GetPermissions_ReturnsOkResult()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                new Permission { Id = 1, EmployeeForename = "John", EmployeeSurname = "Doe", PermissionTypeId = 1, PermissionDate = DateTime.UtcNow }
            };
            _mockGetQuery.Setup(q => q.ExecuteAsync()).ReturnsAsync(permissions);

            // Act
            var result = await _controller.GetPermissions();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<PermissionDTO>>(okResult.Value);
            Assert.Single(returnValue);
        }

    }
}