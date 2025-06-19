using App.Domain.DTOs;
using App.Service.Interface;
using App.Service.Managers;
using App.Web.ViewModels;
using AutoMapper;

public class ShopNameResolver : IValueResolver<PickupRequestDto, PickUpRequestViewModel, string?>
{
    private readonly IShopManager _shopManager;

    public ShopNameResolver(IShopManager shopManager)
    {
        _shopManager = shopManager;
    }

    public string? Resolve(PickupRequestDto source, PickUpRequestViewModel destination, string? destMember, ResolutionContext context)
    {
        var a= _shopManager.GetShopNameByIdAsync(source.ShopId).Result; // Use .Result to keep it sync
        if (a == null)
        {
            return "null";
        }
        return a;
    }
}
