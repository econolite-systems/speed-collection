// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Authorization;
using Econolite.Ode.Extensions.SpeedStatus;
using Econolite.Ode.Models.SpeedStatus;
using Econolite.Ode.Repository.SpeedStatus;
using Econolite.Ode.Status.Speed;
using Microsoft.AspNetCore.Mvc;

namespace Econolite.Ode.Api.SpeedStatus.Controllers
{
    [Route("speed-status")]
    [ApiController]
    [AuthorizeOde(MoundRoadRole.ReadOnly)]
    public class SpeedStatusController : ControllerBase
    {
        private readonly ISpeedStatusRepository _speedStatusRepository;

        public SpeedStatusController(ISpeedStatusRepository speedStatusRepository, IConfiguration configuration)
        {
            _speedStatusRepository = speedStatusRepository;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SpeedStatusModel))]
        public async Task<IActionResult> GetAsync([FromQuery] Guid deviceId)
        {
            var speedStatus = await _speedStatusRepository.GetAsync(deviceId);

            return Ok(speedStatus?.ToModel());
        }

        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SpeedStatusModel[]))]
        public async Task<IActionResult> GetAllAsync()
        {
            var speedStatuses = await _speedStatusRepository.GetAllAsync();

            return Ok(speedStatuses.Select(s => s?.ToModel()));
        }

        [HttpPost("publish")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(long))]
        [AuthorizeOde(MoundRoadRole.Contributor)]
        public async Task<IActionResult> PublishAsync(Guid tenantId, Guid deviceId, SpeedEvent? speedEvent)
        {
            await _speedStatusRepository.PutStatusAsync(tenantId, deviceId, speedEvent);

            return Ok();
        }
    }
}
