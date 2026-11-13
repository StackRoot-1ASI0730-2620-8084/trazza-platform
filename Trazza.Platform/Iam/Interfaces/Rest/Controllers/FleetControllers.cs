using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Trazza.Platform.Iam.Domain.Model.Commands;
using Trazza.Platform.Iam.Domain.Model.ValueObjects;
using Trazza.Platform.Iam.Domain.Services;
using Trazza.Platform.Iam.Infrastructure.Pipeline;
using Trazza.Platform.Iam.Interfaces.Rest.Resources;
using Trazza.Platform.Iam.Interfaces.Rest.Transform;
using Trazza.Platform.Resources;
using Trazza.Platform.Shared.Interfaces.Rest;

namespace Trazza.Platform.Iam.Interfaces.Rest.Controllers;

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
