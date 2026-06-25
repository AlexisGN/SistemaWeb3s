namespace Sistema3S.Web.DTOs.Comun
{
    public class ResultadoPaginadoDto<T>
    {
        public List<T> Items { get; set; } = new();

        public int Pagina { get; set; }

        public int TamanioPagina { get; set; }

        public int TotalRegistros { get; set; }

        public int TotalPaginas
        {
            get
            {
                if (TamanioPagina <= 0)
                {
                    return 0;
                }

                return (int)Math.Ceiling((double)TotalRegistros / TamanioPagina);
            }
        }
    }
}