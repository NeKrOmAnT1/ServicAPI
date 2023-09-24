using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicAPI.Models
{
    public class Model
    {
        public class User
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Token { get; set; }
        }
        public class Product
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }
            public string ProductName { get; set; }
            public int Price { get; set; }
        }
    }
}
