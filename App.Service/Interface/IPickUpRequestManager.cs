using App.Domain.DTOs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Service.Interface
{
    public interface IPickUpRequestManager
    {

        Task<IEnumerable<PickupRequestDto>> GetAllByUserAsync(IdentityUser currentUser);
        Task<PickupRequestDto> AddAsync(PickupRequestDto dto, IdentityUser currentUser);
        Task DeleteAsync(int requestId, IdentityUser currentUser);
        Task AssignDriverAsync(int requestId, int driverId, IdentityUser currentUser);
        Task UnassignDriverAsync(int requestId, IdentityUser currentUser);
        Task UpdateAsync(PickupRequestDto dto, IdentityUser currentUser);
    }
}
