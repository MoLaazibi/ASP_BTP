namespace AP.BTP.Application.Interfaces
{
    public interface IUnitofWork
    {
        public INurserySiteRepository NurserySiteRepository { get; }
        public IAddressRepository AddressRepository { get; }
        public IEmployeeTaskRepository EmployeeTaskRepository { get; }
        public ITaskListRepository TaskListRepository { get; }
        public IGroundPlanRepository GroundPlanRepository { get; }
        public IUserRepository UserRepository { get; }
        public IZoneRepository ZoneRepository { get; }
        public ITreeTypeRepository TreeTypeRepository { get; }
        public ITreeImageRepository TreeImageRepository { get; }
        public IInstructionRepository InstructionRepository { get; }
        public ICategoryRepository CategoryRepository { get; }
        Task Commit();
    }
}
