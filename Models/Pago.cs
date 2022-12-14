using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace InmobiliariaEfler.Models;

public class Pago
{
    public int Id { get; set; }

    [Display(Name = "Número de Pago")]
    public String Numero_pago { get; set; }

    [Display(Name = "Fecha de pago")]
    public DateTime Fecha_pago { get; set; }
    public Double Importe { get; set; }

    [Display(Name = "Número de contrato")]
    public int ContratoId { get; set; }

    [ForeignKey(nameof(ContratoId))]
    public Contrato? Contrato { get; set; }
}