using HousingApi.Messages;
using HousingApi.Models;
using HousingApi.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace HousingApi.Controllers;

[ApiController]
[Route("api/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly HousingApplicationsService _applicationsService;
    private readonly HousingLocationsService _locationsService;
    private readonly RabbitMqPublisher _publisher;

    public ApplicationsController(
        HousingApplicationsService applicationsService,
        HousingLocationsService locationsService,
        RabbitMqPublisher publisher
    )
    {
        _applicationsService = applicationsService;
        _locationsService = locationsService;
        _publisher = publisher;
    }

    [HttpGet]
    public async Task<ActionResult<List<HousingApplicationResponse>>> Get()
    {
        var applications = await _applicationsService.GetAsync();

        // shortened version to map to DTO: applications.Select(application => MapToDto(application))
        return applications.Select(MapToDto).ToList();
    }

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<HousingApplicationResponse>> Get(string id)
    {
        Console.WriteLine("Get application by id: " + id);
        var application = await _applicationsService.GetAsync(id);

        if (application is null)
        {
            Console.WriteLine("Get application by id: " + id);
            return NotFound();
        }

        return MapToDto(application!);
    }

    [HttpPost]
    public async Task<ActionResult<HousingApplicationResponse>> Post(
        CreateHousingApplicationRequest createApplicationDto
    )
    {
        if (!ObjectId.TryParse(createApplicationDto.HousingId, out _))
        {
            ModelState.AddModelError(
                nameof(createApplicationDto.HousingId),
                "housingId must be a valid ObjectId."
            );
            return ValidationProblem(ModelState);
        }

        var location = await _locationsService.GetAsync(createApplicationDto.HousingId);

        if (location is null)
        {
            ModelState.AddModelError(
                nameof(createApplicationDto.HousingId),
                "Referenced housing location was not found."
            );
            return ValidationProblem(ModelState);
        }

        var application = new HousingApplicationEntity
        {
            HousingId = createApplicationDto.HousingId,
            FirstName = createApplicationDto.FirstName,
            LastName = createApplicationDto.LastName,
            Email = createApplicationDto.Email,
            CreatedAt = DateTime.UtcNow,
        };

        // Persist to Mongo first. If this throws, the event is never published.
        await _applicationsService.CreateAsync(application);

        // Publish only after a successful insert — application.Id is now set by Mongo.
        _publisher.Publish(
            new HousingApplicationCreated(
                ApplicationId: application.Id!,
                HousingId: application.HousingId,
                FirstName: application.FirstName,
                LastName: application.LastName,
                Email: application.Email,
                CreatedAt: application.CreatedAt
            )
        );

        return CreatedAtAction(nameof(Get), new { id = application.Id }, MapToDto(application));
    }

    private static HousingApplicationResponse MapToDto(HousingApplicationEntity application) =>
        new()
        {
            Id = application.Id ?? string.Empty,
            HousingId = application.HousingId,
            FirstName = application.FirstName,
            LastName = application.LastName,
            Email = application.Email,
            CreatedAt = application.CreatedAt,
        };
}
