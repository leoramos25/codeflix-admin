using Codeflix.Catalog.Api.Configurations;

var builder = WebApplication.CreateBuilder(args);
builder
    .Services.AddControllersConfiguration()
    .AddUseCases()
    .AddApiConnections(builder.Configuration);
var app = builder.Build();
app.UseDocumentation();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program;
