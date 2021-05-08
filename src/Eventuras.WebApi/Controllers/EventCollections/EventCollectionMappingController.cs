using Eventuras.Services.EventCollections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Eventuras.Services.Exceptions;

namespace Eventuras.WebApi.Controllers.EventCollections
{
    [ApiVersion("3")]
    [Authorize(Policy = Constants.Auth.AdministratorRole)]
    [Route("api/v{version:apiVersion}/events/{id}/collections/{collectionId}")]
    [ApiController]
    public class EventCollectionMappingController : ControllerBase
    {
        private readonly IEventCollectionMappingService _collectionMappingService;

        public EventCollectionMappingController(IEventCollectionMappingService collectionMappingService)
        {
            _collectionMappingService = collectionMappingService ?? throw new ArgumentNullException(nameof(collectionMappingService));
        }

        [HttpPut]
        public async Task<IActionResult> Create(int id, int collectionId)
        {
            try
            {
                await _collectionMappingService.AddEventToCollectionAsync(id, collectionId);
                return Ok();
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (NotAccessibleException e)
            {
                return Forbid(e.Message); 
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Remove(int id, int collectionId)
        {
            try
            {
                await _collectionMappingService.RemoveEventFromCollectionAsync(id, collectionId);
                return Ok();
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (NotAccessibleException e)
            {
                return Forbid(e.Message); 
            }
        }
    }
}
