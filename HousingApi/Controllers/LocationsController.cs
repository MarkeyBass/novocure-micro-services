using HousingApi.Models;
using HousingApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HousingApi.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    private readonly HousingLocationsService _locationsService;

    public LocationsController(HousingLocationsService locationsService) =>
        _locationsService = locationsService;

    [HttpGet]
    public async Task<List<HousingLocationEntity>> Get() => await _locationsService.GetAsync();

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<HousingLocationEntity>> Get(string id)
    {
        var location = await _locationsService.GetAsync(id);

        if (location is null)
        {
            return NotFound();
        }

        return location;
    }

    [HttpPost]
    public async Task<IActionResult> Post(HousingLocationEntity newLocation)
    {
        await _locationsService.CreateAsync(newLocation);

        return CreatedAtAction(nameof(Get), new { id = newLocation.Id }, newLocation);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, HousingLocationEntity updatedLocation)
    {
        var location = await _locationsService.GetAsync(id);

        if (location is null)
        {
            return NotFound();
        }

        updatedLocation.Id = location!.Id;

        await _locationsService.UpdateAsync(id, updatedLocation);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var location = await _locationsService.GetAsync(id);

        if (location is null)
        {
            return NotFound();
        }

        await _locationsService.RemoveAsync(id);

        return NoContent();
    }
}
