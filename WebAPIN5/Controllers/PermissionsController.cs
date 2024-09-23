using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WebAPIN5.Data;
using WebAPIN5.Models;
using WebAPIN5.Repositories;
using WebAPIN5.Services;
using WebAPIN5.Queries;
using WebAPIN5.Commands;

namespace WebAPIN5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {

        private readonly IRequest<Permission> _createPermissionCommand;
        private readonly IRequest<Permission> _modifyPermissionCommand;
        private readonly IQuery<IEnumerable<Permission>> _getAllPermissionsQuery;
       
        private readonly IElasticsearchService _elasticsearchService;
        private readonly KafkaProducer _kafkaProducer;
        private readonly ILogger<PermissionsController> _logger;

       

        public PermissionsController(
            IRequest<Permission> createPermissionCommand,
            IRequest<Permission> modifyPermissionCommand,
           IQuery<IEnumerable<Permission>> getAllPermissionsQuery,
           IElasticsearchService elasticsearchService,
           KafkaProducer kafkaProducer,
           ILogger<PermissionsController> logger)
        {
            _createPermissionCommand = createPermissionCommand;
            _modifyPermissionCommand = modifyPermissionCommand;
            _getAllPermissionsQuery = getAllPermissionsQuery;
            _elasticsearchService = elasticsearchService;
            _kafkaProducer = kafkaProducer;
            _logger = logger;
        }


        // Request Permission
        [HttpPost]
        public async Task<IActionResult> RequestPermission([FromBody] PermissionDTO permissionDTO)
        {
            _logger.LogInformation("Executing: {operation}", nameof(RequestPermission));

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var permission = new Permission
            {
                EmployeeForename = permissionDTO.EmployeeForename,
                EmployeeSurname = permissionDTO.EmployeeSurname,
                PermissionTypeId = permissionDTO.PermissionTypeId,
                PermissionDate = permissionDTO.PermissionDate,
            };

            //Handle CQRS command for creation
            await _createPermissionCommand.HandleAsync(permission);

            //publish kafka message to topic operations
            var operationMessage = new OperationMessageDTO
            {
                Id = Guid.NewGuid(),
                OperationName = "request"
            };

            await _kafkaProducer.ProduceAsync(operationMessage);

            // Persist permission to Elasticsearch
            await _elasticsearchService.IndexPermissionAsync(permission);

            return Ok("Permission created successfully");

        }

        // Modify Permission
        [HttpPut("{id}")]
        public async Task<IActionResult> ModifyPermission(int id, [FromBody] PermissionDTO permissionDTO)
        {
            _logger.LogInformation("Executing: {operation}", nameof(ModifyPermission));
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.IsValid);
            }

            var permission = new Permission
            {
                Id = id,
                EmployeeForename = permissionDTO.EmployeeForename,
                EmployeeSurname = permissionDTO.EmployeeSurname,
                PermissionTypeId = permissionDTO.PermissionTypeId,
                PermissionDate = permissionDTO.PermissionDate,
            };

            // Handle CQRS command for modification
            await _modifyPermissionCommand.HandleAsync(permission);

            //publish kafka message to topic operations
            var operationMessage = new OperationMessageDTO
            {
                Id = Guid.NewGuid(),
                OperationName = "modify"
            };

            // Persist modified permission to Elasticsearch
            await _elasticsearchService.IndexPermissionAsync(permission);

            await _kafkaProducer.ProduceAsync(operationMessage);

            return Ok("Permission modified successfully");

        }


        // Get Permissions
        [HttpGet]
        public async Task<IActionResult> GetPermissions()
        {

            _logger.LogInformation("Executing: {operation}", nameof(GetPermissions));

            // Handle CQRS query to fetch all permissions
            var permissions = await _getAllPermissionsQuery.ExecuteAsync();
            var permissionsDTO = permissions.Select(p => new PermissionDTO
            {
                Id = p.Id,
                EmployeeForename = p.EmployeeForename,
                EmployeeSurname = p.EmployeeSurname,
                PermissionTypeId = p.PermissionTypeId,
                PermissionDate = p.PermissionDate,
            }).ToList();
            // Send message to Kafka
            var operationMessage = new OperationMessageDTO
            {
                Id = Guid.NewGuid(),
                OperationName = "get"
            };

            _logger.LogInformation("Sending to kafka the following message:{dto}", operationMessage.ToString());

            await _kafkaProducer.ProduceAsync(operationMessage);

            return Ok(permissionsDTO);

        }



    }
}
