namespace Sistema3S.Web.DTOs.Publico
{
    public class InicioPublicoDto
    {
        public List<CategoriaPublicaDto> Categorias { get; set; } = new();
        public List<MarcaPublicaDto> Marcas { get; set; } = new();
        public List<ProductoPublicoDto> ProductosNuevos { get; set; } = new();
        public List<ProductoPublicoDto> Productos { get; set; } = new();
        public List<ServicioPublicoDto> Servicios { get; set; } = new();
    }

    public class CategoriaPublicaDto
    {
        public int Id { get; set; }
        public int IdCategoria { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public int CantidadProductos { get; set; }
    }

    public class MarcaPublicaDto
    {
        public int Id { get; set; }
        public int IdMarca { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public int CantidadProductos { get; set; }
    }

    public class ImagenPublicaDto
    {
        public int IdImagen { get; set; }
        public string UrlImagen { get; set; } = string.Empty;
        public string TextoAlternativo { get; set; } = string.Empty;
        public bool EsPrincipal { get; set; }
    }

    public class ProductoPublicoDto
    {
        public int Id { get; set; }
        public int IdProducto { get; set; }
        public int IdElementoCatalogo { get; set; }
        public int IdCategoria { get; set; }
        public int? IdMarca { get; set; }

        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string? Marca { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string ImagenUrl { get; set; } = string.Empty;

        public bool Nuevo { get; set; }
        public bool TieneFichaTecnica { get; set; }
        public string? FichaTecnicaPdf { get; set; }
    }

    public class ProductoDetallePublicoDto : ProductoPublicoDto
    {
        public List<ImagenPublicaDto> Imagenes { get; set; } = new();
    }

    public class ProductoPublicoListadoDto
    {
        public List<ProductoPublicoDto> Items { get; set; } = new();

        public int TotalRegistros { get; set; }
        public int Pagina { get; set; }
        public int TamanioPagina { get; set; }
        public int TotalPaginas { get; set; }
        public bool HayMas { get; set; }
    }

    public class ServicioPublicoDto
    {
        public int Id { get; set; }
        public int IdServicio { get; set; }
        public int IdElementoCatalogo { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? SectorAplicacion { get; set; }
        public string? MensajeWhatsApp { get; set; }
        public bool RequiereVisitaTecnica { get; set; }

        public string ImagenUrl { get; set; } = string.Empty;
    }

    public class ServicioDetallePublicoDto : ServicioPublicoDto
    {
        public List<ImagenPublicaDto> Imagenes { get; set; } = new();
    }
}