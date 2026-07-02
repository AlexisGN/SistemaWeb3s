namespace Sistema3S.Web.DTOs.Inventario
{
    public class InventarioResumenDto
    {
        public int TotalProductosInventario { get; set; }
        public int TotalStockNormal { get; set; }
        public int TotalStockBajo { get; set; }
        public int TotalSinStock { get; set; }
        public int TotalAlertasPendientes { get; set; }
        public int TotalMovimientos { get; set; }
    }
}