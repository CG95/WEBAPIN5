
using Elasticsearch.Net;
using Microsoft.EntityFrameworkCore;
using Nest;
using Serilog;
using Serilog.Events;
using System.Text.Json.Serialization;
using WebAPIN5.Commands;
using WebAPIN5.Data;
using WebAPIN5.Models;
using WebAPIN5.Queries;
using WebAPIN5.Repositories;
using WebAPIN5.Services;

using System.Runtime.CompilerServices;
using NuGet.Protocol;

[assembly: InternalsVisibleTo("TestAPIN5")]

namespace WebAPIN5;

public partial class Program
{
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        builder
            .Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();  // Load environment variables

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()  // Log to console
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)  // Log to file
            .CreateLogger();

        // Set Serilog as the logging provider
        builder.Host.UseSerilog();


        // Add services to the container.
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
            });

        // Register the DbContext with the SQL Server provider
        var sqlServerConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(sqlServerConnectionString)) throw new ArgumentNullException(nameof(sqlServerConnectionString), "SQL Server connection string is not set");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(sqlServerConnectionString));


        //Unit Of Work
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        //repositories

        builder.Services.AddScoped(typeof(WebAPIN5.Repositories.IRepository<>), typeof(Repository<>));

        //CQRS
        builder.Services.AddScoped<WebAPIN5.Commands.IRequest<Permission>, CreatePermissionCommand>(); // For creating permissions
        builder.Services.AddScoped<WebAPIN5.Commands.IRequest<Permission>, ModifyPermissionCommand>(); // For modifying permissions
        builder.Services.AddScoped<WebAPIN5.Queries.IQuery<IEnumerable<Permission>>, GetAllPermissionsQuery>();




        // Elasticsearch Configuration
        var elasticsearchSettings = builder.Configuration.GetSection("Elasticsearch");
        var url = builder.Configuration["Elasticsearch:Url"];
        var elasticUsername = builder.Configuration["Elasticsearch:Username"];
        var elasticPassword = builder.Configuration["Elasticsearch:Password"];
        var index = builder.Configuration["Elasticsearch:Index"];
        

        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url), "Elasticsearch URL is not set");
        if (string.IsNullOrWhiteSpace(index)) throw new ArgumentNullException(nameof(index), "Elasticsearch Index is not set");
        if (string.IsNullOrWhiteSpace(elasticUsername)) throw new ArgumentNullException(nameof(elasticUsername), "Elasticsearch username is not set");
        if (string.IsNullOrWhiteSpace(elasticPassword)) throw new ArgumentNullException(nameof(elasticPassword), "Elasticsearch password is not set");

        var settings = new ConnectionSettings(new Uri(url!))
            .DefaultIndex(index)
            .BasicAuthentication(elasticUsername, elasticPassword);

        var client = new ElasticClient(settings);
        builder.Services.AddSingleton<IElasticClient>(client);
        builder.Services.AddScoped<IElasticsearchService, ElasticsearchService>();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();


        //Inject Kafka Producer
        var kafkaBroker = builder.Configuration["Kafka:Broker"];
        var kafkaTopic = builder.Configuration["Kafka:Topic"];

        
        // Check if any Kafka config is missing
        if (string.IsNullOrWhiteSpace(kafkaBroker)) throw new ArgumentNullException(nameof(kafkaBroker), "Kafka Broker is not set");
        if (string.IsNullOrWhiteSpace(kafkaTopic)) throw new ArgumentNullException(nameof(kafkaTopic), "Kafka Topic is not set");


        var kafkaProducer = new KafkaProducer(kafkaBroker, kafkaTopic);

        // Test Kafka connection
        if (kafkaProducer.TestConnection())
        {
            Log.Information("Successfully connected to Kafka!");
        }
        else
        {
            Log.Error("Failed to connect to Kafka.");
        }

        builder.Services.AddSingleton(kafkaProducer);

        //builder.Services.AddSingleton(new KafkaProducer(kafkaBroker, kafkaTopic));

        

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}