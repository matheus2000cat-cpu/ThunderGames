using System.ComponentModel.DataAnnotations;

namespace ThunderGames.Domain
{
    public class Pessoa
    {
        public int UserId {get; set;}
        [Required]
        [StringLength(150)]
        public string Nome {get; set;} = string.Empty;
        [Required]
        [StringLength(14)]
        public string CPF {get; set;} = string.Empty;
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email {get; set;} = string.Empty;
        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string Senha {get; set;} = string.Empty;
        [Required]
        [StringLength(20)]
        public string Telefone {get; set;} = string.Empty;
        [Required]
        public string Endereco {get; set;} = string.Empty;
        public string Perfil {get; set;} = "Cliente";
    }
}