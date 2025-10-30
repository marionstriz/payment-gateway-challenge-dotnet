using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using PaymentGateway.Api.Services.Clients;
using PaymentGateway.Api.Services.Repositories;
using PaymentGateway.Api.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.UseInlineDefinitionsForEnums();
});

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

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
