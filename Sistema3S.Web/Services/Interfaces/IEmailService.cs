namespace Sistema3S.Web.Services.Interfaces
{
    public interface IEmailService
    {
        Task EnviarConAdjuntoAsync(
            string destinatario,
            string asunto,
            string cuerpo,
            string? rutaArchivoAdjunto
        );
    }
}