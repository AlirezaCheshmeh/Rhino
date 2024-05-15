using API.Extension;
using API.Extension.HangfireExtensions;
using Application.Extensions;
using Application.Services.TelegramServices.BaseMethods;
using Application.Services.TelegramServices.Interfaces;
using Infrastructure.Extensions;
using Presentation.API.Extension;


var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//add Swagger and auth dependency
builder.Services.AddSwaggerDependency();
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add Policy
builder.Services.AddCors(options => options.AddPolicy("myPol", builder =>
{
    builder.SetIsOriginAllowed(x => true)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials();
}));

//Add Services Related To Persistence Infrastructure layer
builder.Services.AddPersistanceInfrestructurelayarServcies(builder.Configuration);
//Add Services Related To Application Layer 
builder.Services.AddApplicationServices(builder.Configuration);

//add cache services
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379"; // Redis connection string
});

//add hangfire services 
builder.Services.AddCustomHangFireServer(builder.Configuration);


var app = builder.Build();

app.UseCors("myPol");
app.IntializeDatabase();



app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
