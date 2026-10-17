using LoanApplication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Interface
{
    public interface INotificationService
    {
        Task<List<Notification>> GetNotifications(int customerId);
        Task MarkAsRead(int id);
    }
}
