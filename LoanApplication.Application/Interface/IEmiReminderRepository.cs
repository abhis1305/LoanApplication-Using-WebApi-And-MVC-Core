using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Interfcae
{
    public interface IEmiReminderRepository
    {
        Task CheckUpcomingEmis();
    }
}
