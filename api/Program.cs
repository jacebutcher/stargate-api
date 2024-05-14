using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.ExceptionLogging; // Import the ExceptionLogging namespace

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("StarbaseApiDatabase");
builder.Services.AddDbContext<StargateContext>(dbContextOptions =>
    dbContextOptions.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Add ExceptionLogging as a singleton service
builder.Services.AddSingleton<ExceptionLogging>();

builder.Services.AddMediatR(cfg =>
{
    cfg.AddRequestPreProcessor<CreateAstronautDutyPreProcessor>();
    cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// (app.Environment.IsDevelopment())
//{
    app.UseSwagger(); // always use Swagger for easier testing
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


