using AcyclicaService.Helpers;
using AcyclicaService.Repository;
using AcyclicaService.Services.Api;
using AcyclicaService.Services.Database;
using Econolite.Ode.Authorization;
using EntityService;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("segment")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class SegmentController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly IAcyclicaApiService _acyclicaApiService;
    private readonly ISegmentInventoryService _segmentInventoryService;
    private readonly ISdk<IEntitiesClient> _entitiesClient;

    public SegmentController(IAcyclicaApiService acyclicaApiService,
        ISegmentInventoryService segmentInventoryService, ISegmentTravelDataService travelDataService,
        ILoggerFactory loggerFactory, ISdk<IEntitiesClient> entitiesClient)
    {
        _acyclicaApiService = acyclicaApiService;
        _logger = loggerFactory.CreateLogger(GetType().Name);
        _segmentInventoryService = segmentInventoryService;
        _entitiesClient = entitiesClient;
    }

    [HttpPut("segmentInventory/GetFromApiAndInsert")]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> GetSegmentInventoryAndInsertAsync()
    {
        try
        {
            var result = await _acyclicaApiService.GetSegmentInventoryAsync();
            foreach (var segment in result)
            {
                _logger.LogInformation($"Inserting Segment {segment.SegmentId} ");
                await _segmentInventoryService.CreateAsync(AcyclicaServiceExtensions.ToDoc(segment));
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex}");
            return StatusCode(500, ex.Message);
        }
    }


    [HttpGet("segmentInventory/GetFromApi")]
    public async Task<IActionResult> GetSegmentInventoryAsync()
    {
        try
        {
            var result = await _acyclicaApiService.GetSegmentInventoryAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex}");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("segmentInventory/getFromDatabase")]
    public async Task<IActionResult> GetSegmentInventoryFromDatabaseAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all Segments from Mongo");
            return Ok(await _segmentInventoryService.GetAllAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex}");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("segmentInventory/updateDatabase")]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> UpdateSegmentsDatabaseAsync(SegmentInventoryDocument segmentInventoryDto)
    {
        try
        {
            await _segmentInventoryService.UpdateAsync(segmentInventoryDto);
            _logger.LogInformation("Updating Segment");
            return Ok("Database updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex}");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("segmentInventory/deleteSegment")]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> DeleteTravelDataAsync(SegmentInventoryDocument segmentInventoryDto)
    {
        try
        {
            await _segmentInventoryService.DeleteAsync(segmentInventoryDto.Id);
            _logger.LogInformation("Deleting Segment Data");
            return Ok("Database updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex}");
            return StatusCode(500, ex.Message);
        }
    }
}
