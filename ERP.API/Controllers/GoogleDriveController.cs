

using Microsoft.AspNetCore.Mvc;
using ERP.API.Services;
using ERP.API.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class GoogleDriveController : ControllerBase
{
	private readonly IGoogleDriveService _googleDriveService;

	public GoogleDriveController(IGoogleDriveService googleDriveService)
	{
		_googleDriveService = googleDriveService;
	}

	// Create a folder
	[HttpPost("create-folder")]
	public async Task<IActionResult> CreateFolder([FromBody] ERP.API.Models.CreateFolderRequest request)
	{
		try
		{
			var folderId = await _googleDriveService.CreateFolderAsync(request.FolderName, request.ParentFolderId);
			return Ok(new { FolderId = folderId });
		}
		catch (InvalidOperationException ex)
		{
			return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = ex.Message });
		}
		catch (Exception ex)
		{
			return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
		}
	}

	// Upload a document
	[HttpPost("upload")]
	[ProducesResponseType(typeof(ERP.API.Models.DocumentUploadResponse), 200)]
	[ProducesResponseType(typeof(ERP.API.Models.DocumentUploadResponse), 400)]
	public async Task<IActionResult> UploadDocument([FromForm] ERP.API.Models.UploadDocumentRequest request)
	{
		var result = await _googleDriveService.UploadFileAsync(request.File, request.FolderId, request.MakePublic);
		if (result.IsSuccess)
			return Ok(result);
		return BadRequest(result);
	}

	// Download a document
	[HttpGet("download/{fileId}")]
	public async Task<IActionResult> DownloadDocument(string fileId)
	{
		var result = await _googleDriveService.DownloadFileAsync(fileId);
		if (result.Stream == null)
			return NotFound();
		return File(result.Stream, result.MimeType ?? "application/octet-stream", result.FileName ?? "document");
	}

	// List documents in a folder
	[HttpGet("list")]
	public async Task<IActionResult> ListDocuments([FromQuery] string? folderId = null, [FromQuery] int maxResults = 100)
	{
		var files = await _googleDriveService.ListFilesAsync(folderId, maxResults);
		return Ok(files);
	}
}

