using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using ERP.API.Models;
using ERP.API.Services;
using ERP.API.Models.DTOs;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesDocumentController : ControllerBase
    {
        private readonly SalesDocumentService _documentService;
        private readonly SalesSummaryService _summaryService;

        public SalesDocumentController(
            SalesDocumentService documentService,
            SalesSummaryService summaryService)
        {
            _documentService = documentService;
            _summaryService = summaryService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(50_000_000)] // 50 MB limit, adjust as needed
        public async Task<IActionResult> UploadDocument([FromForm] UploadSalesDocumentDto dto)
        {
            var file = dto.File;
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            // Build local path: /uploads/documents/{stage}/{stageItemId}/
            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "documents", dto.Stage ?? "General", dto.StageItemId ?? "0");
            if (!Directory.Exists(uploadsRoot))
                Directory.CreateDirectory(uploadsRoot);

            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsRoot, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Build relative URL for storage in DB (for serving via API/static files)
            var relativePath = $"/uploads/documents/{dto.Stage ?? "General"}/{dto.StageItemId ?? "0"}/{fileName}";

            var document = new SalesDocument
            {
                FileUrl = relativePath,
                Title = dto.Title,
                FileType = Path.GetExtension(fileName),
                FileName = fileName,
                IconUrl = "/icons/document.png",
                Description = dto.Description,
                DocumentId = dto.DocumentId,
                Stage = dto.Stage,
                StageItemId = dto.StageItemId,
                IsActive = true,
                DateCreated = DateTime.UtcNow
            };

            var id = await _documentService.CreateAsync(document);

            // Create a summary entry
            var summary = new SalesSummary
            {
                IconUrl = document.IconUrl,
                Title = "Document uploaded",
                Description = $"Document uploaded: {document.Title}",
                DateTime = DateTime.UtcNow,
                IsActive = true,
                Stage = document.Stage,
                StageItemId = document.StageItemId,
                Entities = System.Text.Json.JsonSerializer.Serialize(new { DocumentId = id })
            };

            await _summaryService.CreateAsync(summary);

            return Ok(new { id, fileUrl = relativePath });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesDocument>>> GetAll()
        {
            var documents = await _documentService.GetAllAsync("isactive = true ORDER BY date_created DESC", new { });
            return Ok(documents);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SalesDocument>> GetById(int id)
        {
            var document = await _documentService.GetByIdAsync(id);
            if (document == null)
                return NotFound($"Document with ID {id} not found");

            return Ok(document);
        }

        [HttpGet("stage/{stage}/{stageItemId}")]
        public async Task<ActionResult<IEnumerable<SalesDocument>>> GetByStage(string stage, string stageItemId)
        {
            var documents = await _documentService.GetAllAsync(
                "stage = @Stage AND stage_item_id = @StageItemId AND isactive = true ORDER BY date_created DESC",
                new { Stage = stage, StageItemId = stageItemId });
            return Ok(documents);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateSalesDocumentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var document = new SalesDocument
            {
                FileUrl = dto.FileUrl,
                Title = dto.Title,
                FileType = dto.FileType,
                FileName = dto.FileName,
                IconUrl = dto.IconUrl,
                Description = dto.Description,
                DocumentId = dto.DocumentId,
                Stage = dto.Stage,
                StageItemId = dto.StageItemId,
                IsActive = true,
                DateCreated = DateTime.UtcNow
            };

            var id = await _documentService.CreateAsync(document);

            // Create a summary entry
            var summary = new SalesSummary
            {
                IconUrl = document.IconUrl ?? "/icons/document.png",
                Title = "Document added",
                Description = $"New document added: {document.Title}",
                DateTime = DateTime.UtcNow,
                IsActive = true,
                Stage = document.Stage,
                StageItemId = document.StageItemId,
                Entities = System.Text.Json.JsonSerializer.Serialize(new { DocumentId = id })
            };

            await _summaryService.CreateAsync(summary);

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> Update(int id, [FromBody] SalesDocument document)
        {
            if (id != document.Id)
                return BadRequest("ID mismatch");

            var existingDocument = await _documentService.GetByIdAsync(id);
            if (existingDocument == null)
                return NotFound($"Document with ID {id} not found");

            document.DateUpdated = DateTime.UtcNow;
            await _documentService.UpdateAsync(document);

            // Create a summary entry for the update
            var summary = new SalesSummary
            {
                IconUrl = document.IconUrl ?? "/icons/document.png",
                Title = "Document updated",
                Description = $"Document updated: {document.Title}",
                DateTime = DateTime.UtcNow,
                IsActive = true,
                Stage = document.Stage,
                StageItemId = document.StageItemId,
                Entities = System.Text.Json.JsonSerializer.Serialize(new { DocumentId = id })
            };

            await _summaryService.CreateAsync(summary);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var document = await _documentService.GetByIdAsync(id);
            if (document == null)
                return NotFound($"Document with ID {id} not found");

            // Soft delete by setting isactive to false
            document.IsActive = false;
            document.DateUpdated = DateTime.UtcNow;
            await _documentService.UpdateAsync(document);

            // Create a summary entry for the deletion
            var summary = new SalesSummary
            {
                IconUrl = document.IconUrl ?? "/icons/document.png",
                Title = "Document deleted",
                Description = $"Document deleted: {document.Title}",
                DateTime = DateTime.UtcNow,
                IsActive = true,
                Stage = document.Stage,
                StageItemId = document.StageItemId,
                Entities = System.Text.Json.JsonSerializer.Serialize(new { DocumentId = id })
            };

            await _summaryService.CreateAsync(summary);

            return NoContent();
        }
    }
}