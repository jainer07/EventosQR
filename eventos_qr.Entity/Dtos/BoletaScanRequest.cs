namespace eventos_qr.Entity.Dtos
{
    public class BoletaScanRequest
    {
        public string Code { get; set; } = default!;   // código embebido en el QR
        public long OperatorId { get; set; }        // id del operario (opcional)
        public bool ValidateOnly { get; set; } = false; // true = solo valida, no marca uso
    }
}
