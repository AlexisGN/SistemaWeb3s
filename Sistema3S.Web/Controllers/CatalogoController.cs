using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema3S.Web.Data;
using Sistema3S.Web.DTOs.Catalogo;

namespace Sistema3S.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogoController : ControllerBase
    {
        private readonly Bd3sContext _context;

        public CatalogoController(Bd3sContext context)
        {
            _context = context;
        }

        [HttpGet("categorias")]
        public async Task<IActionResult> ListarCategorias()
        {
            var data = await _context.Categoria
                .Where(c => c.Estado)
                .OrderBy(c => c.Nombre)
                .Select(c => new CatalogoItemDto
                {
                    Id = c.IdCategoria,
                    Nombre = c.Nombre
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("marcas")]
        public async Task<IActionResult> ListarMarcas()
        {
            var data = await _context.Marca
                .Where(m => m.Estado)
                .OrderBy(m => m.Nombre)
                .Select(m => new CatalogoItemDto
                {
                    Id = m.IdMarca,
                    Nombre = m.Nombre
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("unidades-medida")]
        public async Task<IActionResult> ListarUnidadesMedida()
        {
            var data = await _context.UnidadMedida
                .Where(u => u.Estado)
                .OrderBy(u => u.Nombre)
                .Select(u => new CatalogoItemDto
                {
                    Id = u.IdUnidadMedida,
                    Nombre = u.Nombre
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}