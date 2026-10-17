using LoanApplication.Application.Interface;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Infrastructure.Repositories
{
    public class NotificationService : INotificationService
    {
        AppDBContext db;

        public NotificationService(AppDBContext db)
        {
            this.db = db;
        }

        public async Task<List<Notification>> GetNotifications(int customerId)
        {
            return await db.Set<Notification>()
                .Where(n => n.CustomerId == customerId)
                .OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task MarkAsRead(int id)
        {
            var notification = await db.Set<Notification>()
                .FirstOrDefaultAsync(n => n.NotificationId == id);

            if (notification != null)
            {
                notification.IsRead = true;
                await db.SaveChangesAsync();
            }
        }
    }
}
