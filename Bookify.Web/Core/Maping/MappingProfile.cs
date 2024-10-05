namespace Bookify.Web.Core.Maping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Maps between Category model and CategoryViewModel
            CreateMap<Category, CategoryViewModel>().ReverseMap();

            // Maps between Author model and AuthorViewModel
            CreateMap<Author, AuthorViewModel>().ReverseMap();
        }
    }
}
