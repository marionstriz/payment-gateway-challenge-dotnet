using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using PaymentGateway.Api.Services.Clients;
using PaymentGateway.Api.Services.Repositories;
using PaymentGateway.Api.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IPaymentsRepository, InMemoryPaymentsRepository>();
builder.Services.AddSingleton<IBankClient, MountebankBankClient>();

builder.Services.Configure<BankClientSettings>(builder.Configuration.GetSection(nameof(BankClientSettings)));

builder.Services.AddHttpClient(MountebankBankClient.HttpClientName, (sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<BankClientSettings>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

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
