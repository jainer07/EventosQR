namespace eventos_qr.BLL.Helpers
{
    public static class UtilitiesHelper
    {
        public static bool IsValidCelular(string celular)
        {
            if (string.IsNullOrWhiteSpace(celular))
                return false;
            
            // Verificar que el celular tenga exactamente 10 dígitos
            if (celular.Length != 10 || !celular.All(char.IsDigit))
                return false;
            
            // Verificar que el celular comience con '3'
            if (celular[0] != '3')
                return false;

            return true;
        }

        public static bool IsValidCorreo(string? correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return true; // El correo es opcional
            
            // Usar una expresión regular para validar el formato del correo electrónico
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return System.Text.RegularExpressions.Regex.IsMatch(correo, emailPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        public static bool IsValidDocumento(int tipoDocumento, string numeroDocumento)
        {
            if (string.IsNullOrWhiteSpace(numeroDocumento))
                return false;

            return tipoDocumento switch
            {
                1 => numeroDocumento.Length == 10 && numeroDocumento.All(char.IsDigit), // Cédula de ciudadanía
                2 => numeroDocumento.Length == 10 && numeroDocumento.All(char.IsDigit), // Céduala de extranjería
                3 => numeroDocumento.Length == 13 && numeroDocumento.All(char.IsDigit), // NIT
                4 => numeroDocumento.Length >= 6 && numeroDocumento.Length <= 20, // Pasaporte
                _ => false,
            };
        }
    }
}
