using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Sistema3S.Web.Data;
using Sistema3S.Web.Services.Implementations;
using Sistema3S.Web.Services.Interfaces;
using Sistema3S.Web.Services.Pdf;

var builder = WebApplication.CreateBuilder(args);

// Licencia QuestPDF
QuestPDF.Settings.License = LicenseType.Community;

// MVC / Controllers
builder.Services.AddControllersWithViews();

// DbContext - SQL Server
builder.Services.AddDbContext<Bd3sContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BD_3S"))
);

// Servicios de negocio
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<ICotizacionService, CotizacionService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddHttpClient<IConsultaDocumentoService, ConsultaDocumentoService>();

// Servicios para PDF
builder.Services.AddHttpClient();
builder.Services.AddScoped<IPdfCotizacionService, PdfCotizacionService>();

// CORS para Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Necesario para abrir PDFs, imágenes y archivos desde wwwroot
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AngularPolicy");

app.UseAuthorization();

// Controladores API
app.MapControllers();

// Ruta MVC por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();