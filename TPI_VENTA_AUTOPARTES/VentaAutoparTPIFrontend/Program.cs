
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Habilita el acceso a los archivos estáticos.
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
