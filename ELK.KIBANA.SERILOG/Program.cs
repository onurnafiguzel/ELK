using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

ConfigureLogging();
builder.Host.UseSerilog();

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

void ConfigureLogging()
{
	var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

	var configuration = new ConfigurationBuilder()
		.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
		.AddJsonFile(
		$"appsettings.{environment}.json", optional: true)
		.Build();

	Log.Logger = new LoggerConfiguration()
		.Enrich.FromLogContext()
		.Enrich.WithExceptionDetails()
		.WriteTo.Debug()
		.WriteTo.Console()
		.WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment))
		.Enrich.WithProperty("Environment", environment)
		.ReadFrom.Configuration(configuration)
		.CreateLogger();

}

ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment)
{
	return new ElasticsearchSinkOptions(new Uri(configuration["ElasticSearchConfiguration:Uri"]))
	{
		AutoRegisterTemplate = true,
		IndexFormat = $"{Assembly.GetCallingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{environment.ToLower()}-{DateTime.UtcNow:yyyy-MM}",
		NumberOfReplicas = 1,
		NumberOfShards = 2
	};
}