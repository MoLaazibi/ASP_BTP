using AP.BTP.Application.CQRS.Instructions;
using AP.BTP.Application.CQRS.TreeImages;


namespace AP.BTP.Application.CQRS.TreeTypes
{
    public class TreeTypeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsArchived { get; set; }
        public string QrCodeUrl { get; set; }
        public List<TreeImageDTO> TreeImages { get; set; } = new();
        public List<InstructionDTO> Instructions { get; set; } = new();
    }
}
