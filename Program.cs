using Microsoft.EntityFrameworkCore;
using ObWebApi3;
using ObWebApi3.Services;
using System;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=Data/ObDatabase.db"));

builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<AlertService>();
builder.Services.AddHttpClient<AlertService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5000");
});

builder.Services.AddSingleton<BlacklistService>();
builder.Services.AddHttpClient<BlacklistService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5000");
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 7080);
});

builder.Logging.AddConsole();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
