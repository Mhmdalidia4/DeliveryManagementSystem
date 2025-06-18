using App.Domain.DTOs;
using App.Domain.Interfaces.Base;
using App.Domain.Models;
using App.Service.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace App.Service.Managers
{
    public class FeedbackManager: IFeedbackManager
    {
        private readonly IBaseRepository<Feedback> _feedbackRepo;
        private readonly IBaseRepository<Company> _companyRepo;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;

        public FeedbackManager(
            IBaseRepository<Feedback> feedbackRepo,
            IBaseRepository<Company> companyRepo,
            UserManager<IdentityUser> userManager,
            IMapper mapper)
        {
            _feedbackRepo = feedbackRepo;
            _companyRepo = companyRepo;
            _userManager = userManager;
            _mapper = mapper;
        }

        // 1. Get all feedback for the current company user
        public async Task<IEnumerable<FeedbackDto>> GetAllForCompanyAsync(IdentityUser currentUser)
        {
            if (!await _userManager.IsInRoleAsync(currentUser, "company"))
                throw new UnauthorizedAccessException("Only company users can view their feedback.");

            var company = (await _companyRepo.FindAsync(c => c.UserId == currentUser.Id)).FirstOrDefault();
            if (company == null)
                throw new KeyNotFoundException("Company not found.");

            var feedbacks = await _feedbackRepo.FindAsync(f => f.Order.CompanyId == company.CompanyId);
            return _mapper.Map<IEnumerable<FeedbackDto>>(feedbacks);
        }

        // 2. Add feedback (anyone can add)
        public async Task<FeedbackDto> AddAsync(FeedbackDto dto)
        {
            var entity = _mapper.Map<Feedback>(dto);
            entity.SubmittedAt = DateTime.UtcNow;

            await _feedbackRepo.AddAsync(entity);
            return _mapper.Map<FeedbackDto>(entity);
        }

        // 3. Delete feedback (only company who owns it)
        public async Task DeleteAsync(int feedbackId, IdentityUser currentUser)
        {
            if (!await _userManager.IsInRoleAsync(currentUser, "company"))
                throw new UnauthorizedAccessException("Only company users can delete feedback.");

            var company = (await _companyRepo.FindAsync(c => c.UserId == currentUser.Id)).FirstOrDefault();
            if (company == null)
                throw new KeyNotFoundException("Company not found.");

            var feedback = await _feedbackRepo.GetByIdAsync(feedbackId);
            if (feedback == null)
                throw new KeyNotFoundException("Feedback not found.");

            if (feedback.Order.CompanyId != company.CompanyId)
                throw new UnauthorizedAccessException("You can only delete feedback for your own company.");

            await _feedbackRepo.DeleteAsync(feedback);
        }
    }
}
