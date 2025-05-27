using App.Domain.Interfaces;
using App.Domain.Interfaces.Base;
using App.Infrastructure.Repositories.Base;
using App.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace App.Infrastructure.Repositories
{
    public class PickUpRequestRepository : BaseRepository<PickUpRequest>, IPickUpRequestRepository
    {
        public PickUpRequestRepository(AppDbContext context) : base(context)
        {
        }
    }
}
