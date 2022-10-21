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
    public class ContratosController : ControllerBase
    {
        private readonly DataContext contexto;
        private readonly IConfiguration config;

        public ContratosController(DataContext contexto, IConfiguration config)
        {
            this.contexto = contexto;
            this.config = config;
        }

        [HttpGet("Contratosvigentes")]
        public async Task<ActionResult<Contrato>> Contratosvigentes()
        {
            try
            {
                var usuario = User.Identity.Name;
                return Ok(contexto.Contrato.Include(i => i.Inmueble).Where(c => c.Inmueble.Propietario.Email == usuario && c.Fecha_Desde <= DateTime.Today.Date && c.Fecha_Hasta >= DateTime.Today.Date).ToList());

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}