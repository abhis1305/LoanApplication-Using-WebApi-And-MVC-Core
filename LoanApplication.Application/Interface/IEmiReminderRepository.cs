using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Interface
{
    public interface IEmiReminderRepository
    {
        Task CheckUpcomingEmis();
    }
}
