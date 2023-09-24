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

        [HttpPost("register")]
        public IActionResult Register(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok("ПОльзователь зарегистрирован");

        }

        [HttpPost("login")]
        public IActionResult Login(User user)
        {
            var existingUser = _context.Users.SingleOrDefault(u => u.Username == user.Username && u.Password == user.Password);
            if (existingUser == null)
                return Unauthorized("Неверные учетные данные");

            existingUser.Token = GenerateTemporaryToken();
            _context.SaveChanges();

            return Ok(existingUser.Token);
        }

        private string GenerateTemporaryToken()
        {
            string secretKey = Guid.NewGuid().ToString();

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


        [HttpGet]
        [Authorize]
        public IActionResult GetProducts()
        {
            var products = _context.Products.ToList();
            return Ok(products);
        }

        /// <summary> На будущее, если у товара будет отдельная карта от каталога   
        /// [HttpGet("{id}")]
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        //public IActionResult GetProduct(int id)
        //{
        //    var product = _context.Products.Find(id);
        //    if (product == null)
        //        return NotFound("Не найден");

        //    return Ok(product);
        //}

        [HttpPost]
        public IActionResult AddProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
            return Ok("Продукт добавлен");
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
                return BadRequest("Неверный ID");

            _context.Entry(product).State = EntityState.Modified;
            _context.SaveChanges();
            return Ok("Продукт обновлен");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound("Продукт не найден");

            _context.Products.Remove(product);
            _context.SaveChanges();
            return Ok("Продукт успешно удален");
        }
    }
}
