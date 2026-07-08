using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sistema3S.Web.DTOs.Compra;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Services.Implementations
{
    public class PdfCompraService : IPdfCompraService
    {
        private readonly ICompraService _compraService;
        private readonly IWebHostEnvironment _environment;

        public PdfCompraService(
            ICompraService compraService,
            IWebHostEnvironment environment
        )
        {
            _compraService = compraService;
            _environment = environment;
        }

        public async Task<byte[]> GenerarPdfCompraAsync(int idCompra)
        {
            var compra = await _compraService.ObtenerDetalleAsync(idCompra);

            if (compra == null)
            {
                throw new InvalidOperationException("No se encontró la compra seleccionada.");
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

                                            col.Item().Text("Registro interno de compra")
                                                .FontSize(11)
                                                .FontColor("#E5E7EB");

                                            col.Item().Text("Documento generado para control interno")
                                                .FontSize(9)
                                                .FontColor("#CBD5E1");
                                        });
                                });

                                row.ConstantItem(190)
                                    .AlignRight()
                                    .Column(col =>
                                    {
                                        col.Item().AlignRight().Text($"Compra N° {compra.IdCompra}")
                                            .FontSize(16)
                                            .Bold()
                                            .FontColor(Colors.White);

                                        col.Item().AlignRight().Text($"{compra.SerieComprobante}-{compra.NumeroComprobante}")
                                            .FontSize(12)
                                            .Bold()
                                            .FontColor("#FEE2E2");

                                        col.Item().AlignRight().Text(compra.FechaCompra.ToString("dd/MM/yyyy"))
                                            .FontSize(10)
                                            .FontColor("#E5E7EB");

                                        col.Item().AlignRight().Text(compra.EstadoPago)
                                            .FontSize(10)
                                            .Bold()
                                            .FontColor("#DCFCE7");
                                    });
                            });
                    });

                    page.Content().PaddingTop(18).Column(col =>
                    {
                        col.Spacing(12);

                        col.Item().Text("Registro de Compra")
                            .FontSize(24)
                            .Bold()
                            .FontColor("#D81920");

                        col.Item().Element(container =>
                        {
                            CrearCajaInformacion(
                                container,
                                "Datos del proveedor",
                                new List<(string Label, string Value)>
                                {
                                    ("Razón social", compra.RazonSocialProveedor),
                                    ("RUC", compra.RucProveedor),
                                    ("Teléfono", compra.TelefonoProveedor ?? "-"),
                                    ("Correo", compra.CorreoProveedor ?? "-"),
                                    ("Dirección", compra.DireccionProveedor ?? "-")
                                }
                            );
                        });

                        col.Item().Element(container =>
                        {
                            CrearCajaInformacion(
                                container,
                                "Comprobante recibido del proveedor",
                                new List<(string Label, string Value)>
                                {
                                    ("Tipo", compra.TipoComprobanteProveedor),
                                    ("Documento", $"{compra.SerieComprobante}-{compra.NumeroComprobante}"),
                                    ("Fecha emisión", compra.FechaEmisionComprobante?.ToString("dd/MM/yyyy") ?? "-"),
                                    ("Estado compra", compra.EstadoCompra),
                                    ("Estado pago", compra.EstadoPago)
                                }
                            );
                        });

                        col.Item().Element(container =>
                        {
                            if (compra.GuiaRemision == null)
                            {
                                CrearCajaInformacion(
                                    container,
                                    "Guía de remisión recibida",
                                    new List<(string Label, string Value)>
                                    {
                                        ("Estado", "No registrada")
                                    }
                                );
                            }
                            else
                            {
                                var guia = compra.GuiaRemision;

                                CrearCajaInformacion(
                                    container,
                                    "Guía de remisión recibida",
                                    new List<(string Label, string Value)>
                                    {
                                        ("Número", guia.NumeroGuia),
                                        ("Fecha emisión", guia.FechaEmision.ToString("dd/MM/yyyy")),
                                        ("Fecha traslado", guia.FechaTraslado.ToString("dd/MM/yyyy")),
                                        ("Transportista", guia.Transportista ?? "-"),
                                        ("RUC transportista", guia.RucTransportista ?? "-"),
                                        ("Placa", guia.PlacaVehiculo ?? "-"),
                                        ("Punto partida", guia.PuntoPartida),
                                        ("Punto llegada", guia.PuntoLlegada),
                                        ("Observación", guia.Observacion ?? "-")
                                    }
                                );
                            }
                        });

                        col.Item().PaddingTop(4).Text("Productos comprados")
                            .FontSize(12)
                            .Bold()
                            .FontColor("#07111B");

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(70);
                                columns.RelativeColumn();
                                columns.ConstantColumn(55);
                                columns.ConstantColumn(90);
                                columns.ConstantColumn(90);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("Código");
                                header.Cell().Element(HeaderCell).Text("Producto");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Cant.");
                                header.Cell().Element(HeaderCell).AlignRight().Text("P. Unit. c/IGV");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Importe");
                            });

                            foreach (var item in compra.Detalles)
                            {
                                table.Cell().Element(BodyCell).Text(item.CodigoProducto);
                                table.Cell().Element(BodyCell).Text(item.Producto);
                                table.Cell().Element(BodyCell).AlignRight().Text(item.Cantidad.ToString());
                                table.Cell().Element(BodyCell).AlignRight().Text($"S/ {item.PrecioCompra:N2}");
                                table.Cell().Element(BodyCell).AlignRight().Text($"S/ {item.Subtotal:N2}");
                            }
                        });

                        col.Item().Row(row =>
                        {
                            row.RelativeItem();

                            row.ConstantItem(250).Column(totals =>
                            {
                                AgregarLineaTotal(totals, "Subtotal sin IGV", compra.Subtotal, false);
                                AgregarLineaTotal(totals, "IGV 18%", compra.Igv, false);
                                AgregarLineaTotal(totals, "Total", compra.Total, true);
                                AgregarLineaTotal(totals, "Pagado", compra.TotalPagado, false);
                                AgregarLineaTotal(totals, "Saldo", compra.SaldoPendiente, false);
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
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(100);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("Método");
                                header.Cell().Element(HeaderCell).Text("Fecha");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Monto");
                            });

                            if (compra.Pagos.Count == 0)
                            {
                                table.Cell()
                                    .ColumnSpan(3)
                                    .Element(BodyCell)
                                    .Text("No hay pagos registrados.");
                            }
                            else
                            {
                                foreach (var pago in compra.Pagos)
                                {
                                    table.Cell().Element(BodyCell).Text(pago.MetodoPago);
                                    table.Cell().Element(BodyCell).Text(pago.FechaPago.ToString("dd/MM/yyyy"));
                                    table.Cell().Element(BodyCell).AlignRight().Text($"S/ {pago.MontoPagado:N2}");
                                }
                            }
                        });

                        if (compra.Cuotas.Count > 0)
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

                                foreach (var cuota in compra.Cuotas)
                                {
                                    table.Cell().Element(BodyCell).Text(cuota.NumeroCuota.ToString());
                                    table.Cell().Element(BodyCell).Text(cuota.FechaVencimiento.ToString("dd/MM/yyyy"));
                                    table.Cell().Element(BodyCell).AlignRight().Text($"S/ {cuota.MontoCuota:N2}");
                                    table.Cell().Element(BodyCell).AlignRight().Text($"S/ {cuota.MontoPagado:N2}");
                                    table.Cell().Element(BodyCell).Text(cuota.EstadoCuota);
                                }
                            });
                        }

                        if (!string.IsNullOrWhiteSpace(compra.Observacion))
                        {
                            col.Item().Element(container =>
                            {
                                CrearCajaInformacion(
                                    container,
                                    "Observación",
                                    new List<(string Label, string Value)>
                                    {
                                        ("Detalle", compra.Observacion ?? "-")
                                    }
                                );
                            });
                        }
                    });

                    page.Footer().AlignCenter().Element(footer =>
                    {
                        footer
                            .DefaultTextStyle(style => style.FontSize(8).FontColor("#64748B"))
                            .Text(text =>
                            {
                                text.Span("Documento interno generado por Sistema 3S. ");
                                text.Span("El comprobante oficial de compra es emitido por el proveedor.");
                            });
                    });
                });
            }).GeneratePdf(memoryStream);

            return memoryStream.ToArray();
        }

        public byte[] GenerarPdfReporteCompras(
            IEnumerable<ReporteCompraDto> compras,
            DateTime? fechaInicio,
            DateTime? fechaFin
        )
        {
            var lista = compras.ToList();
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
                                    col.Item().Text("Reporte de compras")
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
                                columns.ConstantColumn(78);
                                columns.ConstantColumn(92);
                                columns.RelativeColumn();
                                columns.ConstantColumn(76);
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
                                header.Cell().Element(HeaderCell).Text("Proveedor");
                                header.Cell().Element(HeaderCell).Text("RUC");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Subtotal sin IGV");
                                header.Cell().Element(HeaderCell).AlignRight().Text("IGV");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Total");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Pagado");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Saldo");
                                header.Cell().Element(HeaderCell).Text("Estado");
                            });

                            foreach (var item in lista)
                            {
                                table.Cell().Element(BodyCell).Text(item.IdCompra.ToString());
                                table.Cell().Element(BodyCell).Text(item.FechaCompra.ToString("dd/MM/yyyy"));
                                table.Cell().Element(BodyCell).Text(item.DocumentoCompleto);
                                table.Cell().Element(BodyCell).Text(item.RazonSocialProveedor);
                                table.Cell().Element(BodyCell).Text(item.RucProveedor);
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
                                AgregarLineaTotal(totals, "Total compras", lista.Sum(x => x.Total), true);
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

            return "Todas las compras registradas";
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
                        var span = text.Span(label)
                            .FontSize(destacado ? 13 : 9)
                            .FontColor(destacado ? "#D81920" : "#111827");

                        if (destacado)
                        {
                            span.Bold();
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
                .Padding(6);
        }
    }
}