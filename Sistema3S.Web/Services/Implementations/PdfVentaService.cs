using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sistema3S.Web.DTOs.Venta;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Services.Implementations
{
    public class PdfVentaService : IPdfVentaService
    {
        private readonly IVentaService _ventaService;
        private readonly IWebHostEnvironment _environment;

        public PdfVentaService(
            IVentaService ventaService,
            IWebHostEnvironment environment
        )
        {
            _ventaService = ventaService;
            _environment = environment;
        }

        public async Task<byte[]> GenerarPdfVentaAsync(int idVenta)
        {
            var venta = await _ventaService.ObtenerDetalleAsync(idVenta);

            if (venta == null)
            {
                throw new InvalidOperationException("No se encontró la venta seleccionada.");
            }

            var logoPath = ObtenerRutaLogo();

            using var memoryStream = new MemoryStream();

            Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(32);
                    page.DefaultTextStyle(style => style.FontSize(9));

                    page.Header().Element(header =>
                    {
                        header
                            .Background("#07111B")
                            .Padding(16)
                            .Row(row =>
                            {
                                row.RelativeItem().Row(inner =>
                                {
                                    inner.ConstantItem(64)
                                        .Height(64)
                                        .Element(logoContainer =>
                                        {
                                            if (!string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath))
                                            {
                                                logoContainer
                                                    .Background(Colors.White)
                                                    .Padding(6)
                                                    .Image(logoPath)
                                                    .FitArea();
                                            }
                                            else
                                            {
                                                logoContainer
                                                    .Background(Colors.White)
                                                    .Padding(6)
                                                    .AlignCenter()
                                                    .AlignMiddle()
                                                    .Text("3S")
                                                    .FontSize(18)
                                                    .Bold()
                                                    .FontColor("#D81920");
                                            }
                                        });

                                    inner.RelativeItem()
                                        .PaddingLeft(12)
                                        .Column(col =>
                                        {
                                            col.Item().Text("Sistema 3S")
                                                .FontSize(22)
                                                .Bold()
                                                .FontColor(Colors.White);

                                            col.Item().Text("Comprobante interno de venta")
                                                .FontSize(11)
                                                .FontColor("#E5E7EB");

                                            col.Item().Text("Documento generado para control comercial interno")
                                                .FontSize(9)
                                                .FontColor("#CBD5E1");
                                        });
                                });

                                row.ConstantItem(185)
                                    .AlignRight()
                                    .Column(col =>
                                    {
                                        col.Item().AlignRight().Text(venta.TipoComprobante)
                                            .FontSize(16)
                                            .Bold()
                                            .FontColor(Colors.White);

                                        col.Item().AlignRight().Text(venta.DocumentoCompleto)
                                            .FontSize(13)
                                            .Bold()
                                            .FontColor("#FEE2E2");

                                        col.Item().AlignRight().Text(venta.FechaEmision.ToString("dd/MM/yyyy"))
                                            .FontSize(10)
                                            .FontColor("#E5E7EB");

                                        col.Item().AlignRight().Text(venta.EstadoPago)
                                            .FontSize(10)
                                            .Bold()
                                            .FontColor("#DCFCE7");
                                    });
                            });
                    });

                    page.Content().PaddingTop(18).Column(col =>
                    {
                        col.Spacing(12);

                        col.Item().Text("Registro de Venta")
                            .FontSize(24)
                            .Bold()
                            .FontColor("#D81920");

                        col.Item().Element(container =>
                        {
                            CrearCajaInformacion(
                                container,
                                "Datos del cliente",
                                new List<(string Label, string Value)>
                                {
                                    ("Cliente", venta.Cliente),
                                    ("Tipo documento", venta.TipoDocumentoCliente),
                                    ("Documento", venta.DocumentoCliente),
                                    ("Teléfono", venta.TelefonoCliente ?? "-"),
                                    ("Correo", venta.CorreoCliente ?? "-"),
                                    ("Dirección", venta.DireccionCliente ?? "-")
                                }
                            );
                        });

                        col.Item().Element(container =>
                        {
                            CrearCajaInformacion(
                                container,
                                "Datos de la venta",
                                new List<(string Label, string Value)>
                                {
                                    ("Venta N°", venta.IdVenta.ToString()),
                                    ("Origen", venta.OrigenVenta),
                                    ("Comprobante", venta.TipoComprobante),
                                    ("Documento", venta.DocumentoCompleto),
                                    ("Fecha venta", venta.FechaVenta.ToString("dd/MM/yyyy HH:mm")),
                                    ("Estado venta", venta.EstadoVenta),
                                    ("Estado pago", venta.EstadoPago)
                                }
                            );
                        });

                        col.Item().PaddingTop(4).Text("Detalle de productos y servicios")
                            .FontSize(12)
                            .Bold()
                            .FontColor("#07111B");

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(70);
                                columns.RelativeColumn();
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(85);
                                columns.ConstantColumn(85);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("Código");
                                header.Cell().Element(HeaderCell).Text("Descripción");
                                header.Cell().Element(HeaderCell).Text("Tipo");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Cant.");
                                header.Cell().Element(HeaderCell).AlignRight().Text("P. Unit. c/IGV");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Importe");
                            });

                            foreach (var item in venta.Detalles)
                            {
                                table.Cell().Element(BodyCell).Text(item.CodigoProducto ?? "-");
                                table.Cell().Element(BodyCell).Text(item.Elemento);
                                table.Cell().Element(BodyCell).Text(item.TipoElemento);
                                table.Cell().Element(BodyCell).AlignRight().Text(item.Cantidad.ToString());
                                table.Cell().Element(BodyCell).AlignRight().Text($"S/ {item.PrecioUnitario:N2}");
                                table.Cell().Element(BodyCell).AlignRight().Text($"S/ {item.Subtotal:N2}");
                            }
                        });

                        col.Item().Row(row =>
                        {
                            row.RelativeItem();

                            row.ConstantItem(250).Column(totals =>
                            {
                                AgregarLineaTotal(totals, "Subtotal sin IGV", venta.Subtotal, false);
                                AgregarLineaTotal(totals, "IGV 18%", venta.Igv, false);
                                AgregarLineaTotal(totals, "Total", venta.Total, true);
                                AgregarLineaTotal(totals, "Pagado", venta.TotalPagado, false);
                                AgregarLineaTotal(totals, "Saldo", venta.SaldoPendiente, false);
                            });
                        });

                        col.Item().PaddingTop(6).Text("Pagos registrados")
                            .FontSize(12)
                            .Bold()
                            .FontColor("#07111B");

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(105);
                                columns.ConstantColumn(105);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("Método");
                                header.Cell().Element(HeaderCell).Text("Fecha");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Monto");
                            });

                            if (venta.Pagos.Count == 0)
                            {
                                table.Cell()
                                    .ColumnSpan(3)
                                    .Element(BodyCell)
                                    .Text("No hay pagos registrados.");
                            }
                            else
                            {
                                foreach (var pago in venta.Pagos)
                                {
                                    table.Cell().Element(BodyCell).Text(pago.MetodoPago);
                                    table.Cell().Element(BodyCell).Text(pago.FechaPago.ToString("dd/MM/yyyy"));
                                    table.Cell().Element(BodyCell).AlignRight().Text($"S/ {pago.MontoPagado:N2}");
                                }
                            }
                        });

                        if (venta.Cuotas.Count > 0)
                        {
                            col.Item().PaddingTop(6).Text("Cuotas programadas")
                                .FontSize(12)
                                .Bold()
                                .FontColor("#07111B");

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(50);
                                    columns.ConstantColumn(110);
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(100);
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCell).Text("N°");
                                    header.Cell().Element(HeaderCell).Text("Vencimiento");
                                    header.Cell().Element(HeaderCell).AlignRight().Text("Monto");
                                    header.Cell().Element(HeaderCell).AlignRight().Text("Pagado");
                                    header.Cell().Element(HeaderCell).Text("Estado");
                                });

                                foreach (var cuota in venta.Cuotas)
                                {
                                    table.Cell().Element(BodyCell).Text(cuota.NumeroCuota.ToString());
                                    table.Cell().Element(BodyCell).Text(cuota.FechaVencimiento.ToString("dd/MM/yyyy"));
                                    table.Cell().Element(BodyCell).AlignRight().Text($"S/ {cuota.MontoCuota:N2}");
                                    table.Cell().Element(BodyCell).AlignRight().Text($"S/ {cuota.MontoPagado:N2}");
                                    table.Cell().Element(BodyCell).Text(cuota.EstadoCuota);
                                }
                            });
                        }

                        if (!string.IsNullOrWhiteSpace(venta.Observacion))
                        {
                            col.Item().Element(container =>
                            {
                                CrearCajaInformacion(
                                    container,
                                    "Observación",
                                    new List<(string Label, string Value)>
                                    {
                                        ("Detalle", venta.Observacion ?? "-")
                                    }
                                );
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text(
                        "Documento interno generado por Sistema 3S. Este documento no reemplaza la emisión oficial ante SUNAT."
                    ).FontSize(8).FontColor("#64748B");
                });
            }).GeneratePdf(memoryStream);

            return memoryStream.ToArray();
        }

        public byte[] GenerarPdfReporteVentas(
            IEnumerable<ReporteVentaDto> ventas,
            DateTime? fechaInicio,
            DateTime? fechaFin
        )
        {
            var lista = ventas.ToList();
            var logoPath = ObtenerRutaLogo();

            using var memoryStream = new MemoryStream();

            Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(28);
                    page.DefaultTextStyle(style => style.FontSize(8));

                    page.Header().Element(header =>
                    {
                        header
                            .Background("#07111B")
                            .Padding(14)
                            .Row(row =>
                            {
                                row.ConstantItem(58).Height(58).Element(logoContainer =>
                                {
                                    if (!string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath))
                                    {
                                        logoContainer
                                            .Background(Colors.White)
                                            .Padding(5)
                                            .Image(logoPath)
                                            .FitArea();
                                    }
                                    else
                                    {
                                        logoContainer
                                            .Background(Colors.White)
                                            .AlignCenter()
                                            .AlignMiddle()
                                            .Text("3S")
                                            .FontSize(17)
                                            .Bold()
                                            .FontColor("#D81920");
                                    }
                                });

                                row.RelativeItem().PaddingLeft(12).Column(col =>
                                {
                                    col.Item().Text("Reporte de ventas")
                                        .FontSize(20)
                                        .Bold()
                                        .FontColor(Colors.White);

                                    col.Item().Text(ObtenerTextoRango(fechaInicio, fechaFin))
                                        .FontSize(10)
                                        .FontColor("#E5E7EB");

                                    col.Item().Text($"Total de registros: {lista.Count}")
                                        .FontSize(9)
                                        .FontColor("#CBD5E1");
                                });

                                row.ConstantItem(180).AlignRight().Column(col =>
                                {
                                    col.Item().AlignRight().Text("Sistema 3S")
                                        .FontSize(14)
                                        .Bold()
                                        .FontColor(Colors.White);

                                    col.Item().AlignRight().Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                                        .FontSize(9)
                                        .FontColor("#E5E7EB");
                                });
                            });
                    });

                    page.Content().PaddingTop(14).Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(34);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(85);
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(72);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("ID");
                                header.Cell().Element(HeaderCell).Text("Fecha");
                                header.Cell().Element(HeaderCell).Text("Documento");
                                header.Cell().Element(HeaderCell).Text("Cliente");
                                header.Cell().Element(HeaderCell).Text("Origen");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Subtotal sin IGV");
                                header.Cell().Element(HeaderCell).AlignRight().Text("IGV");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Total");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Pagado");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Saldo");
                                header.Cell().Element(HeaderCell).Text("Estado");
                            });

                            foreach (var item in lista)
                            {
                                table.Cell().Element(BodyCell).Text(item.IdVenta.ToString());
                                table.Cell().Element(BodyCell).Text(item.FechaVenta.ToString("dd/MM/yyyy"));
                                table.Cell().Element(BodyCell).Text(item.DocumentoCompleto);
                                table.Cell().Element(BodyCell).Text(item.Cliente);
                                table.Cell().Element(BodyCell).Text(item.OrigenVenta);
                                table.Cell().Element(BodyCell).AlignRight().Text($"S/ {item.Subtotal:N2}");
                                table.Cell().Element(BodyCell).AlignRight().Text($"S/ {item.Igv:N2}");
                                table.Cell().Element(BodyCell).AlignRight().Text($"S/ {item.Total:N2}");
                                table.Cell().Element(BodyCell).AlignRight().Text($"S/ {item.TotalPagado:N2}");
                                table.Cell().Element(BodyCell).AlignRight().Text($"S/ {item.SaldoPendiente:N2}");
                                table.Cell().Element(BodyCell).Text(item.EstadoPago);
                            }
                        });

                        col.Item().Row(row =>
                        {
                            row.RelativeItem();

                            row.ConstantItem(300).Column(totals =>
                            {
                                AgregarLineaTotal(totals, "Total ventas", lista.Sum(x => x.Total), true);
                                AgregarLineaTotal(totals, "Total pagado", lista.Sum(x => x.TotalPagado), false);
                                AgregarLineaTotal(totals, "Saldo pendiente", lista.Sum(x => x.SaldoPendiente), false);
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text(
                        "Reporte interno generado por Sistema 3S."
                    ).FontSize(8).FontColor("#64748B");
                });
            }).GeneratePdf(memoryStream);

            return memoryStream.ToArray();
        }

        private string? ObtenerRutaLogo()
        {
            var posiblesRutas = new List<string>();

            if (!string.IsNullOrWhiteSpace(_environment.WebRootPath))
            {
                posiblesRutas.Add(Path.Combine(
                    _environment.WebRootPath,
                    "assets",
                    "images",
                    "logo-3s.png"
                ));
            }

            posiblesRutas.Add(Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "assets",
                "images",
                "logo-3s.png"
            ));

            return posiblesRutas.FirstOrDefault(File.Exists);
        }

        private static string ObtenerTextoRango(DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                return $"Del {fechaInicio.Value:dd/MM/yyyy} al {fechaFin.Value:dd/MM/yyyy}";
            }

            if (fechaInicio.HasValue)
            {
                return $"Desde {fechaInicio.Value:dd/MM/yyyy}";
            }

            if (fechaFin.HasValue)
            {
                return $"Hasta {fechaFin.Value:dd/MM/yyyy}";
            }

            return "Todas las ventas registradas";
        }

        private static void CrearCajaInformacion(
            IContainer container,
            string titulo,
            List<(string Label, string Value)> datos
        )
        {
            container
                .Border(1)
                .BorderColor("#E5E7EB")
                .Padding(12)
                .Column(col =>
                {
                    col.Item().Text(titulo)
                        .FontSize(12)
                        .Bold()
                        .FontColor("#07111B");

                    col.Item().PaddingTop(6).Column(info =>
                    {
                        foreach (var dato in datos)
                        {
                            info.Item().PaddingBottom(3).Text(text =>
                            {
                                text.Span($"{dato.Label}: ").Bold();
                                text.Span(dato.Value);
                            });
                        }
                    });
                });
        }

        private static void AgregarLineaTotal(
            ColumnDescriptor totals,
            string label,
            decimal value,
            bool destacado
        )
        {
            totals.Item()
                .BorderBottom(1)
                .BorderColor("#E5E7EB")
                .PaddingVertical(5)
                .Row(row =>
                {
                    row.RelativeItem().Text(text =>
                    {
                        var textoLabel = text.Span(label)
                            .FontSize(destacado ? 13 : 9)
                            .FontColor(destacado ? "#D81920" : "#111827");

                        if (destacado)
                        {
                            textoLabel.Bold();
                        }
                    });

                    row.AutoItem().Text(text =>
                    {
                        text.Span($"S/ {value:N2}")
                            .FontSize(destacado ? 13 : 9)
                            .Bold()
                            .FontColor(destacado ? "#D81920" : "#111827");
                    });
                });
        }

        private static IContainer HeaderCell(IContainer container)
        {
            return container
                .Background("#F1F5F9")
                .BorderBottom(1)
                .BorderColor("#E5E7EB")
                .Padding(6);
        }

        private static IContainer BodyCell(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor("#E5E7EB")
                .Padding(5);
        }
    }
}