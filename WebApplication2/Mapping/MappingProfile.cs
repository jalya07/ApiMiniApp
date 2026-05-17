using AutoMapper;
using WebApplication2.Dtos;
using WebApplication2.Dtos.OrganizerDto;
using WebApplication2.Dtos.TicketDto;
using WebApplication2.Dtos.UserDtos;
using WebApplication2.Entities;

namespace WebApplication2.Mapping;

public class MappingProfile: Profile
{
    public MappingProfile(HttpClientHandler httpClientHandler)
    {
        // Event
        CreateMap<Event, EventReadDto>();
        CreateMap<EventCreateDto, Event>();
        CreateMap<EventUpdateDto, Event>();
 
        // Organizer
        CreateMap<Organizer, OrganizerReadDto>();
        CreateMap<OrganizerCreateDto, Organizer>();
        CreateMap<OrganizerUpdateDto, Organizer>();
 
        // Ticket
        CreateMap<Ticket, TicketReadDto>();
        CreateMap<TicketCreateDto, Ticket>();
        CreateMap<TicketUpdateDto, Ticket>();
        
        CreateMap<RegisterDto,AppUser>();
    }
}