using DorelAppBackend.Middleware;
using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Services;
using DorelAppBackend.Services.Implementation;
using DorelAppBackend.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);


builder.WebHost.UseKestrel(options =>
{
    options.Listen(IPAddress.Any, 4500);
    options.Listen(IPAddress.Any, 4200, listenOptions =>
    {
        try
        {
            // docker
            listenOptions.UseHttps("/app/backendcertificate.pfx"); //Environment.GetEnvironmentVariable("PFX_PASS")
        }
        catch
        {
            // local
            listenOptions.UseHttps("C:/Users/Adi/Desktop/certs/backendcertificate.pfx");
        }
        
    });
});




// Add services to the container.


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins("http://localhost")
                   .AllowAnyHeader()
                   .AllowAnyMethod().AllowCredentials();
        });
});

builder.Services.AddTransient<ILoginService, LoginService>();
builder.Services.AddTransient<IRedisService, RedisService>();
builder.Services.AddTransient<IRedisCacheService, RedisCacheService>();
builder.Services.AddTransient<IMailService, MailService>();
builder.Services.AddTransient<IPasswordHashService, PasswordHashService>();
builder.Services.AddTransient<IDataService, DataService>();
builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
builder.Services.AddTransient<IReviewService, ReviewService>();
builder.Services.AddTransient<IChatService, ChatService>();

// Add configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false);
builder.Services.AddControllers();
builder.Services.AddSignalR();
// Add DbContext
string hostIp = "";

try
{
    IPAddress[] addresses = Dns.GetHostAddresses("host.docker.internal");
    if (addresses.Length > 0)
    {
        // we are running in docker
        hostIp = addresses[0].ToString();
    }
}
catch 
{

    if (String.IsNullOrEmpty(hostIp))
    {
        hostIp = "10.132.0.2";
    }
}


var saPassword = Environment.GetEnvironmentVariable("SA_PASSWORD");

builder.Services.AddDbContext<DorelDbContext>(options =>
    options.UseSqlServer($"Server={hostIp},1433;Database=DorelDB;User Id=sa;Password={saPassword};TrustServerCertificate=True"));


var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("AllowSpecificOrigin");
// Configure the HTTP request pipeline.

app.MapHub<ChatHub>("/chatHub").RequireCors("AllowSpecificOrigin");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
