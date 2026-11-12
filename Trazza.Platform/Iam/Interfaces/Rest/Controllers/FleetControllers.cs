using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TrazzaPlatform.API.Iam.Domain.Model.Commands;
using TrazzaPlatform.API.Iam.Domain.Model.ValueObjects;
using TrazzaPlatform.API.Iam.Domain.Services;
using TrazzaPlatform.API.Iam.Infrastructure.Pipeline;
using TrazzaPlatform.API.Iam.Interfaces.Rest.Resources;
using TrazzaPlatform.API.Iam.Interfaces.Rest.Transform;
using TrazzaPlatform.API.Resources;
using TrazzaPlatform.API.Shared.Interfaces.Rest;

namespace TrazzaPlatform.API.Iam.Interfaces.Rest.Controllers;

/// <summary>
///     Trucks of the signed-in carrier. Identified by their public id.
/// </summary>
[Route("api/v1/[controller]")]
[Authorize(Roles.Carrier)]
public class VehiclesController(
    IFleetCommandService fleetCommandService,
    IFleetQueryService fleetQueryService,
    IStringLocalizer<SharedResource> localizer) : TrazzaControllerBase(localizer)
{
    private ulong UserId => HttpContext.GetAuthenticatedUser().Id;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok((await fleetQueryService.ListVehiclesAsync(UserId, ct)).Select(IamResourceAssembler.ToResource));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveVehicleResource resource, CancellationToken ct) =>
        CreatedFromResult(await fleetCommandService.Handle(
                new CreateVehicleCommand(UserId, IamResourceAssembler.ToVehicleData(resource)), ct),
            IamResourceAssembler.ToResource);

    [HttpPut("{vehicleId:guid}")]
    public async Task<IActionResult> Update(Guid vehicleId, [FromBody] SaveVehicleResource resource,
        CancellationToken ct) =>
        FromResult(await fleetCommandService.Handle(
                new UpdateVehicleCommand(UserId, vehicleId, IamResourceAssembler.ToVehicleData(resource)), ct),
            IamResourceAssembler.ToResource);

    [HttpDelete("{vehicleId:guid}")]
    public async Task<IActionResult> Deactivate(Guid vehicleId, CancellationToken ct) =>
        NoContentFromResult(await fleetCommandService.Handle(new DeactivateVehicleCommand(UserId, vehicleId), ct));
}

/// <summary>
///     Drivers employed by the signed-in carrier.
/// </summary>
[Route("api/v1/[controller]")]
[Authorize(Roles.Carrier)]
public class DriversController(
    IFleetCommandService fleetCommandService,
    IFleetQueryService fleetQueryService,
    IStringLocalizer<SharedResource> localizer) : TrazzaControllerBase(localizer)
{
    private ulong UserId => HttpContext.GetAuthenticatedUser().Id;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok((await fleetQueryService.ListDriversAsync(UserId, ct)).Select(IamResourceAssembler.ToResource));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveDriverResource resource, CancellationToken ct)
    {
        var data = IamResourceAssembler.ToDriverData(resource);
        if (data.IsFailure) return ErrorResult(data.Error!);
        return CreatedFromResult(await fleetCommandService.Handle(new CreateDriverCommand(UserId, data.Value!), ct),
            IamResourceAssembler.ToResource);
    }

    [HttpPut("{driverId}")]
    public async Task<IActionResult> Update(ulong driverId, [FromBody] SaveDriverResource resource, CancellationToken ct)
    {
        var data = IamResourceAssembler.ToDriverData(resource);
        if (data.IsFailure) return ErrorResult(data.Error!);
        return FromResult(await fleetCommandService.Handle(new UpdateDriverCommand(UserId, driverId, data.Value!), ct),
            IamResourceAssembler.ToResource);
    }

    [HttpDelete("{driverId}")]
    public async Task<IActionResult> Deactivate(ulong driverId, CancellationToken ct) =>
        NoContentFromResult(await fleetCommandService.Handle(new DeactivateDriverCommand(UserId, driverId), ct));
}

/// <summary>
///     Frequent pick-up and delivery addresses of the signed-in user, with real coordinates.
/// </summary>
[Route("api/v1/[controller]")]
[Authorize]
public class SavedAddressesController(
    IFleetCommandService fleetCommandService,
    IFleetQueryService fleetQueryService,
    IStringLocalizer<SharedResource> localizer) : TrazzaControllerBase(localizer)
{
    private ulong UserId => HttpContext.GetAuthenticatedUser().Id;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok((await fleetQueryService.ListSavedAddressesAsync(UserId, ct)).Select(IamResourceAssembler.ToResource));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSavedAddressResource resource, CancellationToken ct) =>
        CreatedFromResult(await fleetCommandService.Handle(new CreateSavedAddressCommand(UserId, resource.Label,
                resource.Address, resource.District, resource.Latitude, resource.Longitude), ct),
            IamResourceAssembler.ToResource);

    [HttpDelete("{savedAddressId}")]
    public async Task<IActionResult> Delete(ulong savedAddressId, CancellationToken ct) =>
        NoContentFromResult(await fleetCommandService.Handle(new DeleteSavedAddressCommand(UserId, savedAddressId), ct));
}
