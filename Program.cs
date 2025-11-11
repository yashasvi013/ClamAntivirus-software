//using System.Net;
//using Microsoft.OpenApi.Models;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container
//builder.Services.AddControllers();

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
//    });
//});

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "ClamAV + GENAI API",
//        Version = "v1",
//        Description = "Combined Microservice with ClamAV and GENAI endpoints"
//    });
//});

//var app = builder.Build();

//// Configure certificate validation globally (not recommended for production)
//ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

//// Configure middleware pipeline

//app.UseSwagger();
//app.UseSwaggerUI(c =>
//{
//    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ClamAV + GENAI API v1");
//    c.RoutePrefix = "swagger";
//});


//app.UseAuthorization();
//app.UseCors("AllowAll");
//app.MapControllers();

//app.Run();
using System.Net;
using ClamAVMicroservice.Controllers;
using ClamAVMicroservice.Services; // <-- ADDED: This line is needed to recognize your services.
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Add services to the container ---

builder.Services.AddControllers();

// Your existing CORS policy is preserved.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();

// Your existing Swagger configuration is preserved.
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ClamAV + GENAI API",
        Version = "v1",
        Description = "Combined Microservice with ClamAV and GENAI endpoints"
    });
});


// --- Register your custom services for Dependency Injection ---
// This tells the application how to create your services when a controller asks for them.
builder.Services.AddScoped<IEmailService, EmailService>();

// Register GenAI service for dependency injection
builder.Services.AddScoped<IGenAIService, GenAIService>();


var app = builder.Build();

// Your existing certificate validation callback is preserved.
// (Not recommended for production)
ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;


// --- Configure the HTTP request pipeline ---

// Your existing Swagger middleware configuration is preserved.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ClamAV + GENAI API v1");
    c.RoutePrefix = "swagger";
});


app.UseAuthorization();
app.UseHttpsRedirection();
// Your existing CORS middleware is preserved.
app.UseCors("AllowAll");

app.MapControllers();

app.Run();