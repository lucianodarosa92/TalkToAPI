using System.Linq;
using TalkToAPI.Database;
using TalkToAPI.V1.Models;
using TalkToAPI.V1.Respositories.Interfaces;

namespace TalkToAPI.V1.Respositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly TalkToContext _banco;

        public TokenRepository(TalkToContext banco)
        {
            _banco = banco;
        }

        public Token Obter(string refreshToken)
        {
            return _banco.Token.FirstOrDefault(a => a.RefreshToken == refreshToken && a.Utilizado == false);
        }

        public void Cadastrar(Token token)
        {
            _banco.Token.Add(token);
            _banco.SaveChanges();
        }

        public void Atualizar(Token token)
        {
            _banco.Token.Update(token);
            _banco.SaveChanges();
        }
    }
}