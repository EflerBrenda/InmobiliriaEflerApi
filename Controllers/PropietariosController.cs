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
using MailKit;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http.Features;

namespace InmobiliariaEfler.Api
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class PropietariosController : ControllerBase
    {
        private readonly DataContext contexto;
        private readonly IConfiguration config;

        public PropietariosController(DataContext contexto, IConfiguration config)
        {
            this.contexto = contexto;
            this.config = config;
        }
        // GET: api/<controller>
        [HttpGet]
        public async Task<ActionResult<Propietario>> Get()
        {
            try
            {
                var usuario = User.Identity.Name;
                return await contexto.Propietario.SingleOrDefaultAsync(x => x.Email == usuario);

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        // POST api/<controller>/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromForm] UsuarioLogin usuarioLogin)
        {
            try
            {
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: usuarioLogin.Password,
                    salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 8));

                var p = await contexto.Propietario.FirstOrDefaultAsync(x => x.Email == usuarioLogin.Email);
                if (p == null || p.Password != hashed)
                {
                    return BadRequest("Nombre de usuario o clave incorrecta");
                }
                else
                {
                    var key = new SymmetricSecurityKey(
                        System.Text.Encoding.ASCII.GetBytes(config["TokenAuthentication:SecretKey"]));
                    var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, p.Email),
                        new Claim("FullName", p.Nombre + " " + p.Apellido),
                        new Claim(ClaimTypes.Role, "Propietario"),
                    };

                    var token = new JwtSecurityToken(
                        issuer: config["TokenAuthentication:Issuer"],
                        audience: config["TokenAuthentication:Audience"],
                        claims: claims,
                        expires: DateTime.Now.AddMinutes(60),
                        signingCredentials: credenciales
                    );
                    return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        // PUT api/<controller>
        [HttpPut]
        public async Task<IActionResult> Put([FromForm] Propietario propietario)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    contexto.Propietario.Update(propietario);
                    await contexto.SaveChangesAsync();
                    return Ok(propietario);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        // PUT api/<controller>/cambiarPassword
        [HttpPut("cambiarPassword")]
        public async Task<IActionResult> cambiarPassword([FromForm] CambioPassword usuario)
        {
            try
            {
                string hashedPassVieja = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                   password: usuario.PasswordActual,
                   salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                   prf: KeyDerivationPrf.HMACSHA1,
                   iterationCount: 1000,
                   numBytesRequested: 256 / 8));
                string hashedPassNueva = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                       password: usuario.PasswordNueva,
                       salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                       prf: KeyDerivationPrf.HMACSHA1,
                       iterationCount: 1000,
                       numBytesRequested: 256 / 8));


                var p = await contexto.Propietario.SingleOrDefaultAsync(x => x.Email == User.Identity.Name);
                string PassVieja = p.Password;

                if (PassVieja == hashedPassVieja)
                {

                    p.Password = hashedPassNueva;
                    contexto.Propietario.Update(p);
                    await contexto.SaveChangesAsync();
                    return Ok(p);


                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        // GET: api/<controller>
        [HttpGet("token")]
        public async Task<ActionResult> token()
        {
            try
            {

                var perfil = new
                {
                    Email = User.Identity.Name,
                    Nombre = User.Claims.First(x => x.Type == "FullName").Value,
                    Rol = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value
                };

                Random rand = new Random(Environment.TickCount);
                string randomChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
                string nuevaClave = "";
                for (int i = 0; i < 8; i++)
                {
                    nuevaClave += randomChars[rand.Next(0, randomChars.Length)];
                }

                String nuevaClaveSin = nuevaClave;

                nuevaClave = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                            password: nuevaClave,
                            salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                            prf: KeyDerivationPrf.HMACSHA1,
                            iterationCount: 1000,
                            numBytesRequested: 256 / 8));


                Propietario original = await contexto.Propietario.AsNoTracking().FirstOrDefaultAsync(x => x.Email == perfil.Email);
                original.Password = nuevaClave;
                contexto.Propietario.Update(original);
                await contexto.SaveChangesAsync();

                var message = new MimeKit.MimeMessage();
                message.To.Add(new MailboxAddress(perfil.Nombre, "eflerbrenda@gmail.com"));
                message.From.Add(new MailboxAddress("Inmobiliria Efler", "eflerbrenda@gmail.com"));
                message.Subject = "Inmobiliria Efler App";
                message.Body = new TextPart("html")
                {
                    Text = @$"<h1>Hola {perfil.Nombre}!</h1>
					<p> Su nueva contraseña es: <b>{nuevaClaveSin}</b></p><br>
                    <p> Adios!</p>",
                };

                message.Headers.Add("Encabezado", "Valor");
                MailKit.Net.Smtp.SmtpClient client = new MailKit.Net.Smtp.SmtpClient();
                client.ServerCertificateValidationCallback = (object sender,
                System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                System.Security.Cryptography.X509Certificates.X509Chain chain,
                System.Net.Security.SslPolicyErrors sslPolicyErrors) =>
                { return true; };
                client.Connect("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.Auto);
                client.Authenticate(config["SMTPUser"], config["SMTPPass"]);
                //client.Authenticate("ulp.api.net@gmail.com", "ktitieuikmuzcuup");

                await client.SendAsync(message);

                return Ok("Listo! ya se envio la nueva contraseña a su e-mail.");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // GET api/<controller>/5
        [HttpPost("PedidoEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByEmail([FromForm] string email)
        {
            try
            {
                var feature = HttpContext.Features.Get<IHttpConnectionFeature>();
                var LocalPort = feature?.LocalPort.ToString();
                var ipv4 = HttpContext.Connection.LocalIpAddress.MapToIPv4().ToString();
                var ipConexion = "http://" + ipv4 + ":" + LocalPort + "/";

                var entidad = await contexto.Propietario.FirstOrDefaultAsync(x => x.Email == email);
                var key = new SymmetricSecurityKey(
                        System.Text.Encoding.ASCII.GetBytes(config["TokenAuthentication:SecretKey"]));
                var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, entidad.Email),
                        new Claim("FullName", entidad.Nombre + " " + entidad.Apellido),
                        new Claim("id", entidad.Id + " " ),
                        new Claim(ClaimTypes.Role, "Propietario"),

                    };

                var token = new JwtSecurityToken(
                    issuer: config["TokenAuthentication:Issuer"],
                    audience: config["TokenAuthentication:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(600),
                    signingCredentials: credenciales
                );
                var to = new JwtSecurityTokenHandler().WriteToken(token);

                var direccion = ipConexion + "API/Propietarios/token?access_token=" + to;
                try
                {


                    var message = new MimeKit.MimeMessage();
                    message.To.Add(new MailboxAddress(entidad.Nombre, "eflerbrenda@gmail.com"));
                    message.From.Add(new MailboxAddress("Inmobiliria Efler", "eflerbrenda@gmail.com"));
                    message.Subject = "Inmobiliria Efler App";
                    message.Body = new TextPart("html")


                    {
                        Text = @$"<h1>Hola {entidad.Nombre}!</h1>
					<p>Si usted solicito el cambio de contraseña,<a href={direccion} >presione aquí para reestablecerla.</a> </p><br><p> Si no lo hizo, desestime este e-mail.</p>",
                    };

                    message.Headers.Add("Encabezado", "Valor");
                    MailKit.Net.Smtp.SmtpClient client = new MailKit.Net.Smtp.SmtpClient();
                    client.ServerCertificateValidationCallback = (object sender,
                    System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                    System.Security.Cryptography.X509Certificates.X509Chain chain,
                    System.Net.Security.SslPolicyErrors sslPolicyErrors) =>
                    { return true; };
                    client.Connect("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.Auto);
                    client.Authenticate(config["SMTPUser"], config["SMTPPass"]);

                    await client.SendAsync(message);


                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
                return entidad != null ? Ok(entidad) : NotFound();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }

}
