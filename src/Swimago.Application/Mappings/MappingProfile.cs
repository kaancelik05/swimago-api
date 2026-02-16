using AutoMapper;
using Swimago.Application.DTOs.Common;
using Swimago.Application.DTOs.Listings;
using Swimago.Application.DTOs.Reservations;
using Swimago.Application.DTOs.Reviews;
using Swimago.Application.DTOs.Blog;
using Swimago.Application.DTOs.Auth;
using Swimago.Domain.Entities;

namespace Swimago.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Common
        CreateMap<Dictionary<string, string>, MultiLanguageDto>()
            .ConvertUsing(src => new MultiLanguageDto(
                src.GetValueOrDefault("tr") ?? "",
                src.GetValueOrDefault("en"),
                src.GetValueOrDefault("de"),
                src.GetValueOrDefault("ru")
            ));

        // Listings
        CreateMap<Listing, ListingResponse>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<ListingImage, ListingImageDto>()
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
            .ForMember(dest => dest.AltText, opt => opt.MapFrom(src => src.Alt))
            .ForMember(dest => dest.IsCover, opt => opt.MapFrom(src => src.IsCover));

        CreateMap<Amenity, AmenityDto>()
            .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.Icon))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Label));

        CreateMap<CreateListingRequest, Listing>()
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.Amenities, opt => opt.Ignore());

        // Reservations
        CreateMap<Reservation, ReservationResponse>()
            .ForMember(dest => dest.SpecialRequests, opt => opt.MapFrom(src => src.SpecialRequests))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.VenueType, opt => opt.MapFrom(src => src.VenueType.ToString()));

        CreateMap<ReservationPayment, PaymentResponse>();

        CreateMap<CreateReservationRequest, Reservation>()
            .ForMember(dest => dest.SpecialRequests, opt => opt.MapFrom(src => src.SpecialRequests != null 
                ? new Dictionary<string, string> { { "tr", src.SpecialRequests } } 
                : null));

        // Auth
        CreateMap<User, AuthResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Profile != null ? src.Profile.FirstName.GetValueOrDefault("tr") : ""))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Profile != null ? src.Profile.LastName.GetValueOrDefault("tr") : ""))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.Token, opt => opt.Ignore())
            .ForMember(dest => dest.TokenExpiry, opt => opt.Ignore());

        // Reviews
        CreateMap<Review, ReviewResponse>()
            .ForMember(dest => dest.GuestName, opt => opt.MapFrom(src => 
                src.Guest != null && src.Guest.Profile != null 
                    ? $"{src.Guest.Profile.FirstName.GetValueOrDefault("tr")} {src.Guest.Profile.LastName.GetValueOrDefault("tr")}"
                    : "Anonim"))
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Text))
            .ForMember(dest => dest.HostResponse, opt => opt.MapFrom(src => src.HostResponseText))
            .ForMember(dest => dest.HostResponseAt, opt => opt.MapFrom(src => src.HostResponseDate));

        // Blog
        CreateMap<BlogPost, BlogPostResponse>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src =>
                src.Author != null && src.Author.Profile != null
                    ? $"{src.Author.Profile.FirstName.GetValueOrDefault("tr")} {src.Author.Profile.LastName.GetValueOrDefault("tr")}"
                    : "Anonim"))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Slug));
    }
}
