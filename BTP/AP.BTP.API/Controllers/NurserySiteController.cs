using AP.BTP.Application.CQRS.NurserySites;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace AP.BTP.API.Controllers
{
    [ApiController]
    public class NurserySiteController : APIv1Controller
    {
        private readonly IMediator mediator;
        private readonly IWebHostEnvironment env;
        public NurserySiteController(IMediator mediator, IWebHostEnvironment env)
        {
            this.mediator = mediator;
            this.env = env;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll([FromQuery] int pageNr, [FromQuery] int pageSize, [FromQuery] bool? isArchived)
        {
            var query = new GetAllNurserySiteQuery { PageNr = pageNr, PageSize = pageSize, IsArchived = isArchived };
            var result = await mediator.Send(query);
            return Ok(result);
        }

        [HttpGet]
        [Route("count")]
        public async Task<IActionResult> GetCount()
        {
            var query = new GetNurserySiteCountQuery();
            var result = await mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetNurserySiteByIdQuery { Id = id };
            var result = await mediator.Send(query);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Create([FromBody] AddNurserySiteCommand command)
        {
            var result = await mediator.Send(command);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("groundplan/upload")]
        public async Task<IActionResult> UploadGroundPlan([FromForm] IFormFile file, [FromForm] string? oldUrl)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file selected.");

            const long MaxFileSize = 20 * 1024 * 1024;
            if (file.Length > MaxFileSize)
                return BadRequest("File is too large.");

            var allowedTypes = new[] { "image/png", "image/jpeg" };
            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest("Invalid file type.");

            if (!string.IsNullOrWhiteSpace(oldUrl))
            {
                try
                {
                    var filename = Path.GetFileName(new Uri(oldUrl).AbsolutePath);
                    var oldPath = Path.Combine(env.WebRootPath, "Files", "GroundPlans", filename);

                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }
                catch 
                {
                    // Ignored on purpose:
                    // If the file cannot be deleted (e.g., locked by another process, permission issue),
                    // it is not critical. The application should continue, and a leftover file is harmless.                }
                }
            }

                var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExtension}";

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files", "GroundPlans");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var newPath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(newPath, FileMode.Create))
                await file.CopyToAsync(stream);

            var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var apiPublic = config["Api:PublicBaseUrl"]?.TrimEnd('/');
            var url = !string.IsNullOrWhiteSpace(apiPublic)
                ? $"{apiPublic}/Files/GroundPlans/{fileName}"
                : $"{Request.Scheme}://{Request.Host}/Files/GroundPlans/{fileName}";
            return Ok(url);
        }


        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Edit([FromBody] EditNurserySiteCommand command)
        {
            var result = await mediator.Send(command);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("archive")]
        public async Task<IActionResult> EditArchive([FromBody] EditNurserySiteArchivedCommand command)
        {
            var result = await mediator.Send(command);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var site = await mediator.Send(new GetNurserySiteByIdQuery { Id = id });
                var fileUrl = site?.GroundPlan?.FileUrl;
                if (!string.IsNullOrWhiteSpace(fileUrl) && fileUrl.StartsWith('/'))
                {
                    var path = Path.Combine(env.ContentRootPath, "wwwroot", fileUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(path))
                    {
                        try
                        {
                            System.IO.File.Delete(path);
                        }
                        catch
                        {
                            // File deletion failed (e.g., locked, permission issue, already deleted)
                            // It's safe to ignore because this is a cleanup operation,
                            // and failing to delete the file does not block the main process.                        }
                        }
                    }
                }
            }
            catch 
            {
                // Overall failure (e.g., site not found, mediator failure, null reference)
                // Can be safely ignored because this operation is best-effort cleanup.
                // The main flow does not depend on deleting this file.
            }

            var result = await mediator.Send(new DeleteNurserySiteCommand { Id = id });
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }
    }
}
