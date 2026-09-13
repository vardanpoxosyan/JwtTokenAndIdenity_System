using ExceptionHandling.Data;
using ExceptionHandling.DepandancyInjection;
using ExceptionHandling.Exceptions;
using ExceptionHandling.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.JwtAuthenticationService(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddTodatabase(builder.Configuration);
builder.Services.AddServices();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseHttpsRedirection();
app.UseMiddleware<ExceptionMiddleWare>();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();


app.Run();
