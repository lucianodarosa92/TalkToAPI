using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using TalkToAPI.Helpers.Constants;
using TalkToAPI.V1.Models;
using TalkToAPI.V1.Models.DTO;
using TalkToAPI.V1.Respositories.Interfaces;

namespace TalkToAPI.V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]

    public class UsuarioController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly SignInManager<ApplicationUser> _signmanager;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsuarioController(IMapper mapper, IUsuarioRepository usuarioRepository, ITokenRepository tokenRepository, SignInManager<ApplicationUser> signmanager, UserManager<ApplicationUser> userManager)
        {
            _mapper = mapper;
            _usuarioRepository = usuarioRepository;
            _tokenRepository = tokenRepository;
            _signmanager = signmanager;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet("ObterTodos", Name = "ObterTodos")]
        public ActionResult ObterTodos([FromHeader(Name = "Accept")] string mediaType)
        {
            var usuariosAppUser = _userManager.Users.ToList();

            if (mediaType == CustomMediaType.Hateoas)
            {
                var usuariosDTO = _mapper.Map<List<ApplicationUser>, List<UsuarioDTO>>(usuariosAppUser);

                foreach (var usuarioDTO in usuariosDTO)
                {
                    usuarioDTO.Links.Add(new LinkDTO("_ObterUsuario", Url.Link("ObterUsuario", new { id = usuarioDTO.Id }), "GET"));
                }

                var lista = new ListaDTO<UsuarioDTO>() { Lista = usuariosDTO };

                lista.Links.Add(new LinkDTO("_self", Url.Link("ObterTodos", null), "GET"));

                return Ok(lista);
            }
            else
            {
                var usuarioDTOSemHyperLink = _mapper.Map<List<ApplicationUser>, List<UsuarioDTOSemHyperLink>>(usuariosAppUser);

                return Ok(usuarioDTOSemHyperLink);
            }
        }

        [Authorize]
        [HttpGet("ObterUsuario={id}", Name = "ObterUsuario")]
        public ActionResult ObterUsuario(string id, [FromHeader(Name = "Accept")] string mediaType)
        {
            var usuario = _userManager.FindByIdAsync(id).Result;

            if (usuario == null)
            {
                return NotFound();
            }

            if (mediaType == CustomMediaType.Hateoas)
            {
                var usuarioDTOdb = _mapper.Map<ApplicationUser, UsuarioDTO>(usuario);

                usuarioDTOdb.Links.Add(new LinkDTO("_self", Url.Link("ObterUsuario", new { id = usuarioDTOdb.Id }), "GET"));
                usuarioDTOdb.Links.Add(new LinkDTO("_atualizar", Url.Link("atualizar", new { id = usuarioDTOdb.Id }), "PUT"));

                return Ok(usuarioDTOdb);
            }
            else
            {
                var usuarioDTOSemHyperLink = _mapper.Map<ApplicationUser, UsuarioDTOSemHyperLink>(usuario);

                return Ok(usuarioDTOSemHyperLink);
            }
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody] UsuarioDTO usuarioDTO)
        {
            ModelState.Remove("Nome");
            ModelState.Remove("ConfirmacaoSenha");

            if (ModelState.IsValid)
            {
                var usuario = _usuarioRepository.Obter(usuarioDTO.Email, usuarioDTO.Senha);

                if (usuario != null)
                {
                    //login no entity
                    //_signmanager.SignInAsync(usuario, false);

                    return GerarToken(usuario);
                }
                else
                {
                    return NotFound("Usuario não localizado!");
                }
            }
            else
            {
                return UnprocessableEntity(ModelState);
            }
        }

        [HttpPost("cadastrar", Name = "cadastrar")]
        public ActionResult Cadastrar([FromBody] UsuarioDTO usuarioDTO, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (ModelState.IsValid)
            {
                var usuario = new ApplicationUser();
                usuario.FullName = usuarioDTO.Nome;
                usuario.UserName = usuarioDTO.Email;
                usuario.Email = usuarioDTO.Email;

                var resultado = _userManager.CreateAsync(usuario, usuarioDTO.Senha).Result;

                if (!resultado.Succeeded)
                {
                    var erros = new List<String>();
                    foreach (var erro in resultado.Errors)
                    {
                        erros.Add(erro.Description);
                    }
                    return UnprocessableEntity(erros);
                }
                else
                {
                    if (mediaType == CustomMediaType.Hateoas)
                    {
                        var usuarioDTOdb = _mapper.Map<ApplicationUser, UsuarioDTO>(usuario);

                        usuarioDTOdb.Links.Add(new LinkDTO("_atualizar", Url.Link("atualizar", new { id = usuarioDTOdb.Id }), "PUT"));
                        usuarioDTOdb.Links.Add(new LinkDTO("_ObterUsuario", Url.Link("ObterUsuario", new { id = usuarioDTOdb.Id }), "GET"));

                        return Ok(usuarioDTOdb);
                    }
                    else
                    {
                        var usuarioDTOSemHyperLink = _mapper.Map<ApplicationUser, UsuarioDTOSemHyperLink>(usuario);

                        return Ok(usuarioDTOSemHyperLink);
                    }
                }
            }
            else
            {
                return UnprocessableEntity(ModelState);
            }
        }

        [Authorize]
        [HttpPut("atualizar={id}", Name = "atualizar")]
        public ActionResult Atualizar(string id, [FromBody] UsuarioDTO usuarioDTO, [FromHeader(Name = "Accept")] string mediaType)
        {
            //TODO - Adicionar filtro de validação

            if (_userManager.GetUserAsync(HttpContext.User).Result == null || _userManager.GetUserAsync(HttpContext.User).Result.Id != id)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                //TODO - Refatorar para AutoMapper
                //var usuario = new ApplicationUser();

                var usuario = _userManager.GetUserAsync(HttpContext.User).Result;

                usuario.FullName = usuarioDTO.Nome;
                usuario.UserName = usuarioDTO.Email;
                usuario.Email = usuarioDTO.Email;
                usuario.Slogan = usuarioDTO.Slogan;

                //TODO - Remover no Identity critérios de senha
                var resultado = _userManager.UpdateAsync(usuario).Result;
                _userManager.RemovePasswordAsync(usuario);
                _userManager.AddPasswordAsync(usuario, usuarioDTO.Senha);

                if (!resultado.Succeeded)
                {
                    var erros = new List<String>();
                    foreach (var erro in resultado.Errors)
                    {
                        erros.Add(erro.Description);
                    }
                    return UnprocessableEntity(erros);
                }
                else
                {
                    if (mediaType == CustomMediaType.Hateoas)
                    {
                        var usuarioDTOdb = _mapper.Map<ApplicationUser, UsuarioDTO>(usuario);

                        usuarioDTOdb.Links.Add(new LinkDTO("_self", Url.Link("atualizar", new { id = usuarioDTOdb.Id }), "PUT"));
                        usuarioDTOdb.Links.Add(new LinkDTO("_ObterUsuario", Url.Link("ObterUsuario", new { id = usuarioDTOdb.Id }), "GET"));

                        return Ok(usuarioDTOdb);
                    }
                    else
                    {
                        var usuarioDTOSemHyperLink = _mapper.Map<ApplicationUser, UsuarioDTOSemHyperLink>(usuario);

                        return Ok(usuarioDTOSemHyperLink);
                    }
                }
            }
            else
            {
                return UnprocessableEntity(ModelState);
            }
        }

        [HttpPost("renovar")]
        public ActionResult Renovar([FromBody] TokenDTO tokenDTO)
        {
            var refreshTokenDB = _tokenRepository.Obter(tokenDTO.RefreshToken);

            if (refreshTokenDB == null)
            {
                return NotFound();
            }

            //atualiza token antigo
            refreshTokenDB.Atualizado = DateTime.Now;
            refreshTokenDB.Utilizado = true;
            _tokenRepository.Atualizar(refreshTokenDB);

            //gera token novo
            var usuario = _usuarioRepository.Obter(refreshTokenDB.UsuarioId);

            return GerarToken(usuario);
        }

        private TokenDTO BuildToken(ApplicationUser usuario)
        {
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("chave-api-jwt-minhas-tarefas")); // Recomento -> appsettings.json
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddHours(1);

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = Guid.NewGuid().ToString().Replace("-", "");

            var expirationRefreshToken = DateTime.UtcNow.AddHours(2);

            return new TokenDTO { Token = tokenString, Expiration = expiration, RefreshToken = refreshToken, ExpirationRefreshToken = expirationRefreshToken };
        }

        private ActionResult GerarToken(ApplicationUser usuario)
        {
            var token = BuildToken(usuario);

            var tokenModel = new Token()
            {
                RefreshToken = token.RefreshToken,
                ExpirationToken = token.Expiration,
                ExpirationRefreshToken = token.ExpirationRefreshToken,
                Usuario = usuario,
                Criado = DateTime.Now,
                Utilizado = false
            };

            _tokenRepository.Cadastrar(tokenModel);

            return Ok(token);
        }
    }
}