using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TalkToAPI.Helpers.Constants;
using TalkToAPI.V1.Models;
using TalkToAPI.V1.Models.DTO;
using TalkToAPI.V1.Repositories.Interfaces;

namespace TalkToAPI.V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]

    public class MensagemController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IMensagemRepository _mensagemRepository;

        public MensagemController(IMapper mapper, IMensagemRepository mensagemRepository)
        {
            _mapper = mapper;
            _mensagemRepository = mensagemRepository;
        }

        [HttpPost("CadastrarMensagem", Name = "CadastrarMensagem")]
        [Authorize]
        public ActionResult CadastrarMensagem([FromBody] Mensagem mensagem, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _mensagemRepository.CadastrarMensagem(mensagem);

                    if (mediaType == CustomMediaType.Hateoas)
                    {
                        var mensagemDTO = _mapper.Map<Mensagem, MensagemDTO>(mensagem);

                        mensagemDTO.Links.Add(new LinkDTO("_AtualizarMensagem", Url.Link("AtualizarMensagem", new { id = mensagemDTO.Id }), "PUT"));
                        mensagemDTO.Links.Add(new LinkDTO("_ObterMensagem", Url.Link("ObterMensagem", new { id = mensagemDTO.Id }), "GET"));

                        return Ok(mensagemDTO);
                    }
                    else
                    {
                        return Ok(mensagem);
                    }
                }
                catch (Exception e)
                {
                    return UnprocessableEntity(e);
                }

            }
            else
            {
                return UnprocessableEntity(ModelState);
            }
        }

        [HttpPatch("AtualizarMensagem={id}", Name = "AtualizarMensagem")]
        [Authorize]
        public ActionResult AtualizarMensagem(int id, [FromBody] JsonPatchDocument<Mensagem> jsonPath, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (jsonPath == null)
            {
                return BadRequest();
            }

            var mensagem = _mensagemRepository.ObterMensagem(id);

            jsonPath.ApplyTo(mensagem);
            mensagem.Atualizado = DateTime.UtcNow;

            _mensagemRepository.AtualizarMensagem(mensagem);

            if (mediaType == CustomMediaType.Hateoas)
            {
                var mensagemDTO = _mapper.Map<Mensagem, MensagemDTO>(mensagem);

                mensagemDTO.Links.Add(new LinkDTO("_self", Url.Link("AtualizarMensagem", new { id = mensagemDTO.Id }), "PUT"));
                mensagemDTO.Links.Add(new LinkDTO("_ObterMensagem", Url.Link("ObterMensagem", new { id = mensagemDTO.Id }), "GET"));

                return Ok(mensagemDTO);
            }
            else
            {
                return Ok(mensagem);
            }
        }

        [HttpGet("ObterMensagens={idUsuarioDe}/{idUsuarioPara}", Name = "ObterMensagens")]
        [Authorize]
        public ActionResult ObterMensagens(string idUsuarioDe, string idUsuarioPara, [FromHeader(Name ="Accept")] string mediaType)
        {
            if (idUsuarioDe == idUsuarioPara)
            {
                return UnprocessableEntity();
            }

            var mensagens = _mensagemRepository.ObterMensagens(idUsuarioDe, idUsuarioPara);

            if (mediaType == CustomMediaType.Hateoas)
            {                
                var mensagensDTO = _mapper.Map<List<Mensagem>, List<MensagemDTO>>(mensagens);

                foreach (var mensagemDTO in mensagensDTO)
                {
                    mensagemDTO.Links.Add(new LinkDTO("_ObterMensagem", Url.Link("ObterMensagem", new { id = mensagemDTO.Id }), "GET"));
                    mensagemDTO.Links.Add(new LinkDTO("_AtualizarMensagem", Url.Link("AtualizarMensagem", new { id = mensagemDTO.Id }), "PUT"));
                }

                var lista = new ListaDTO<MensagemDTO>() { Lista = mensagensDTO };

                lista.Links.Add(new LinkDTO("_self", Url.Link("ObterMensagens", new { idUsuarioDe = idUsuarioDe, idUsuarioPara = idUsuarioPara }), "GET"));

                return Ok(lista);
            }
            else
            {
                return Ok(mensagens);
            }
        }

        [HttpGet("ObterMensagem={id}", Name = "ObterMensagem")]
        [Authorize]
        public ActionResult ObterMensagem(int id, [FromHeader(Name = "Accept")] string mediaType)
        {
            var mensagem = _mensagemRepository.ObterMensagem(id);

            if (mensagem == null)
            {
                return NotFound();
            }

            if (mediaType == CustomMediaType.Hateoas)
            {
                var mensagemDTO = _mapper.Map<Mensagem, MensagemDTO>(mensagem);

                mensagemDTO.Links.Add(new LinkDTO("_self", Url.Link("ObterMensagem", new { id = mensagemDTO.Id }), "GET"));
                mensagemDTO.Links.Add(new LinkDTO("_AtualizarMensagem", Url.Link("AtualizarMensagem", new { id = mensagemDTO.Id }), "PUT"));

                return Ok(mensagemDTO);
            }
            else
            {
                return Ok(mensagem);
            }
        }
    }
}