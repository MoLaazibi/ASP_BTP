using AP.BTP.Application.Interfaces;
using AP.BTP.Infrastructure.Contexts;

namespace AP.BTP.Infrastructure.UoW
{
    public class UnitofWork : IUnitofWork
    {
        private readonly BTPContext ctxt;
        private readonly INurserySiteRepository nurserySiteRepository;
        private readonly IAddressRepository addressRepository;
        private readonly IGroundPlanRepository groundPlanRepository;
        private readonly IEmployeeTaskRepository employeeTaskRepo;
        private readonly IUserRepository userRepo;
        private readonly ITaskListRepository taskListRepo;
        private readonly IZoneRepository zoneRepository;
        private readonly ITreeTypeRepository treeTypeRepository;
        private readonly ITreeImageRepository treeImageRepository;
        private readonly IInstructionRepository instructionRepository;
        private readonly ICategoryRepository categoryRepository;


        public UnitofWork(
            BTPContext ctxt,
            INurserySiteRepository nurserySiteRepository,
            IAddressRepository addressRepository,
            IEmployeeTaskRepository employeeTaskRepo,
            ITaskListRepository taskListRepo,
            IUserRepository userRepo,
            IGroundPlanRepository groundPlanRepository,
            IZoneRepository zoneRepository,
            ITreeTypeRepository treeTypeRepository,
            ITreeImageRepository treeImageRepository,
            IInstructionRepository instructionRepository,
            ICategoryRepository categoryRepository)
        {
            this.ctxt = ctxt;
            this.nurserySiteRepository = nurserySiteRepository;
            this.addressRepository = addressRepository;
            this.employeeTaskRepo = employeeTaskRepo;
            this.taskListRepo = taskListRepo;
            this.groundPlanRepository = groundPlanRepository;
            this.userRepo = userRepo;
            this.zoneRepository = zoneRepository;
            this.treeTypeRepository = treeTypeRepository;
            this.treeImageRepository = treeImageRepository;
            this.instructionRepository = instructionRepository;
            this.categoryRepository = categoryRepository;
        }
        public INurserySiteRepository NurserySiteRepository => nurserySiteRepository;
        public IAddressRepository AddressRepository => addressRepository;
        public IEmployeeTaskRepository EmployeeTaskRepository => employeeTaskRepo;
        public ITaskListRepository TaskListRepository => taskListRepo;
        public IGroundPlanRepository GroundPlanRepository => groundPlanRepository;
        public IUserRepository UserRepository => userRepo;
        public IZoneRepository ZoneRepository => zoneRepository;
        public ITreeTypeRepository TreeTypeRepository => treeTypeRepository;
        public ITreeImageRepository TreeImageRepository => treeImageRepository;
        public IInstructionRepository InstructionRepository => instructionRepository;
        public ICategoryRepository CategoryRepository => categoryRepository;
        public async Task Commit()
        {
            await ctxt.SaveChangesAsync();
        }
    }
}
