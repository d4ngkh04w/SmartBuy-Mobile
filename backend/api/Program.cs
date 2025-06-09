using api.Configurations;
using api.Helpers;
using api.Utils;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

ConfigHelper.Initialize(builder.Configuration);

builder.Services.AddDatabaseConfiguration();
builder.Services.AddCorsConfiguration();
builder.Services.AddApiConfiguration();
builder.Services.AddAuthenticationConfiguration();
builder.Services.AddRateLimitConfiguration();
builder.Services.AddMemoryCacheConfiguration();
builder.Services.AddApplicationServices();

var app = builder.Build();

app.ConfigureMiddlewarePipeline();

app.Run();
