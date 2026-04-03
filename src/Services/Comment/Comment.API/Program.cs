using Comment.API.Apis;
using Comment.API.Extensions;
using Comment.Application;
using Comment.Infrastructure;
using Comment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddApiServices(builder.Configuration)
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapCommentsApi();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CommentDbContext>();
    dbContext.Database.Migrate();
}
app.Run();
