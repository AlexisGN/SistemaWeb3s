namespace Sistema3S.Web.Services.Interfaces
{
    public interface IWhatsAppService
    {
        Task EnviarDocumentoPdfAsync(
            string telefonoDestino,
            string rutaPdf,
            string nombreArchivo,
            string mensaje
        );
    }
}