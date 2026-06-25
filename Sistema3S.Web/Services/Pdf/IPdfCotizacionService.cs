namespace Sistema3S.Web.Services.Pdf
{
    public interface IPdfCotizacionService
    {
        Task<string> GenerarCotizacionAsync(int idCotizacion);
    }
}