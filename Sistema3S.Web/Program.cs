using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using Sistema3S.Web.Data;
using Sistema3S.Web.Services.Implementations;
using Sistema3S.Web.Services.Interfaces;
using Sistema3S.Web.Services.Pdf;
using Sistema3S.Web.Services.Seguridad;

var builder = WebApplication.CreateBuilder(args);

// Licencia QuestPDF
QuestPDF.Settings.License = LicenseType.Community;

// MVC / Controllers
builder.Services.AddControllersWithViews();

// DbContext - SQL Server
builder.Services.AddDbContext<Bd3sContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BD_3S"))
);

// Configuración JWT
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException("La clave JWT no está configurada correctamente.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Sistema3S";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "Sistema3SAdmin";

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

// Servicios de seguridad / autenticación
builder.Services.AddScoped<PasswordHashService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Servicios de negocio
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<ICotizacionService, CotizacionService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IPublicoService, PublicoService>();

builder.Services.AddHttpClient<IConsultaDocumentoService, ConsultaDocumentoService>();
builder.Services.AddHttpClient<IProveedorService, ProveedorService>();

builder.Services.AddScoped<IInventarioService, InventarioService>();

builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddScoped<IPdfCompraService, PdfCompraService>();
builder.Services.AddScoped<IExcelCompraService, ExcelCompraService>();

builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<IPdfVentaService, PdfVentaService>();
builder.Services.AddScoped<IExcelVentaService, ExcelVentaService>();

builder.Services.AddScoped<ICajaService, CajaService>();

builder.Services.AddScoped<PasswordHashService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// Servicios para PDF / correo / WhatsApp
builder.Services.AddHttpClient();
builder.Services.AddScoped<IPdfCotizacionService, PdfCotizacionService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();

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

// Primero autenticación, luego autorización
app.UseAuthentication();

app.UseAuthorization();

// Controladores API
app.MapControllers();

// Ruta MVC por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();