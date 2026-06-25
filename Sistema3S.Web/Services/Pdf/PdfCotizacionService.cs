using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sistema3S.Web.Data;

namespace Sistema3S.Web.Services.Pdf
{
    public class PdfCotizacionService : IPdfCotizacionService
    {
        private readonly Bd3sContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpClientFactory _httpClientFactory;

        private const string ColorRojo = "#d21927";
        private const string ColorNegro = "#141414";
        private const string ColorTexto = "#0f172a";
        private const string ColorSecundario = "#64748b";
        private const string ColorBorde = "#e2e8f0";
        private const string ColorFondo = "#f8fafc";

        public PdfCotizacionService(
            Bd3sContext context,
            IWebHostEnvironment environment,
            IHttpClientFactory httpClientFactory
        )
        {
            _context = context;
            _environment = environment;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GenerarCotizacionAsync(int idCotizacion)
        {
            var modelo = await ConstruirModeloAsync(idCotizacion);

            var carpeta = Path.Combine(
                _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                "uploads",
                "cotizaciones"
            );

            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            var nombreArchivo = $"cotizacion-COT-{idCotizacion.ToString().PadLeft(5, '0')}.pdf";
            var rutaFisica = Path.Combine(carpeta, nombreArchivo);
            var rutaRelativa = $"/uploads/cotizaciones/{nombreArchivo}";

            var logoPath = ObtenerRutaLogo();

            Document
                .Create(document =>
                {
                    document.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(28);
                        page.DefaultTextStyle(text => text.FontSize(9).FontColor(ColorTexto));

                        page.Header().Element(container => ComponerCabecera(container, modelo, logoPath));

                        page.Content().PaddingTop(14).Column(column =>
                        {
                            column.Spacing(12);

                            column.Item().Element(container => ComponerDatosCliente(container, modelo));
                            column.Item().Element(container => ComponerDetalle(container, modelo));
                            column.Item().Element(container => ComponerResumen(container, modelo));
                            column.Item().Element(container => ComponerObservaciones(container, modelo));
                        });

                        page.Footer().Element(container => ComponerPiePagina(container));
                    });
                })
                .GeneratePdf(rutaFisica);

            var cotizacion = await _context.Cotizacion
                .FirstOrDefaultAsync(c => c.IdCotizacion == idCotizacion);

            if (cotizacion == null)
            {
                throw new InvalidOperationException("No se pudo actualizar la ruta del PDF.");
            }

            cotizacion.ArchivoPdf = rutaRelativa;
            await _context.SaveChangesAsync();

            return rutaRelativa;
        }

        private async Task<CotizacionPdfModel> ConstruirModeloAsync(int idCotizacion)
        {
            var cotizacion = await _context.Cotizacion
                .FirstOrDefaultAsync(c => c.IdCotizacion == idCotizacion);

            if (cotizacion == null)
            {
                throw new InvalidOperationException("Cotización no encontrada.");
            }

            var estado = await _context.EstadoCotizacion
                .Where(e => e.IdEstadoCotizacion == cotizacion.IdEstadoCotizacion)
                .Select(e => e.Nombre)
                .FirstOrDefaultAsync() ?? "Sin estado";

            var empresa = await _context.Empresa
                .Where(e => e.Estado)
                .OrderBy(e => e.IdEmpresa)
                .Select(e => new EmpresaPdfModel
                {
                    NombreComercial = e.NombreComercial,
                    RazonSocial = e.RazonSocial,
                    Ruc = e.Ruc,
                    Rubro = e.Rubro,
                    Correo = e.Correo,
                    Telefono = e.Telefono,
                    Direccion = e.Direccion,
                    SitioWeb = e.SitioWeb
                })
                .FirstOrDefaultAsync();

            empresa ??= new EmpresaPdfModel
            {
                NombreComercial = "3S",
                RazonSocial = "3S - Servicios y Soluciones Superiores",
                Rubro = "Ingeniería y soluciones para la industria"
            };

            var cliente = await ObtenerClienteAsync(cotizacion.IdCliente);

            var detalles = await (
                from d in _context.DetalleCotizacion
                join ec in _context.ElementoCatalogo on d.IdElementoCatalogo equals ec.IdElementoCatalogo
                join te in _context.TipoElemento on ec.IdTipoElemento equals te.IdTipoElemento
                where d.IdCotizacion == idCotizacion
                orderby d.IdDetalleCotizacion
                select new DetallePdfModel
                {
                    TipoElemento = te.Nombre,
                    Nombre = ec.Nombre,
                    Descripcion = ec.Descripcion,
                    ImagenUrl = ec.ImagenUrl,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal,
                    Observacion = d.Observacion
                }
            ).ToListAsync();

            if (detalles.Count == 0)
            {
                throw new InvalidOperationException("La cotización no tiene detalles para generar PDF.");
            }

            foreach (var detalle in detalles)
            {
                detalle.ImagenBytes = await ObtenerImagenAsync(detalle.ImagenUrl);
            }

            return new CotizacionPdfModel
            {
                IdCotizacion = cotizacion.IdCotizacion,
                FechaCotizacion = cotizacion.FechaCotizacion,
                EstadoCotizacion = estado,
                OrigenCotizacion = cotizacion.OrigenCotizacion,
                Subtotal = cotizacion.Subtotal,
                Descuento = cotizacion.Descuento,
                Igv = cotizacion.Igv,
                Total = cotizacion.Total,
                Observacion = cotizacion.Observacion,
                Empresa = empresa,
                Cliente = cliente,
                Detalles = detalles
            };
        }

        private async Task<ClientePdfModel> ObtenerClienteAsync(int idCliente)
        {
            var natural = await (
                from c in _context.Cliente
                join tc in _context.TipoCliente on c.IdTipoCliente equals tc.IdTipoCliente
                join td in _context.TipoDocumento on c.IdTipoDocumento equals td.IdTipoDocumento
                join pn in _context.ClientePersonaNatural on c.IdCliente equals pn.IdCliente
                where c.IdCliente == idCliente
                select new ClientePdfModel
                {
                    Nombre = pn.Nombres + " " + pn.ApellidoPaterno + " " + (pn.ApellidoMaterno ?? ""),
                    EsEmpresa = false,
                    TipoCliente = tc.Nombre,
                    TipoDocumento = td.Nombre,
                    NumeroDocumento = c.NumeroDocumento,
                    Correo = c.Correo,
                    Telefono = c.Telefono,
                    Direccion = c.Direccion
                }
            ).FirstOrDefaultAsync();

            if (natural != null)
            {
                return natural;
            }

            var empresa = await (
                from c in _context.Cliente
                join tc in _context.TipoCliente on c.IdTipoCliente equals tc.IdTipoCliente
                join td in _context.TipoDocumento on c.IdTipoDocumento equals td.IdTipoDocumento
                join ce in _context.ClienteEmpresa on c.IdCliente equals ce.IdCliente
                where c.IdCliente == idCliente
                select new ClientePdfModel
                {
                    Nombre = ce.RazonSocial,
                    NombreComercial = ce.NombreComercial,
                    EsEmpresa = true,
                    TipoCliente = tc.Nombre,
                    TipoDocumento = td.Nombre,
                    NumeroDocumento = c.NumeroDocumento,
                    Correo = c.Correo,
                    Telefono = c.Telefono,
                    Direccion = c.Direccion
                }
            ).FirstOrDefaultAsync();

            return empresa ?? new ClientePdfModel
            {
                Nombre = "Cliente no encontrado",
                EsEmpresa = false,
                TipoCliente = "Sin tipo",
                TipoDocumento = "Documento",
                NumeroDocumento = "-"
            };
        }

        private void ComponerCabecera(
            IContainer container,
            CotizacionPdfModel modelo,
            string? logoPath
        )
        {
            var nombreEmpresa = !string.IsNullOrWhiteSpace(modelo.Empresa.RazonSocial)
                ? modelo.Empresa.RazonSocial
                : "3S - Servicios y Soluciones Superiores";

            var contactos = new List<string>();

            if (!string.IsNullOrWhiteSpace(modelo.Empresa.Correo))
            {
                contactos.Add($"Correo: {modelo.Empresa.Correo}");
            }

            if (!string.IsNullOrWhiteSpace(modelo.Empresa.Telefono))
            {
                contactos.Add($"Teléfono: {modelo.Empresa.Telefono}");
            }

            if (!string.IsNullOrWhiteSpace(modelo.Empresa.SitioWeb))
            {
                contactos.Add($"Web: {modelo.Empresa.SitioWeb}");
            }

            container
                .Border(1)
                .BorderColor(ColorBorde)
                .Background(Colors.White)
                .Column(wrapper =>
                {
                    wrapper.Item().Padding(14).Row(row =>
                    {
                        row.ConstantItem(100).Height(62).Element(c =>
                        {
                            if (!string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath))
                            {
                                c.Padding(2).Image(logoPath).FitArea();
                            }
                            else
                            {
                                c.Border(1)
                                    .BorderColor(ColorBorde)
                                    .AlignCenter()
                                    .AlignMiddle()
                                    .Text("3S")
                                    .FontSize(22)
                                    .Bold()
                                    .FontColor(ColorRojo);
                            }
                        });

                        row.RelativeItem().PaddingLeft(14).Column(column =>
                        {
                            column.Spacing(4);

                            column.Item()
                                .Text(nombreEmpresa)
                                .FontSize(15)
                                .Bold()
                                .FontColor(ColorNegro);

                            column.Item()
                                .Text(modelo.Empresa.Rubro ?? "Ingeniería y soluciones para la industria")
                                .FontSize(9)
                                .FontColor(ColorSecundario);

                            if (contactos.Count > 0)
                            {
                                column.Item()
                                    .Text(string.Join("  |  ", contactos))
                                    .FontSize(8)
                                    .FontColor(ColorSecundario);
                            }

                            if (!string.IsNullOrWhiteSpace(modelo.Empresa.Direccion))
                            {
                                column.Item()
                                    .Text(modelo.Empresa.Direccion)
                                    .FontSize(8)
                                    .FontColor(ColorSecundario);
                            }
                        });

                        row.ConstantItem(145).Column(column =>
                        {
                            column.Spacing(6);

                            column.Item()
                                .AlignRight()
                                .Text($"COT-{modelo.IdCotizacion.ToString().PadLeft(5, '0')}")
                                .FontSize(21)
                                .Bold()
                                .FontColor(ColorNegro);

                            column.Item()
                                .AlignRight()
                                .Text($"Fecha: {modelo.FechaCotizacion:dd/MM/yyyy}")
                                .FontSize(9)
                                .FontColor(ColorSecundario);
                        });
                    });

                    wrapper.Item().Height(4).Background(ColorRojo);
                    wrapper.Item().Height(2).Background(ColorNegro);
                });
        }

        private void ComponerDatosCliente(IContainer container, CotizacionPdfModel modelo)
        {
            var campos = new List<(string Label, string Valor)>();

            if (modelo.Cliente.EsEmpresa)
            {
                campos.Add(("Razón social", modelo.Cliente.Nombre));

                if (!string.IsNullOrWhiteSpace(modelo.Cliente.NombreComercial))
                {
                    campos.Add(("Nombre comercial", modelo.Cliente.NombreComercial));
                }

                campos.Add(("RUC", modelo.Cliente.NumeroDocumento));
            }
            else
            {
                campos.Add(("Cliente", modelo.Cliente.Nombre));
                campos.Add(("Documento", $"{modelo.Cliente.TipoDocumento} {modelo.Cliente.NumeroDocumento}"));
            }

            if (!string.IsNullOrWhiteSpace(modelo.Cliente.Correo))
            {
                campos.Add(("Correo", modelo.Cliente.Correo));
            }

            if (!string.IsNullOrWhiteSpace(modelo.Cliente.Telefono))
            {
                campos.Add(("Teléfono", modelo.Cliente.Telefono));
            }

            if (!string.IsNullOrWhiteSpace(modelo.Cliente.Direccion))
            {
                campos.Add(("Dirección", modelo.Cliente.Direccion));
            }

            container
                .Border(1)
                .BorderColor(ColorBorde)
                .Background(ColorFondo)
                .Padding(12)
                .Column(column =>
                {
                    column.Spacing(8);

                    column.Item()
                        .Text("DATOS DEL CLIENTE")
                        .FontSize(11)
                        .Bold()
                        .FontColor(ColorNegro);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        foreach (var campo in campos)
                        {
                            table.Cell()
                                .PaddingBottom(7)
                                .Element(c => Campo(c, campo.Label, campo.Valor));
                        }

                        if (campos.Count % 2 != 0)
                        {
                            table.Cell().Text("");
                        }
                    });
                });
        }

        private void ComponerDetalle(IContainer container, CotizacionPdfModel modelo)
        {
            container.Column(column =>
            {
                column.Spacing(8);

                column.Item()
                    .Text("DETALLE DE COTIZACIÓN")
                    .FontSize(11)
                    .Bold()
                    .FontColor(ColorNegro);

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(62);
                        columns.RelativeColumn(3);
                        columns.ConstantColumn(44);
                        columns.ConstantColumn(76);
                        columns.ConstantColumn(82);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CeldaCabecera).Text("Imagen");
                        header.Cell().Element(CeldaCabecera).Text("Producto / Servicio");
                        header.Cell().Element(CeldaCabecera).AlignCenter().Text("Cant.");
                        header.Cell().Element(CeldaCabecera).AlignRight().Text("Precio c/IGV");
                        header.Cell().Element(CeldaCabecera).AlignRight().Text("Subtotal");
                    });

                    foreach (var item in modelo.Detalles)
                    {
                        table.Cell().Element(CeldaCuerpo).Height(58).Element(c => ComponerImagenDetalle(c, item));

                        table.Cell().Element(CeldaCuerpo).Column(c =>
                        {
                            c.Spacing(2);

                            c.Item().Text(item.Nombre).Bold().FontSize(9).FontColor(ColorTexto);
                            c.Item().Text(item.TipoElemento).FontSize(8).FontColor(ColorSecundario);

                            if (!string.IsNullOrWhiteSpace(item.Observacion))
                            {
                                c.Item().Text($"Obs.: {item.Observacion}").FontSize(7).FontColor(ColorSecundario);
                            }
                        });

                        table.Cell().Element(CeldaCuerpo).AlignCenter().AlignMiddle().Text(item.Cantidad.ToString()).Bold();
                        table.Cell().Element(CeldaCuerpo).AlignRight().AlignMiddle().Text($"S/ {item.PrecioUnitario:0.00}");
                        table.Cell().Element(CeldaCuerpo).AlignRight().AlignMiddle().Text($"S/ {item.Subtotal:0.00}").Bold();
                    }
                });
            });
        }

        private void ComponerResumen(IContainer container, CotizacionPdfModel modelo)
        {
            var tieneDescuento = modelo.Descuento > 0;
            var valorVenta = Math.Round(modelo.Total / 1.18m, 2);

            container.Row(row =>
            {
                row.RelativeItem()
                    .Background(ColorFondo)
                    .Border(1)
                    .BorderColor(ColorBorde)
                    .Padding(12)
                    .Column(column =>
                    {
                        column.Spacing(5);

                        column.Item()
                            .Text("CONDICIONES COMERCIALES")
                            .FontSize(10)
                            .Bold()
                            .FontColor(ColorNegro);

                        column.Item()
                            .Text("Los precios indicados incluyen IGV y pueden estar sujetos a validación técnica, disponibilidad de stock o visita de evaluación.")
                            .FontSize(8)
                            .FontColor(ColorSecundario);

                        column.Item()
                            .Text("La cotización tiene una vigencia referencial de 7 días calendario, salvo acuerdo distinto con el cliente.")
                            .FontSize(8)
                            .FontColor(ColorSecundario);
                    });

                row.ConstantItem(18);

                row.ConstantItem(220)
                    .Border(1)
                    .BorderColor(ColorBorde)
                    .Padding(12)
                    .Column(column =>
                    {
                        column.Spacing(7);

                        FilaTotal(column, "Subtotal con IGV", modelo.Subtotal);

                        if (tieneDescuento)
                        {
                            FilaTotal(column, "Descuento", modelo.Descuento);
                        }

                        FilaTotal(column, "Valor venta", valorVenta);
                        FilaTotal(column, "IGV incluido 18%", modelo.Igv);

                        column.Item().LineHorizontal(1).LineColor(ColorBorde);

                        column.Item().Background(ColorNegro).Padding(9).Row(totalRow =>
                        {
                            totalRow.RelativeItem().Text("TOTAL").FontColor(Colors.White).Bold().FontSize(11);
                            totalRow.AutoItem().Text($"S/ {modelo.Total:0.00}").FontColor(Colors.White).Bold().FontSize(13);
                        });
                    });
            });
        }

        private void ComponerObservaciones(IContainer container, CotizacionPdfModel modelo)
        {
            container
                .Border(1)
                .BorderColor(ColorBorde)
                .Padding(12)
                .Column(column =>
                {
                    column.Spacing(5);

                    column.Item()
                        .Text("OBSERVACIONES")
                        .FontSize(10)
                        .Bold()
                        .FontColor(ColorNegro);

                    column.Item()
                        .Text(string.IsNullOrWhiteSpace(modelo.Observacion)
                            ? "Sin observaciones adicionales."
                            : modelo.Observacion)
                        .FontSize(8)
                        .FontColor(ColorSecundario);
                });
        }

        private void ComponerPiePagina(IContainer container)
        {
            container
                .PaddingTop(8)
                .Column(column =>
                {
                    column.Item().LineHorizontal(1).LineColor(ColorBorde);

                    column.Item().PaddingTop(6).Row(row =>
                    {
                        row.RelativeItem()
                            .Text("Documento generado automáticamente por el Sistema Web 3S.")
                            .FontSize(8)
                            .FontColor(ColorSecundario);

                        row.AutoItem().Text(text =>
                        {
                            text.Span("Página ").FontSize(8).FontColor(ColorSecundario);
                            text.CurrentPageNumber().FontSize(8).FontColor(ColorSecundario);
                            text.Span(" de ").FontSize(8).FontColor(ColorSecundario);
                            text.TotalPages().FontSize(8).FontColor(ColorSecundario);
                        });
                    });
                });
        }

        private void ComponerImagenDetalle(IContainer container, DetallePdfModel item)
        {
            if (item.ImagenBytes != null &&
                item.ImagenBytes.Length > 0 &&
                EsImagenCompatible(item.ImagenBytes))
            {
                try
                {
                    container
                        .Padding(3)
                        .Image(item.ImagenBytes)
                        .FitArea();

                    return;
                }
                catch
                {
                    // Si QuestPDF no puede decodificar la imagen, se muestra el cuadro genérico.
                }
            }

            container
                .Border(1)
                .BorderColor(ColorBorde)
                .Background(ColorFondo)
                .AlignCenter()
                .AlignMiddle()
                .Text("Sin imagen")
                .FontSize(7)
                .FontColor(ColorSecundario);
        }

        private static IContainer CeldaCabecera(IContainer container)
        {
            return container
                .Background(ColorNegro)
                .PaddingVertical(7)
                .PaddingHorizontal(6)
                .DefaultTextStyle(text => text.FontColor(Colors.White).FontSize(8).Bold());
        }

        private static IContainer CeldaCuerpo(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(ColorBorde)
                .PaddingVertical(6)
                .PaddingHorizontal(6);
        }

        private void Campo(IContainer container, string label, string valor)
        {
            container.Column(column =>
            {
                column.Spacing(2);

                column.Item().Text(label).FontSize(7).FontColor(ColorSecundario);
                column.Item().Text(string.IsNullOrWhiteSpace(valor) ? "-" : valor).FontSize(9).Bold().FontColor(ColorTexto);
            });
        }

        private void FilaTotal(ColumnDescriptor column, string label, decimal valor)
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text(label).FontSize(8).FontColor(ColorSecundario);
                row.AutoItem().Text($"S/ {valor:0.00}").FontSize(9).Bold().FontColor(ColorTexto);
            });
        }

        private async Task<byte[]?> ObtenerImagenAsync(string? imagenUrl)
        {
            if (string.IsNullOrWhiteSpace(imagenUrl))
            {
                return null;
            }

            try
            {
                if (Uri.IsWellFormedUriString(imagenUrl, UriKind.Absolute))
                {
                    var client = _httpClientFactory.CreateClient();
                    client.Timeout = TimeSpan.FromSeconds(6);

                    using var response = await client.GetAsync(imagenUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    var contentType = response.Content.Headers.ContentType?.MediaType;

                    if (!EsContentTypeImagenCompatible(contentType))
                    {
                        return null;
                    }

                    var bytes = await response.Content.ReadAsByteArrayAsync();

                    if (!EsImagenCompatible(bytes))
                    {
                        return null;
                    }

                    return bytes;
                }

                var rutaRelativa = imagenUrl
                    .TrimStart('/')
                    .Replace("/", Path.DirectorySeparatorChar.ToString());

                var rutaFisica = Path.Combine(
                    _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                    rutaRelativa
                );

                if (!File.Exists(rutaFisica))
                {
                    return null;
                }

                var extension = Path.GetExtension(rutaFisica).ToLower();

                if (extension != ".png" &&
                    extension != ".jpg" &&
                    extension != ".jpeg")
                {
                    return null;
                }

                var archivoBytes = await File.ReadAllBytesAsync(rutaFisica);

                if (!EsImagenCompatible(archivoBytes))
                {
                    return null;
                }

                return archivoBytes;
            }
            catch
            {
                return null;
            }
        }

        private bool EsContentTypeImagenCompatible(string? contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return false;
            }

            contentType = contentType.ToLower();

            return contentType == "image/png" ||
                   contentType == "image/jpeg" ||
                   contentType == "image/jpg";
        }

        private bool EsImagenCompatible(byte[] bytes)
        {
            if (bytes.Length < 8)
            {
                return false;
            }

            var esPng =
                bytes[0] == 0x89 &&
                bytes[1] == 0x50 &&
                bytes[2] == 0x4E &&
                bytes[3] == 0x47 &&
                bytes[4] == 0x0D &&
                bytes[5] == 0x0A &&
                bytes[6] == 0x1A &&
                bytes[7] == 0x0A;

            var esJpg =
                bytes[0] == 0xFF &&
                bytes[1] == 0xD8;

            return esPng || esJpg;
        }

        private string? ObtenerRutaLogo()
        {
            var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var rutas = new[]
            {
                Path.Combine(webRoot, "assets", "images", "logo-3s.png"),
                Path.Combine(webRoot, "images", "logo-3s.png"),
                Path.Combine(webRoot, "logo-3s.png")
            };

            return rutas.FirstOrDefault(File.Exists);
        }

        private class CotizacionPdfModel
        {
            public int IdCotizacion { get; set; }
            public DateTime FechaCotizacion { get; set; }
            public string EstadoCotizacion { get; set; } = string.Empty;
            public string OrigenCotizacion { get; set; } = string.Empty;
            public decimal Subtotal { get; set; }
            public decimal Descuento { get; set; }
            public decimal Igv { get; set; }
            public decimal Total { get; set; }
            public string? Observacion { get; set; }

            public EmpresaPdfModel Empresa { get; set; } = new();
            public ClientePdfModel Cliente { get; set; } = new();
            public List<DetallePdfModel> Detalles { get; set; } = new();
        }

        private class EmpresaPdfModel
        {
            public string NombreComercial { get; set; } = string.Empty;
            public string RazonSocial { get; set; } = string.Empty;
            public string? Ruc { get; set; }
            public string? Rubro { get; set; }
            public string? Correo { get; set; }
            public string? Telefono { get; set; }
            public string? Direccion { get; set; }
            public string? SitioWeb { get; set; }
        }

        private class ClientePdfModel
        {
            public string Nombre { get; set; } = string.Empty;
            public string? NombreComercial { get; set; }
            public bool EsEmpresa { get; set; }
            public string TipoCliente { get; set; } = string.Empty;
            public string TipoDocumento { get; set; } = string.Empty;
            public string NumeroDocumento { get; set; } = string.Empty;
            public string? Correo { get; set; }
            public string? Telefono { get; set; }
            public string? Direccion { get; set; }
        }

        private class DetallePdfModel
        {
            public string TipoElemento { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string? Descripcion { get; set; }
            public string? ImagenUrl { get; set; }
            public byte[]? ImagenBytes { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public decimal Subtotal { get; set; }
            public string? Observacion { get; set; }
        }
    }
}