using AcyclicaService.Helpers;
using AcyclicaService.Repository;
using AcyclicaService.Services.Api;
using AcyclicaService.Services.Database;
using Econolite.Ode.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SpeedCollectionWebApi.Controllers
{
    [Route("location")]
    [ApiController]
    [AuthorizeOde(MoundRoadRole.ReadOnly)]
    public class LocationController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IAcyclicaApiService _acyclicaApiService;
        private readonly ILocationInventoryService _locationInventoryService;

        public LocationController(IAcyclicaApiService acyclicaApiService, ILocationInventoryService locationInventoryService, ILoggerFactory loggerFactory)
        {
            _acyclicaApiService = acyclicaApiService;
            _logger = loggerFactory.CreateLogger(GetType().Name);
            _locationInventoryService = locationInventoryService;
        }

        [HttpPut("locationInventory/GetFromApiAndInsert")]
        [AuthorizeOde(MoundRoadRole.Contributor)]
        public async Task<IActionResult> GetLocationInventoryAndInsertAsync()
        {
            try
            {
                var result = await _acyclicaApiService.GetLocationInventoryAsync();
                foreach (var location in result.Locations)
                {
                    _logger.LogInformation($"Inserting Location {location.LocationId} ");
                    await _locationInventoryService.CreateAsync(AcyclicaServiceExtensions.ToDoc(location));
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("locationInventory/GetFromApi")]
        public async Task<IActionResult> GetLocationInventoryAsync()
        {
            try
            {
                var result = await _acyclicaApiService.GetLocationInventoryAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("locationInventory/getFromDatabase")]
        public async Task<IActionResult> GetLocationInventoryFromDatabaseAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all Locations from Mongo");
                return Ok(await _locationInventoryService.GetAllAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("locationInventory/updateDatabase")]
        [AuthorizeOde(MoundRoadRole.Contributor)]
        public async Task<IActionResult> UpdateLocationsDatabaseAsync(LocationInventoryDocument locationInventoryDto)
        {
            try
            {
                await _locationInventoryService.UpdateAsync(locationInventoryDto);
                _logger.LogInformation("Updating Location");
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
