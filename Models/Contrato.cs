using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace InmobiliariaEfler.Models;

public class Contrato
{
    [Display(Name = "Código")]
    public int Id { get; set; }

    [Display(Name = "Fecha de inicio")]
    public DateTime Fecha_Desde { get; set; }

    [Display(Name = "Fecha de finalización")]
    public DateTime Fecha_Hasta { get; set; }

    [Display(Name = "Monto del alquiler")]
    public decimal Monto_Alquiler { get; set; }

    [Display(Name = "Inmueble")]
    public int InmuebleId { get; set; }
    [ForeignKey(nameof(InmuebleId))]
    public Inmueble? Inmueble { get; set; }

    [Display(Name = "Inquilino")]
    public int InquilinoId { get; set; }
    [ForeignKey(nameof(InquilinoId))]
    public Inquilino? Inquilino { get; set; }


}