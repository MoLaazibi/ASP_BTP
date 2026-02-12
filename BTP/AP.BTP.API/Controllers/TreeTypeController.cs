using AP.BTP.Application.CQRS.Instructions;
using AP.BTP.Application.CQRS.TreeTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Linq;

namespace AP.BTP.API.Controllers
{
    [ApiController]
    public class TreeTypeController : APIv1Controller
    {
        private readonly IMediator mediator;
        private readonly IWebHostEnvironment env;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;
        public TreeTypeController(IMediator mediator, IWebHostEnvironment env, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.mediator = mediator;
            this.env = env;
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
        }

        [HttpGet]
        [Route("count")]
        public async Task<IActionResult> GetCount([FromQuery] bool? isArchived)
        {
            var query = new GetTreeTypeCountQuery { IsArchived = isArchived };
            var result = await mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("by-archive")]
        public async Task<IActionResult> GetByArchive([FromQuery] bool isArchived, [FromQuery] int pageNr, [FromQuery] int pageSize)
        {
            var query = new GetAllTreeTypesByArchiveStatusQuery
            {
                IsArchived = isArchived,
                PageNr = pageNr,
                PageSize = pageSize
            };

            var result = await mediator.Send(query);

            foreach (var tree in result)
                ConvertTreeTypeUrlsToAbsolute(tree);

            return Ok(result);
        }

        [HttpGet]
        [Route("{treeTypeId:int}")]
        public async Task<IActionResult> GetById(int treeTypeId)
        {
            // Use a wide page size to fetch and filter by id without adding a new query
            var items = await mediator.Send(new GetAllTreeTypesQuery { PageNr = 1, PageSize = int.MaxValue });
            var item = items.FirstOrDefault(t => t.Id == treeTypeId);
            if (item == null) return NotFound("Boomsoort niet gevonden");

            ConvertTreeTypeUrlsToAbsolute(item);

            return Ok(item);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Create([FromForm] TreeTypeRequestDTO dto)
        {
            // --- 1. Pre-validate files ---
            var allowedImageTypes = new[] { "image/png", "image/jpeg" };
            var allowedPdfTypes = new[] { "application/pdf" };
            const long maxImageSize = 20 * 1024 * 1024; // 20MB
            const long maxPdfSize = 50 * 1024 * 1024;  // 50MB

            if (dto.Images != null)
            {
                foreach (var img in dto.Images)
                {
                    if (img.Length == 0) return BadRequest("Empty image file not allowed.");
                    if (!allowedImageTypes.Contains(img.ContentType)) return BadRequest("Invalid image type.");
                    if (img.Length > maxImageSize) return BadRequest("Image file too large (max 20MB).");
                }
            }

            if (dto.Instructions != null)
            {
                foreach (var instr in dto.Instructions)
                {
                    if (instr.Pdf == null || instr.Season == null) continue;
                    if (!allowedPdfTypes.Contains(instr.Pdf.ContentType)) return BadRequest("Invalid PDF type.");
                    if (instr.Pdf.Length > maxPdfSize) return BadRequest("PDF file too large (max 50MB).");
                }
            }

            // --- 2. Save files after validation ---
            var savedImageUrls = new List<string>();
            var savedInstructions = new List<AddTreeTypeCommand.InstructionInput>();
            var root = Path.Combine(env.ContentRootPath, "wwwroot");

            // ------------ Save IMAGES -------------
            if (dto.Images != null)
            {
                var imgDir = Path.Combine(root, "Files", "TreeTypes");
                Directory.CreateDirectory(imgDir);

                foreach (var img in dto.Images)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(img.FileName)}";
                    var path = Path.Combine(imgDir, fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    await img.CopyToAsync(stream);

                    savedImageUrls.Add($"/Files/TreeTypes/{fileName}");
                }
            }

            // ------------ Save INSTRUCTIONS (PDFs) -------------
            if (dto.Instructions != null)
            {
                var instrDir = Path.Combine(root, "Files", "TreeInstructions");
                Directory.CreateDirectory(instrDir);

                foreach (var i in dto.Instructions)
                {
                    if (i.Pdf == null) continue;

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(i.Pdf.FileName)}";
                    var path = Path.Combine(instrDir, fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    await i.Pdf.CopyToAsync(stream);

                    savedInstructions.Add(new AddTreeTypeCommand.InstructionInput
                    {
                        Season = i.Season,
                        FileUrl = $"/Files/TreeInstructions/{fileName}"
                    });
                }
            }

            // ------------ Prepare Command -------------
            var cmd = new AddTreeTypeCommand
            {
                Name = dto.Name,
                Description = dto.Description,
                ImageUrls = savedImageUrls,
                Instructions = savedInstructions
            };

            var result = await mediator.Send(cmd);
            if (!result.Succeeded) return BadRequest(result);

            if (result.Data != null)
            {
                try
                {
                    var apiPublic = configuration["Api:PublicBaseUrl"]?.TrimEnd('/');
                    var baseUrl = !string.IsNullOrWhiteSpace(apiPublic)
                        ? apiPublic
                        : $"{Request.Scheme}://{Request.Host}{Request.PathBase}".TrimEnd('/');
                    var targetUrl = $"{baseUrl}/api/v1/TreeType/{result.Data.Id}/instruction/current/file";
                    using var generator = new QRCodeGenerator();
                    using var data = generator.CreateQrCode(targetUrl, QRCodeGenerator.ECCLevel.M);
                    var pngQr = new PngByteQRCode(data);
                    var bytes = pngQr.GetGraphic(10);

                    var webRoot = Path.Combine(env.ContentRootPath, "wwwroot");
                    var dir = Path.Combine(webRoot, "Files", "QRCodes");
                    Directory.CreateDirectory(dir);
                    var fileName = $"TreeType_{result.Data.Id}.png";
                    var phys = Path.Combine(dir, fileName);
                    System.IO.File.WriteAllBytes(phys, bytes);

                    var updateQrCmd = new UpdateTreeTypeQrCodeCommand
                    {
                        TreeTypeId = result.Data.Id,
                        QrCodeUrl = $"/Files/QRCodes/{fileName}"
                    };
                    var qrUpdateResult = await mediator.Send(updateQrCmd);
                }
                catch (Exception ex)
                {
                    // QR code creation failure should not block the main request.
                    // The tree type will still be created successfully.
                    // Log the exception for debugging.
                    Console.WriteLine($"QR generation failed: {ex}");
                }
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("{treeTypeId:int}/instruction/current/file")]
        public async Task<IActionResult> GetCurrentInstructionFile(int treeTypeId)
        {
            var result = await mediator.Send(new GetCurrentInstructionForTreeTypeQuery(treeTypeId));
            if (!result.Succeeded || result.Data == null || string.IsNullOrWhiteSpace(result.Data.FileUrl))
                return NotFound(result.Message);

            var fileUrl = result.Data.FileUrl;
            var apiPublic = configuration["Api:PublicBaseUrl"]?.TrimEnd('/');
            var tryAbsolute = fileUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? fileUrl
                : (!string.IsNullOrWhiteSpace(apiPublic) ? $"{apiPublic}{fileUrl}" : $"{Request.Scheme}://{Request.Host}{fileUrl}");
            var fileName = Path.GetFileName(new Uri(tryAbsolute).AbsolutePath);


            if (fileUrl.StartsWith('/'))
            {
                var localPath = Path.Combine(env.ContentRootPath, "wwwroot", fileUrl.TrimStart('/'));
                if (System.IO.File.Exists(localPath))
                {
                    var bytes = await System.IO.File.ReadAllBytesAsync(localPath);
                    return File(bytes, "application/pdf", fileName);
                }
            }

            try
            {
                var client = httpClientFactory.CreateClient();
                try
                {
                    var bytes1 = await client.GetByteArrayAsync(tryAbsolute);
                    return File(bytes1, "application/pdf", fileName);
                }
                catch 
                {
                    // This exception is intentionally ignored because failing here is expected:
                    // If the API does not contain the PDF, we fall back to the UI base URL.
                }

                var uiBase = configuration["Ui:BaseUrl"];
                if (!string.IsNullOrWhiteSpace(uiBase) && fileUrl.StartsWith('/'))
                {
                    try
                    {
                        var uiAbsolute = $"{uiBase.TrimEnd('/')}{fileUrl}";
                        var bytes2 = await client.GetByteArrayAsync(uiAbsolute);
                        var fileName2 = Path.GetFileName(new Uri(uiAbsolute).AbsolutePath);
                        return File(bytes2, "application/pdf", fileName2);
                    }
                    catch
                    {
                        // Same as above — failure is not fatal; next fallback is redirect.
                    }
                }


                throw new Exception("Unable to fetch PDF from API or UI base");
            }
            catch
            {
                // At this point, both fetch attempts failed.
                // Fallback: Redirect to UI version if possible.
                // Failure here is not ignored, simply the default redirect logic.

                var uiBase = configuration["Ui:BaseUrl"];
                if (fileUrl.StartsWith('/') && !string.IsNullOrWhiteSpace(uiBase))
                {
                    return Redirect($"{uiBase.TrimEnd('/')}{fileUrl}");
                }
                return Redirect(result.Data.FileUrl);
            }
        }

        private void ConvertTreeTypeUrlsToAbsolute(TreeTypeDTO tree)
        {
            var apiPublic = configuration["Api:PublicBaseUrl"]?.TrimEnd('/');
            var baseUrl = !string.IsNullOrWhiteSpace(apiPublic)
                ? apiPublic
                : $"{Request.Scheme}://{Request.Host}{Request.PathBase}".TrimEnd('/');
            foreach (var img in tree.TreeImages.Where(img => !string.IsNullOrWhiteSpace(img.FileUrl)))
            {
                img.FileUrl = img.FileUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? img.FileUrl : $"{baseUrl}{img.FileUrl}";
            }

            foreach (var instr in tree.Instructions.Where(instr => !string.IsNullOrWhiteSpace(instr.FileUrl)))
            {
                instr.FileUrl = instr.FileUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? instr.FileUrl : $"{baseUrl}{instr.FileUrl}";
            }

            if (!string.IsNullOrWhiteSpace(tree.QrCodeUrl))
                tree.QrCodeUrl = tree.QrCodeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? tree.QrCodeUrl : $"{baseUrl}{tree.QrCodeUrl}";
        }

        [HttpPut("{treeTypeId:int}")]
        public async Task<IActionResult> Update(int treeTypeId, [FromForm] EditTreeTypeRequestDTO dto)
        {
            if (dto == null) return BadRequest("Payload ontbreekt.");
            if (dto.Id == 0) dto.Id = treeTypeId;
            if (dto.Id != treeTypeId) return BadRequest("Id komt niet overeen met route.");

            var allowedImageTypes = new[] { "image/png", "image/jpeg" };
            var allowedPdfTypes = new[] { "application/pdf" };
            const long maxImageSize = 20 * 1024 * 1024; // 20MB
            const long maxPdfSize = 50 * 1024 * 1024;  // 50MB

            if (dto.Images != null)
            {
                foreach (var img in dto.Images)
                {
                    if (img.Length == 0) return BadRequest("Leeg afbeeldingsbestand niet toegestaan.");
                    if (!allowedImageTypes.Contains(img.ContentType)) return BadRequest("Ongeldig afbeeldingstype.");
                    if (img.Length > maxImageSize) return BadRequest("Afbeelding is te groot (max 20MB).");
                }
            }

            if (dto.Instructions != null)
            {
                foreach (var pdf in dto.Instructions
                    .Select(i => i.Pdf)
                    .Where(pdf => pdf != null))
                {
                    if (!allowedPdfTypes.Contains(pdf.ContentType))
                        return BadRequest("Ongeldig PDF-type.");

                    if (pdf.Length > maxPdfSize)
                        return BadRequest("PDF is te groot (max 50MB).");
                }
            }

            var root = Path.Combine(env.ContentRootPath, "wwwroot");

            var savedImageUrls = new List<string>();
            if (dto.ExistingImages != null && dto.ExistingImages.Any())
                savedImageUrls.AddRange(dto.ExistingImages.Where(x => !string.IsNullOrWhiteSpace(x)));

            if (dto.Images != null)
            {
                var imgDir = Path.Combine(root, "Files", "TreeTypes");
                Directory.CreateDirectory(imgDir);

                foreach (var img in dto.Images)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(img.FileName)}";
                    var path = Path.Combine(imgDir, fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    await img.CopyToAsync(stream);

                    savedImageUrls.Add($"/Files/TreeTypes/{fileName}");
                }
            }

            var savedInstructions = new List<EditTreeTypeCommand.InstructionInput>();
            if (dto.Instructions != null)
            {
                var instrDir = Path.Combine(root, "Files", "TreeInstructions");
                Directory.CreateDirectory(instrDir);

                foreach (var i in dto.Instructions)
                {
                    var hasPdf = i.Pdf != null;
                    var hasExisting = !string.IsNullOrWhiteSpace(i.ExistingFileUrl);
                    if (!hasPdf && !hasExisting) continue;
                    if (string.IsNullOrWhiteSpace(i.Season)) continue;

                    var fileUrl = i.ExistingFileUrl ?? string.Empty;
                    if (hasPdf)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(i.Pdf.FileName)}";
                        var path = Path.Combine(instrDir, fileName);
                        using var stream = new FileStream(path, FileMode.Create);
                        await i.Pdf.CopyToAsync(stream);

                        fileUrl = $"/Files/TreeInstructions/{fileName}";
                    }

                    savedInstructions.Add(new EditTreeTypeCommand.InstructionInput
                    {
                        Season = i.Season,
                        FileUrl = fileUrl
                    });
                }
            }

            var cmd = new EditTreeTypeCommand
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                ImageUrls = savedImageUrls,
                Instructions = savedInstructions
            };

            var result = await mediator.Send(cmd);
            if (!result.Succeeded) return BadRequest(result);

            if (result.Data != null)
                ConvertTreeTypeUrlsToAbsolute(result.Data);

            return Ok(result);
        }

        [HttpDelete("{treeTypeId:int}")]
        public async Task<IActionResult> Delete(int treeTypeId)
        {
            var all = await mediator.Send(new GetAllTreeTypesQuery { PageNr = 1, PageSize = int.MaxValue });
            var toDelete = all.FirstOrDefault(t => t.Id == treeTypeId);

            if (toDelete != null)
            {
                string? MapToLocalPath(string? url)
                {
                    if (string.IsNullOrWhiteSpace(url)) return null;

                    if (url.StartsWith('/'))
                        return Path.Combine(env.ContentRootPath, "wwwroot", url.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

                    if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                    {
                        var pathPart = uri.AbsolutePath.TrimStart('/');
                        if (!string.IsNullOrWhiteSpace(pathPart))
                            return Path.Combine(env.ContentRootPath, "wwwroot", pathPart.Replace("/", Path.DirectorySeparatorChar.ToString()));
                    }

                    return null;
                }

                void DeleteLocalFile(string? url)
                {
                    var localPath = MapToLocalPath(url);
                    if (string.IsNullOrWhiteSpace(localPath)) return;
                    if (System.IO.File.Exists(localPath))
                    {
                        try 
                        { 
                            System.IO.File.Delete(localPath); 
                        } 
                        catch (Exception ex)
                        {
                            // Non-critical: failure to delete local file should not stop the API from
                            // deleting the database record. Log the error so it can be investigated.
                            Console.WriteLine($"Could not delete file '{localPath}': {ex.Message}");
                        }
                    }
                }

                foreach (var img in toDelete.TreeImages)
                    DeleteLocalFile(img.FileUrl);

                foreach (var instr in toDelete.Instructions)
                    DeleteLocalFile(instr.FileUrl);

                DeleteLocalFile(toDelete.QrCodeUrl);
            }

            var result = await mediator.Send(new DeleteTreeTypeCommand { Id = treeTypeId });
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("archive")]
        public async Task<IActionResult> EditArchive([FromBody] EditTreeTypeArchivedCommand command)
        {
            var result = await mediator.Send(command);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(result);
        }

    }
}
