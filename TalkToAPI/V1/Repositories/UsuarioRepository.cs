using Microsoft.AspNetCore.Identity;
using System;
using System.Text;
using TalkToAPI.V1.Models;
using TalkToAPI.V1.Respositories.Interfaces;

namespace TalkToAPI.V1.Respositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsuarioRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        ApplicationUser IUsuarioRepository.Obter(string email, string senha)
        {
            var user = _userManager.FindByEmailAsync(email).Result;

            if (_userManager.CheckPasswordAsync(user, senha).Result)
            {
                return user;
            }
            else
            {
                /*
                 * Domain Notification
                 */
                throw new Exception("Usuário não localizado!");
            }
        }

        ApplicationUser IUsuarioRepository.Obter(string Id)
        {
            return _userManager.FindByIdAsync(Id).Result;
        }

        void IUsuarioRepository.Cadastrar(ApplicationUser Usuario, string senha)
        {
            var result = _userManager.CreateAsync(Usuario, senha).Result;

            if (!result.Succeeded)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var erro in result.Errors)
                {
                    sb.Append(erro.Description);
                }

                /*
                 * Domain Notification
                 */
                throw new Exception($"Usuário não cadastrado! {sb}");
            }
        }
    }
}