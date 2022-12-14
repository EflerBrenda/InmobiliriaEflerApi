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
    public class InquilinosController : ControllerBase
    {
        private readonly DataContext contexto;
        private readonly IConfiguration config;

        public InquilinosController(DataContext contexto, IConfiguration config)
        {
            this.contexto = contexto;
            this.config = config;
        }
        // GET: api/<controller>
        [HttpGet("{id}")]
        public async Task<ActionResult<Contrato>> Get(int id)
        {
            try
            {
                var usuario = User.Identity.Name;
                return Ok(await contexto.Contrato.Include(inq => inq.Inquilino).Include(i => i.Inmueble).SingleOrDefaultAsync(x => x.InmuebleId == id && x.Inmueble.Propietario.Email == usuario && x.Fecha_Desde <= DateTime.Today.Date && x.Fecha_Hasta >= DateTime.Today.Date));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}