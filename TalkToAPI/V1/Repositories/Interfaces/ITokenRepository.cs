﻿using TalkToAPI.V1.Models;

namespace TalkToAPI.V1.Respositories.Interfaces
{
    public interface ITokenRepository
    {
        void Cadastrar(Token token);
        Token Obter(string refreshToken);
        void Atualizar(Token token);
    }
}