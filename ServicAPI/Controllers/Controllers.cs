using Microsoft.EntityFrameworkCore;
using ServicAPI.DataBase;
using static ServicAPI.Models.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace ServicAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context) 
        {
            _context = context;
        }

        [HttpPost("Register")]
        public IActionResult Register([FromBody]User user)
        {
            try
            {
                _context.Users.Add(user);
                _context.SaveChanges();
                return Ok("Пользователь зарегистрирован");
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при регистрации пользователя: {ex.Message}");
            }

        }
        [HttpPost("Login")]
        public IActionResult Login([FromBody]User user)
        {
            var existingUser = _context.Users.SingleOrDefault(u => u.Username == user.Username && u.Password == user.Password);
            if (existingUser == null)
                return Unauthorized("Неверные учетные данные");

            existingUser.Token = GenerateTemporaryToken();
            _context.SaveChanges();

            return Ok(existingUser.Token);
        }
        private string secretKey = Guid.NewGuid().ToString();
        private string GenerateTemporaryToken()
        {
            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, "username"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(30);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }
    }

    [Route("[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet("GetProduct")]
        public IActionResult GetProducts([FromHeader] string authorizationToken)
        {
            if (string.IsNullOrWhiteSpace(authorizationToken))
            {
                return Unauthorized("Не предоставлен временный токен.");
            }
            var products = _context.Products.ToList();
            return Ok(products);

        }

        [HttpPost("AddProduct")]
        public IActionResult AddProduct([FromBody] Product product, [FromHeader] string authorizationToken)
        {
            if (string.IsNullOrWhiteSpace(authorizationToken))
            {
                return Unauthorized("Не предоставлен временный токен.");
            }
            _context.Products.Add(product);
            _context.SaveChanges();
            return Ok("Продукт добавлен");
        }

        [HttpPut("UpdateProduct/{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] Product product, [FromHeader] string authorizationToken)
        {
            if (string.IsNullOrWhiteSpace(authorizationToken))
            {
                return Unauthorized("Не предоставлен временный токен.");
            }
            if (id != product.Id)
            { 
                return BadRequest("Неверный ID");
            }    

            _context.Entry(product).State = EntityState.Modified;
            _context.SaveChanges();
            return Ok("Продукт обновлен");
        }

        [HttpDelete("DeleteProduct/{id}")]
        public IActionResult DeleteProduct(int id, [FromHeader] string authorizationToken)
        {
            if (string.IsNullOrWhiteSpace(authorizationToken))
            {
                return Unauthorized("Не предоставлен временный токен.");
            }
            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound("Продукт не найден");

            _context.Products.Remove(product);
            _context.SaveChanges();
            return Ok("Продукт успешно удален");
        }
    }
}
