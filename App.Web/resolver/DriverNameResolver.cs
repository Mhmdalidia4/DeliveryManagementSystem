using App.Domain.DTOs;
using App.Service.Interface;
using App.Service.Managers;
using App.Web.ViewModels;
using AutoMapper;

public class DriverNameResolver : IValueResolver<PickupRequestDto, PickUpRequestViewModel, string?>
{
    private readonly IDriverManager _driverManager;

    public DriverNameResolver(IDriverManager driverManager)
    {
        _driverManager = driverManager;
    }

    public string? Resolve(PickupRequestDto source, PickUpRequestViewModel destination, string? destMember, ResolutionContext context)
    {
        try
        {
            var task = _driverManager.GetDriverNameByIdAsync(source.AssignedDriverId ?? 0);
            task.Wait(); // Safe if the method is fast + you control the code
            return task.Result ?? "null";
        }
        catch
        {
            return "null";
        }
    }
}
