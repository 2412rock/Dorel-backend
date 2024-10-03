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
});




// Add services to the container.


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalHost",
        builder =>
        {
            builder.WithOrigins("http://localhost")
                   .AllowAnyHeader()
                   .AllowAnyMethod().AllowCredentials();
        });
    options.AddPolicy("AllowAnyOrigin",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });

    options.AddPolicy("AllowDorelOrigin",
        builder =>
        {
            builder.WithOrigins("https://dorelapp.xyz")
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
builder.Services.AddTransient<IAccessLogsService, AccessLogsService>();

// Add configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false);
builder.Services.AddControllers();
builder.Services.AddSignalR();
// Add DbContext
string hostIp = "192.168.1.159";



var saPassword = Environment.GetEnvironmentVariable("SA_PASSWORD");

builder.Services.AddDbContext<DorelDbContext>(options =>
    options.UseSqlServer($"Server={hostIp},1501;Database=DorelDB;User Id=sa;Password={saPassword};TrustServerCertificate=True"));


var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

/*app.UseCors("AllowLocalHost");
app.MapHub<ChatHub>("/chatHub").RequireCors("AllowLocalHost");*/



app.MapHub<ChatHub>("/chatHub").RequireCors("AllowDorelOrigin");
app.UseCors("AllowDorelOrigin");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();