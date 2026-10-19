namespace MesaPartesDigital;

/// <summary>Representa los datos principales de un expediente.</summary>
public sealed class Expediente
{
    public string Codigo { get; set; }
    public string Solicitante { get; set; }
    public string DNI { get; set; }
    public string TipoDocumento { get; set; }
    public string Asunto { get; set; }
    public DateTime Fecha { get; set; }

    public Expediente(string codigo, string solicitante, string dni, string tipoDocumento, string asunto, DateTime fecha)
    {
        Codigo = codigo;
        Solicitante = solicitante;
        DNI = dni;
        TipoDocumento = tipoDocumento;
        Asunto = asunto;
        Fecha = fecha;
    }
}
