using App.Domain.Interfaces.Base;
using MyApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Domain.Interfaces
{
    public interface ICompanyRepository : IBaseRepository<Company>
    {
    }
}
