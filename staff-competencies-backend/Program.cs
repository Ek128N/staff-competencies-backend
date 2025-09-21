using Serilog;
using staff_competencies_backend.Middlewares;
using staff_competencies_backend.Utils;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt",
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
    .CreateLogger();

builder.Host.UseSerilog();

var services = builder.Services;

services.AddServices(builder.Configuration, builder.Environment);
services.AddControllers();
services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => { options.SwaggerEndpoint("/openapi/v1.json", "test"); });
}

app.UseHttpsRedirection();
app.UseErrorHandlingMiddleware();
app.UseAuthorization();
app.MapControllers();

app.Run();