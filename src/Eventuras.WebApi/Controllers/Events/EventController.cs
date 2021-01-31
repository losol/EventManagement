using System;
using System.Linq;
using System.Threading.Tasks;
using Eventuras.Services.Events;
using Eventuras.WebApi.Models;
using Eventuras.WebApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventuras.WebApi
{
    [ApiVersion("3")]
    [Authorize(Policy = Constants.Auth.AdministratorRole)]
    [Route("v{version:apiVersion}/events")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventInfoRetrievalService _eventInfoService;

        public EventController(IEventInfoRetrievalService eventInfoService)
        {
            _eventInfoService = eventInfoService;
        }

        // GET: v1/events
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IQueryable<EventDto>>> Get()
        {
            // TODO: add event type filter
            var events = from e in await _eventInfoService.GetUpcomingEventsAsync()
                         select new EventDto()
                         {
                             Id = e.EventInfoId,
                             Name = e.Title,
                             Slug = e.Code,
                             Description = e.Description,
                             StartDate = e.DateStart,
                             EndDate = e.DateEnd,
                             Featured = e.Featured,
                             Location = new LocationDto()
                             {
                                 Name = e.Location
                             }
                         };
            return Ok(events);
        }

        // GET: v1/events/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<EventDto>> Get(int id)
        {
            var eventInfo = await _eventInfoService.GetEventInfoByIdAsync(id);
            if (eventInfo == null)
            {
                return NotFound();
            }

            var dto = new EventDto()
            {
                Id = eventInfo.EventInfoId,
                Name = eventInfo.Title,
                Slug = eventInfo.Code,
                Description = eventInfo.Description,
                StartDate = eventInfo.DateStart,
                EndDate = eventInfo.DateEnd,
                Featured = eventInfo.Featured
            };

            return Ok(dto);
        }

        // POST: api/EventInfo
        [HttpPost]
        [Authorize("events:write")]
        public ActionResult Post([FromBody] string value)
        {
            return Ok("Post a new event, authorized?!");
        }

        // PUT: api/EventInfo/5
        [HttpPut("{id}")]
        [Authorize("events:write")]
        public void Put(int id, [FromBody] string value)
        {
            throw new NotImplementedException();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        [Authorize("events:write")]
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
