using System.Collections.Generic;
using TalkToAPI.V1.Models;

namespace TalkToAPI.V1.Repositories.Interfaces
{
    public interface IMensagemRepository
    {
        void CadastrarMensagem(Mensagem mensagem);
        void AtualizarMensagem(Mensagem mensagem);
        List<Mensagem> ObterMensagens(string idUsuarioDe, string IdUsuarioPara);
    }
}