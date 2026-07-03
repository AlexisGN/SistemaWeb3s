namespace Sistema3S.Web.Services.Interfaces
{
    public interface IPdfCompraService
    {
        Task<byte[]> GenerarPdfCompraAsync(int idCompra);
    }
}