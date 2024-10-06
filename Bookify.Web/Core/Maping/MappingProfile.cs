﻿using Microsoft.AspNetCore.Mvc.Rendering;

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
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()) // Ignore ImageUrl in the mapping
                .ReverseMap(); // Reverse the mapping to go from BookViewModel to Book

            // Maps from BookViewModel to BookCategory
            // Ignoring BookId as it will be set manually when creating BookCategory
            CreateMap<BookViewModel, BookCategory>()
                .ForMember(dest => dest.BookId, opt => opt.Ignore());
        }
    }
}
