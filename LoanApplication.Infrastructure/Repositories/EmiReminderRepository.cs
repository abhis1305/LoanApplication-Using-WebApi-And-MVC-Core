using LoanApplication.Application.Interface;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class EmiReminderRepository : IEmiReminderRepository
{
    private readonly AppDBContext db;

    public EmiReminderRepository(AppDBContext db)
    {
        this.db = db;
    }

    public async Task CheckUpcomingEmis()
    {
        var reminderDate = DateTime.Today.AddDays(2);

        var emis = await db.EmiSchedules
            .Where(e => e.DueDate.Date == reminderDate &&
                        e.PaymentStatus == "Pending")
            .ToListAsync();

        foreach (var emi in emis)
        {
            var loan = await db.LoanAccounts
                .FirstOrDefaultAsync(l =>
                    l.LoanAccountId == emi.LoanAccountId);

            if (loan == null)
                continue;

            var notification = new Notification
            {
                CustomerId = loan.CustomerId,
                Message = "Your EMI payment of " +
                          emi.Emi +
                          " is due on " +
                          emi.DueDate.ToString("dd-MM-yyyy"),

                IsRead = false,
                CreatedAt = DateTime.Now
            };

            db.Set<Notification>().Add(notification);
        }

        await db.SaveChangesAsync();
    }
}