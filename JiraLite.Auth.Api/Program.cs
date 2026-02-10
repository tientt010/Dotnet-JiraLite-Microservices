using JiraLite.Auth.Api.Apis;
using JiraLite.Auth.Api.Bootstraping;
using JiraLite.Auth.Api.Services;
using JiraLite.Auth.Infrastructure.Data;
using JiraLite.Auth.Infrastructure.Entities;
using JiraLite.Share.Dtos.Auth;
using JiraLite.Share.Settings;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapAuthApi().MapInternalApi();

app.Run();