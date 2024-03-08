using AcyclicaService.Services.Api;
using AcyclicaService.Services.Database;
using Econolite.Ode.Authorization;
using Microsoft.AspNetCore.Mvc;
using SegmentCollection.Extensions;
using SegmentCollection.Models;
using SegmentCollection.Services.Database;

namespace SpeedCollectionWebApi.Controllers
{
    [Route("segment-collection")]
    [ApiController]
    [AuthorizeOde(MoundRoadRole.ReadOnly)]
    public class SegmentCollectionSegmentController : ControllerBase
    {
        private readonly ILogger<SegmentCollectionSegmentController> _logger;
        private readonly ISegmentCollectionSegmentService _segmentCollectionSegmentService;

        public SegmentCollectionSegmentController(ISegmentCollectionSegmentService segmentCollectionSegmentService, IConfiguration configuration, ILogger<SegmentCollectionSegmentController> logger)
        {
            _segmentCollectionSegmentService = segmentCollectionSegmentService;
            _logger = logger;
        }

        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SegmentCollectionSegment>))]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok((await _segmentCollectionSegmentService.GetAllAsync()).Select(s => s.ToDto()));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SegmentCollectionSegment))]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            return Ok((await _segmentCollectionSegmentService.GetAsync(id)).ToDto());
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SegmentCollectionSegment))]
        [AuthorizeOde(MoundRoadRole.Contributor)]
        public async Task<IActionResult> CreateAsync(SegmentCollectionSegment longQueueSegment)
        {
            return Ok((await _segmentCollectionSegmentService.CreateAsync(longQueueSegment.ToDoc())).ToDto());
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(long))]
        [AuthorizeOde(MoundRoadRole.Contributor)]
        public async Task<IActionResult> UpdateAsync(SegmentCollectionSegment longQueueSegment)
        {
            await _segmentCollectionSegmentService.UpdateAsync(longQueueSegment.ToDoc());
            return Ok();
        }

        [HttpDelete]
        [AuthorizeOde(MoundRoadRole.Contributor)]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            return Ok(await _segmentCollectionSegmentService.DeleteAsync(id));
        }
    }
}
