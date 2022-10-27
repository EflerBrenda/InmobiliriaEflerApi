using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace InmobiliariaEfler.Models;

public class Inmueble
{
    public int Id { get; set; }
    [Display(Name = "Dirección")]
    public String Direccion { get; set; }
    public int Ambientes { get; set; }
    //public Double? Superficie { get; set; }
    public Double Latitud { get; set; }
    public Double Longitud { get; set; }
    public Double Precio { get; set; }

    public int Uso { get; set; }

    [Display(Name = "Oferta activa")]
    public Boolean Oferta_activa { get; set; }

    public String? Imagen { get; set; }

    public int PropietarioId { get; set; }
    public int TipoInmuebleId { get; set; }

    [Display(Name = "Dueño")]
    public Propietario? Propietario { get; set; }

    [Display(Name = "Tipo de inmueble")]
    public Tipo_Inmueble? TipoInmueble { get; set; }

}