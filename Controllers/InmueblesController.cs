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
        private readonly IWebHostEnvironment environment;

        public InmueblesController(DataContext contexto, IConfiguration config, IWebHostEnvironment environment)
        {
            this.contexto = contexto;
            this.config = config;
            this.environment = environment;
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
        public async Task<IActionResult> Post([FromForm] Inmueble inmueble)
        {
            try
            {

                if (inmueble.Imagen != null)
                {

                    MemoryStream stream1 = new MemoryStream(Convert.FromBase64String(inmueble.Imagen));
                    IFormFile inmuebleFoto = new FormFile(stream1, 0, stream1.Length, "inmueble", ".jpg");
                    string wwwPath = environment.WebRootPath;
                    string path = Path.Combine(wwwPath, "Uploads");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    Random r = new Random();
                    string fileName = "inmueble_" + inmueble.PropietarioId + r.Next(0, 100000) + Path.GetExtension(inmuebleFoto.FileName);
                    string pathCompleto = Path.Combine(path, fileName);

                    inmueble.Imagen = Path.Combine("http://192.168.0.104:5000/", "Uploads/", fileName);
                    using (FileStream stream = new FileStream(pathCompleto, FileMode.Create))
                    {
                        inmuebleFoto.CopyTo(stream);
                    }

                }
                await contexto.Inmueble.AddAsync(inmueble);
                contexto.SaveChanges();
                return CreatedAtAction(nameof(Get), new { id = inmueble.Id }, inmueble);
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