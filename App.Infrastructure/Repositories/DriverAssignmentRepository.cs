using App.Domain.Interfaces;
using App.Infrastructure.Repositories.Base;
using App.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Infrastructure.Repositories
{
    public class DriverAssignmentRepository : BaseRepository<DriverAssignment>, IDriverAssignmentRepository
    {
        public DriverAssignmentRepository(AppDbContext context) : base(context)
        {
        }

    }
}
