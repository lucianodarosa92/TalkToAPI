using System.ComponentModel.DataAnnotations;

namespace TalkToAPI.V1.Models.DTO
{
    public class UsuarioDTO
    {
        [Required]
        public string Nome { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Senha { get; set; }
        [Required]
        [Compare("Senha")]
        public string ConfirmacaoSenha { get; set; }

        public string Slogan { get; set; }
    }
}