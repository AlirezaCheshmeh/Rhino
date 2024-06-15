using API.Extension;
using API.Extension.HangfireExtensions;
using Application.Extensions;
using Application.Services.TelegramServices.BaseMethods;
using Application.Services.TelegramServices.Interfaces;
using Infrastructure.Extensions;
using Presentation.API.Extension;
using Serilog;


var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
//controller
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//swager
builder.Services.AddSwaggerDependency();
//add Swagger and auth dependency

builder.Services.AddJwtAuthentication(builder.Configuration);

// Add Policy
builder.Services.AddCors(options => options.AddPolicy("myPol", builder =>
{
    builder.SetIsOriginAllowed(x => true)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials();
}));

//add seq logg
builder.Host.UseSerilog(SerilogConfig.ConfigureLogger);
//Log.Logger = new LoggerConfiguration()
//    .MinimumLevel.Information()
//    .WriteTo.Console()
//    .WriteTo.File("Applogs/myBeautifulLog-.text", rollingInterval: RollingInterval.Day)
//    .CreateLogger();//use for file
//builder.Host.UseSerilog();

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

//use files
app.UseStaticFiles();
//logs
app.UseSerilogRequestLogging();
//cors
app.UseCors("myPol");
//init db
app.IntializeDatabase();
//swagger
app.UseSwagger();
app.UseSwaggerUI();
//redirections
app.UseHttpsRedirection();
//authorization
app.UseAuthorization();
//controller
app.MapControllers();

app.Run();
