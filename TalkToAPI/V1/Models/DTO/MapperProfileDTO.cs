using AutoMapper;

namespace TalkToAPI.V1.Models.DTO
{
    public class MapperProfileDTO : Profile
    {
        public MapperProfileDTO()
        {
            CreateMap<ApplicationUser, UsuarioDTO>()
                .ForMember(dest => dest.Nome, orig => orig.MapFrom(src => src.FullName));

            CreateMap<Mensagem, MensagemDTO>();
        }
    }
}