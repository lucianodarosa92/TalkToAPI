using System.Collections.Generic;
using System.Linq;
using TalkToAPI.Database;
using TalkToAPI.V1.Models;
using TalkToAPI.V1.Repositories.Interfaces;

namespace TalkToAPI.V1.Repositories
{
    public class MensagemRepository : IMensagemRepository
    {
        private readonly TalkToContext _banco;

        public MensagemRepository(TalkToContext banco)
        {
            _banco = banco;
        }

        public void CadastrarMensagem(Mensagem mensagem)
        {
            _banco.Mensagem.Add(mensagem);
            _banco.SaveChanges();
        }

        public void AtualizarMensagem(Mensagem mensagem)
        {
            _banco.Mensagem.Update(mensagem);
            _banco.SaveChanges();
        }

        public List<Mensagem> ObterMensagens(string idUsuarioDe, string IdUsuarioPara)
        {
            var mensagens = _banco.Mensagem.Where(m =>
            (m.DeId == idUsuarioDe && m.ParaId == IdUsuarioPara) ||
            (m.DeId == IdUsuarioPara && m.ParaId == idUsuarioDe)).ToList().OrderBy(o => o.Id).ToList();

            return mensagens;
        }

        public Mensagem ObterMensagem(int id)
        {
            return _banco.Mensagem.Find(id);
        }
    }
}