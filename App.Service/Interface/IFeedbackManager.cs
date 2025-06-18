using App.Domain.DTOs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Service.Interface
{
    public interface IFeedbackManager
    {
        Task<IEnumerable<FeedbackDto>> GetAllForCompanyAsync(IdentityUser currentUser);
        Task<FeedbackDto> AddAsync(FeedbackDto dto);
        Task DeleteAsync(int feedbackId, IdentityUser currentUser);
    }
}
