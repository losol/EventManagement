using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Eventuras.Domain;
using Eventuras.Services.EventCollections;
using Eventuras.Services.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Eventuras.WebApi.Controllers.EventCollections
{
    [ApiVersion("3")]
    [Route("v{version:apiVersion}/eventcollections")]
    [ApiController]
    [Authorize(Policy = Constants.Auth.AdministratorRole)]
    public class EventCollectionController : ControllerBase
    {
        private readonly IEventCollectionManagementService _service;

        public EventCollectionController(IEventCollectionManagementService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> List()
        {
            var collections = await _service.ListCollectionsAsync();
            return Ok(collections
                .Select(c => new EventCollectionDto(c))
                .ToArray());
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var c = await _service.GetCollectionByIdAsync(id);
                return Ok(new EventCollectionDto(c));
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EventCollectionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.FormatErrors());
            }

            try
            {
                var collection = new EventCollection();
                dto.CopyTo(collection);
                await _service.CreateCollectionAsync(collection);
                return Ok(collection);
            }
            catch (NotAccessibleException e)
            {
                return Forbid(e.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EventCollectionDto dto)
        {
            try
            {
                var collection = await _service.GetCollectionByIdAsync(id);
                dto.CopyTo(collection);
                await _service.UpdateCollectionAsync(collection);
                return Ok(new EventCollectionDto(collection));
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var collection = await _service.GetCollectionByIdAsync(id);
                await _service.DeleteCollectionAsync(collection);
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

    public class EventCollectionDto
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Id { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int OrganizationId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Slug { get; set; }

        public string Description { get; set; }

        public bool Featured { get; set; }

        public string FeaturedImageUrl { get; set; }

        public string FeaturedImageCaption { get; set; }

        public EventCollectionDto()
        {
        }

        public EventCollectionDto(EventCollection collection)
        {
            Id = collection.CollectionId;
            OrganizationId = collection.OrganizationId;
            Name = collection.Name;
            Slug = collection.Slug;
            Description = collection.Description;
            Featured = collection.Featured;
            FeaturedImageUrl = collection.FeaturedImageUrl;
            FeaturedImageCaption = collection.FeaturedImageCaption;
        }

        public void CopyTo(EventCollection collection)
        {
            collection.OrganizationId = OrganizationId;
            collection.Name = Name;
            collection.Description = Description;
            collection.Slug = Slug;
            collection.Featured = Featured;
            collection.FeaturedImageUrl = FeaturedImageUrl;
            collection.FeaturedImageCaption = FeaturedImageCaption;
        }
    }
}
