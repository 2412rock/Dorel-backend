using DorelAppBackend.Middleware;
using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Services;
using DorelAppBackend.Services.Implementation;
using DorelAppBackend.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials()
                      .SetPreflightMaxAge(TimeSpan.FromSeconds(3600)); ;
        });
});

builder.Services.AddTransient<ILoginService, LoginService>();
builder.Services.AddTransient<IRedisService, RedisService>();
builder.Services.AddTransient<IRedisCacheService, RedisCacheService>();
builder.Services.AddTransient<IMailService, MailService>();

// Add configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false);

// Add DbContext
string hostIp = "";
IPAddress[] addresses = Dns.GetHostAddresses("host.docker.internal");
if (addresses.Length > 0)
{
    // we are running in docker
    hostIp = addresses[0].ToString();
}
else
{
    // running locally
    hostIp = Environment.GetEnvironmentVariable("HOST_IP");
}
var saPassword = Environment.GetEnvironmentVariable("SA_PASSWORD");

builder.Services.AddDbContext<LoginDbContext>(options =>
    options.UseSqlServer($"Server={hostIp},1433;Database=UsersDB;User Id=sa;Password={saPassword};TrustServerCertificate=True"));


var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("AllowSpecificOrigin");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run("http://0.0.0.0:4200");
