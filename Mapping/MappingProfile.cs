using AutoMapper;
using Ticket_Booking_System.DTOs;
using Ticket_Booking_System.DTOs.Ticket_Booking_System.DTOs;
using Ticket_Booking_System.Model;

namespace Ticket_Booking_System.Mapping
{

    
    public class MappingProfile : Profile 
    {
        public MappingProfile()
        {
            CreateMap<CreateEventDto, Event>();

            CreateMap<Event, EventDetailDto>();

            CreateMap<Event, EventDetailDto>()
               .ForMember(dest => dest.TotalSeats,
         opt => opt.MapFrom(src => src.TotalTickets ?? 0))
     .ForMember(dest => dest.AvailableSeats,opt => opt.MapFrom(src => (src.TotalTickets ?? 0) - (src.Bookings != null ? src.Bookings.Sum(b => b.NumberOfTickets) : 0)));




            CreateMap<Seat, SeatDto>();


            CreateMap<Event, EventListDto>();


            CreateMap<Booking, MyBookingDto>()
                                       .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))              .ForMember(dest => dest.BookingDate, opt => opt.MapFrom(src => src.BookingDate))              .ForMember(dest => dest.NumberOfTickets, opt => opt.MapFrom(src => src.NumberOfTickets)) 
                                       .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Event != null ? src.Event.Name : string.Empty))              .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Event != null ? src.Event.Location : string.Empty))              .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Event != null ? src.Event.Date : default))              .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Event != null ? src.Event.Price : default)) 
                                       .ForMember(dest => dest.OrganizerUsername, opt => opt.MapFrom(src => (src.Event != null && src.Event.Organizer != null) ? src.Event.Organizer.UserName : string.Empty)) 
                                                                 
                          .ReverseMap();         }







    }
    }

