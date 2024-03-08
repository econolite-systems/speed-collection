using AcyclicaService.Helpers;
using AcyclicaService.Repository;
using AcyclicaService.Services.Api;
using AcyclicaService.Services.Database;
using Econolite.Ode.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SpeedCollectionWebApi.Controllers
{
    [Route("travel")]
    [ApiController]
    [AuthorizeOde(MoundRoadRole.ReadOnly)]
    public class TravelController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IAcyclicaApiService _acyclicaApiService;
        private readonly ISegmentTravelDataService _travelDataService;

        public TravelController(IAcyclicaApiService acyclicaApiService, ISegmentTravelDataService travelDataService, ILoggerFactory loggerFactory)
        {
            _acyclicaApiService = acyclicaApiService;
            _logger = loggerFactory.CreateLogger(GetType().Name);
            _travelDataService = travelDataService;
        }

        [HttpGet("travelData/{segmentId}/{startTimeEpoch}/{endTimeEpoch}/{period}")]
        public async Task<IActionResult> GetSegmentTravelTimeAsync(int segmentId, long startTimeEpoch, long endTimeEpoch, int period)
        {
            try
            {
                var result = await _acyclicaApiService.GetSegmentTravelDataAsync("json", segmentId, startTimeEpoch, endTimeEpoch, period);
                _logger.LogInformation("Fetching Travel Data");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("travelData/{segmentId}/{startTimeEpoch}/{endTimeEpoch}/{period}/GetAndInsert")]
        [AuthorizeOde(MoundRoadRole.Contributor)]
        public async Task<IActionResult> GetSegmentTravelTimeAndInsertAsync(int segmentId, long startTimeEpoch, long endTimeEpoch, int period)
        {
            try
            {
                var result = await _acyclicaApiService.GetSegmentTravelDataAsync("json", segmentId, startTimeEpoch, endTimeEpoch, period);
                _logger.LogInformation("Fetching Travel Data");
                foreach (var travelData in result)
                {
                    _logger.LogInformation($"Inserting Travel Data having segment id of {segmentId} ");
                    await _travelDataService.CreateAsync(travelData.ToDoc());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("travelData/getFromDatabase")]
        public async Task<IActionResult> GetTravelDataFromDatabaseAsync()
        {
            try
            {
                _logger.LogInformation("Fetching Travel Data from Mongo");
                return Ok(await _travelDataService.GetAllAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("travelData/updateDatabase")]
        [AuthorizeOde(MoundRoadRole.Contributor)]
        public async Task<IActionResult> UpdateTravelDataDatabaseAsync(TravelDataDocument travelDataDto)
        {
            try
            {
                await _travelDataService.UpdateAsync(travelDataDto);
                _logger.LogInformation("Updating Travel Data");
                return Ok("Database updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("travelData/deleteTravelData")]
        [AuthorizeOde(MoundRoadRole.Contributor)]
        public async Task<IActionResult> DeleteTravelDataAsync(TravelDataDocument travelDataDto)
        {
            try
            {
                await _travelDataService.DeleteAsync(travelDataDto.Id);
                _logger.LogInformation("Deleting Travel Data");
                return Ok("Database updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
