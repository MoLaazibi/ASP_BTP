using AP.BTP.Application.CQRS.Categories;
using AP.BTP.Application.CQRS.EmployeeTasks;
using AP.BTP.Application.CQRS.NurserySites;
using AP.BTP.Application.CQRS.TaskLists;
using AP.BTP.Application.CQRS.TreeImages;
using AP.BTP.Application.CQRS.TreeTypes;
using AP.BTP.Application.CQRS.Users;
using AP.BTP.Domain;
using AutoMapper;

namespace AP.BTP.Application
{
    public class Mappings : Profile
    {
        public Mappings()
        {
            // NurserySite mappings
            CreateMap<NurserySite, NurserySiteDTO>()
                .ForPath(dest => dest.GroundPlan.FileUrl,
                    opt => opt.MapFrom(src => src.GroundPlan.FileUrl))
                .ForMember(dest => dest.Address,
                    opt => opt.MapFrom(src => src.Address))
                .ReverseMap();
            // Zone mappings
            CreateMap<Zone, ZoneDTO>().ReverseMap();
            // Address mappings
            CreateMap<Address, AddressDTO>().ReverseMap();
            // GroundPlan mappings
            CreateMap<GroundPlan, GroundPlanDTO>().ReverseMap();
            // TaskList mappings
            CreateMap<TaskList, TaskListDTO>()
                .ForMember(dto => dto.Tasks, opt => opt.MapFrom(src => src.Tasks))
                .ForMember(dto => dto.ZoneName, opt => opt.MapFrom(src => src.Zone.Code))
                .ForMember(dto => dto.IsArchived, opt => opt.MapFrom(src =>
                    src.Date.Date <= DateTime.Today && src.IsArchived));
            CreateMap<TaskListDTO, TaskList>()
                .ForMember(e => e.Tasks, o => o.Ignore())
                .ForMember(e => e.IsArchived, o => o.MapFrom(src => src.IsArchived));
            // EmployeeTask 
            CreateMap<EmployeeTask, EmployeeTaskDTO>();
            CreateMap<EmployeeTaskDTO, EmployeeTask>()
                .ForMember(e => e.TaskList, o => o.Ignore());

            // User roles mapping
            CreateMap<User, UserDTO>()
                .ForMember(d => d.Roles, o => o.MapFrom(s => s.Roles.Select(r => r.Role)))
                .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
                .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
                .ForMember(dto => dto.TaskListsCount, opt => opt.MapFrom(src => src.TaskLists == null ? 0 : src.TaskLists.Count))
                .ForMember(dto => dto.PreferredNurserySiteId, opt => opt.MapFrom(src => src.PreferredNurserySiteId))
                .ReverseMap()
                .ForMember(d => d.Roles, o => o.MapFrom(s => (s.Roles ?? new List<Role>()).Select(r => new UserRole { Role = r })))
                .ForMember(d => d.PreferredNurserySiteId, o => o.MapFrom(s => s.PreferredNurserySiteId));


            // TreeType
            CreateMap<TreeType, TreeTypeDTO>()
               .ForMember(d => d.TreeImages, o => o.MapFrom(src => src.TreeImages))
               .ForMember(d => d.Instructions, o => o.MapFrom(src => src.Instructions));

            CreateMap<TreeImage, TreeImageDTO>();
            CreateMap<Instruction, CQRS.Instructions.InstructionDTO>();

            // Category
            CreateMap<Category, CategoryDTO>().ReverseMap();
        }
    }
}
