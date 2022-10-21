using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using InmobiliariaEfler.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;



namespace InmobiliariaEfler.Api
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class InmueblesController : ControllerBase
    {
        private readonly DataContext contexto;
        private readonly IConfiguration config;

        public InmueblesController(DataContext contexto, IConfiguration config)
        {
            this.contexto = contexto;
            this.config = config;
        }

        // GET: api/<controller>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var usuario = User.Identity.Name;
                return Ok(contexto.Inmueble.Include(e => e.Propietario).Include(t => t.TipoInmueble).Where(e => e.Propietario.Email == usuario).ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Inmueble inmueble)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await contexto.Inmueble.AddAsync(inmueble);
                    contexto.SaveChanges();
                    return CreatedAtAction(nameof(Get), new { id = inmueble.Id }, inmueble);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT api/<controller>
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Inmueble inmueble)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    contexto.Inmueble.Update(inmueble);
                    contexto.SaveChanges();
                    return Ok(inmueble);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }
        [HttpGet("InmueblesVigentes")]
        public async Task<ActionResult<Inmueble>> InmueblesVigentes()
        {
            try
            {
                var usuario = User.Identity.Name;
                return Ok(contexto.Contrato.Include(i => i.Inmueble).Where(c => c.Inmueble.Propietario.Email == usuario && c.Fecha_Desde <= DateTime.Today.Date && c.Fecha_Hasta >= DateTime.Today.Date).ToList().Select(x => new Inmueble
                {
                    Id = x.Inmueble.Id,
                    Direccion = x.Inmueble.Direccion,
                    Ambientes = x.Inmueble.Ambientes,
                    Latitud = x.Inmueble.Latitud,
                    Longitud = x.Inmueble.Longitud,
                    Precio = x.Inmueble.Precio,
                    Uso = x.Inmueble.Uso,
                    Oferta_activa = x.Inmueble.Oferta_activa,
                    PropietarioId = x.Inmueble.PropietarioId,
                    TipoInmuebleId = x.Inmueble.TipoInmuebleId
                }));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        // GET: api/<controller>
        [HttpGet("ObtenerTipoInmueble")]
        public async Task<IActionResult> ObtenerTipoInmueble()
        {
            try
            {
                return Ok(await contexto.Tipo_Inmueble.ToListAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}