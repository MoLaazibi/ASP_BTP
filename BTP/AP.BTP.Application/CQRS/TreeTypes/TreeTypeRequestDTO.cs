using Microsoft.AspNetCore.Http;

namespace AP.BTP.Application.CQRS.TreeTypes
{
    public class TreeTypeRequestDTO
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public IFormFile[]? Images { get; set; }

        public List<InstructionFile>? Instructions { get; set; }

        public class InstructionFile
        {
            public string Season { get; set; } = string.Empty;
            public IFormFile Pdf { get; set; } = default!;
        }
    }
}
