using AutoMapper;

namespace Bookify.Web.Core.Maping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Category, CategoryDTO>().ReverseMap();

            CreateMap<Author, AuthorDTO>().ReverseMap();
        }
    }
}
