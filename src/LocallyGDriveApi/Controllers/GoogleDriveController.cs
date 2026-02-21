using GoogleServices.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LocallyGDriveApi.Shared.Helpers;

namespace LocallyGDriveApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleDriveController(IGoogleDriveService googleDriveService, ILogger<GoogleDriveController> logger) : ControllerBase
    {
        private readonly IGoogleDriveService _googleDriveService = googleDriveService;
        private readonly ILogger<GoogleDriveController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            try
            {
                var files = await _googleDriveService.ListFilesAsync(
                    pageSize:10, 
                    pageToken: null,
                    query: null,
                    orderBy: null,
                    includeTrashed: true,
                    cancellationToken: cancellationToken
                );

                if (files is null)
                {
                    return NotFound();
                }

                return Ok(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception was throw");
                
                return this.InternalServerError(ex.Message);
            }
        }
    }
}
