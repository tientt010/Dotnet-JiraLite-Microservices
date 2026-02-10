using JiraLite.Api.Apis;
using JiraLite.Api.Bootstraping;

var builder = WebApplication.CreateBuilder(args);
builder.AddApplicationServices();


var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// app.UseAuthentication();
// app.UseAuthorization();

app.MapJiraLiteApi();

app.Run();
