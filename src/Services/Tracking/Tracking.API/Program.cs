using JiraLite.Api.Bootstraping;
using Tracking.Api.Apis;
using Tracking.API.Authorization.Extensions;
using Tracking.Application;
using Tracking.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);

builder.AddTrackingAuthorization();

builder.Services.AddApplicationServices();

builder.Services.AddInfrastructureServices(builder.Configuration);


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapTrackingApi();

app.Run();