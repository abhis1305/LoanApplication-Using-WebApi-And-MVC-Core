using AutoMapper;
using LoanApplication.Application.DTO;
using LoanApplication.Application.Interfcae;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Infrastructure.Repositories
{
    public class SupportTicketService : ISupportTicketService
    {
        AppDBContext db;
        IMapper mapper;
        IMemoryCache cache;

        public SupportTicketService(AppDBContext db, IMapper mapper, IMemoryCache cache)
        {
            this.db = db;
            this.mapper = mapper;
            this.cache = cache;
        }
        public async Task AddTicketAsync(CreateTicketDTO dto)
        {
            var ticket = mapper.Map<SupportTicket>(dto);
            ticket.Status = "Pending";
            ticket.CreatedDate = DateTime.Now;
            db.SupportTickets.Add(ticket);
            await db.SaveChangesAsync();
            cache.Remove("ticketlist");
        }

        public async Task<List<TicketResponseDTO>> FetchMyTicketsAsync(int customerId)
        {
            if (cache.TryGetValue("ticketlist", out List<TicketResponseDTO> list))
            {
                return list;                     // from cache
            }
            var data = await db.SupportTickets
                .Include(t => t.Customer)
                .Include(t => t.LoanAccount)
                .Where(t => t.CustomerId == customerId)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();
            var mdata = mapper.Map<List<TicketResponseDTO>>(data);
            cache.Set("ticketlist", mdata, TimeSpan.FromMinutes(5));
            return mdata;
        }

        public async Task<bool> DeleteTicketAsync(int id)
        {
            var ticket = await db.SupportTickets.FirstOrDefaultAsync(t => t.TicketId == id);
            if (ticket == null) return false;

            db.SupportTickets.Remove(ticket);
            await db.SaveChangesAsync();
            cache.Remove("ticketlist");
            return true;
        }

        public async Task<TicketResponseDTO?> FetchTicketByIdAsync(int id)
        {
            var ticket = await db.SupportTickets
                .Include(t => t.Customer)
                .Include(t => t.LoanAccount)
                .FirstOrDefaultAsync(t => t.TicketId == id);
            return ticket == null ? null : mapper.Map<TicketResponseDTO>(ticket);
        }

        public async Task<List<TicketResponseDTO>> FetchTicketsAsync()
        {
            var data = await db.SupportTickets
                .Include(t => t.Customer)
                .Include(t => t.LoanAccount)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();
            return mapper.Map<List<TicketResponseDTO>>(data);
        }

        public async Task<bool> RespondTicketAsync(int id, OfficerResponseDTO dto)
        {
            var ticket = await db.SupportTickets.FirstOrDefaultAsync(t => t.TicketId == id);
            if (ticket == null) return false;

            ticket.OfficerResponse = dto.OfficerResponse;
            ticket.Status = dto.Status;
            await db.SaveChangesAsync();
            cache.Remove("ticketlist");
            return true;
        }
    }
}
