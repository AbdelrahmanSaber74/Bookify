namespace Bookify.Web.Core.Maping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Maps between Category model and CategoryViewModel
            CreateMap<Category, CategoryViewModel>().ReverseMap();
            CreateMap<Category, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id)) // Set the Value to the Category Id
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name)); // Set the Text to the Category Name

            // Maps between Author model and AuthorViewModel
            CreateMap<Author, AuthorViewModel>().ReverseMap();
            CreateMap<Author, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id)) // Set the Value to the Author Id
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name)); // Set the Text to the Author Name

            // Maps between Book model and BookViewModel
            CreateMap<Book, BookViewModel>()
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorName, opt => opt.Ignore())
                .ForMember(dest => dest.NameOfCategories, opt => opt.Ignore())
                .ReverseMap();



            // Maps from BookViewModel to BookCategory
            // Ignoring BookId as it will be set manually when creating BookCategory
            CreateMap<BookViewModel, BookCategory>()
                .ForMember(dest => dest.BookId, opt => opt.Ignore());



            CreateMap<BookCopyViewModel, BookCopy>()
                .ForMember(dest => dest.Book, opt => opt.Ignore())
                .ReverseMap();


            CreateMap<Book, BookCopyViewModel>()
             .ForMember(dest => dest.BookTitle, opt => opt.Ignore());


            CreateMap<Book, SelectListItem>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Title));



            //Users 
            CreateMap<ApplicationUser, UserViewModel>();

            CreateMap<AddEditUserViewModel, ApplicationUser>()
                  .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                  .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.UserName.ToUpper()))
                  .ReverseMap();


            // Subscriber 
            CreateMap<Subscriber, SubscriberViewModel>()
                 .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName.ToUpper()}"))
                .ReverseMap();

            CreateMap<Subscriber, SubscriberSearchResultViewModel>();

            CreateMap<Subscriber, SubscriberFormViewModel>()
                .ReverseMap();




            CreateMap<Governorate, SelectListItem>()
               .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

            CreateMap<Area, SelectListItem>()
               .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id)) // Set the Value to the Category Id
               .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name)); // Set the Text to the Category Name





        }

    }
}
