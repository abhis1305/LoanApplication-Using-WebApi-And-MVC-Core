using LoanApplication.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Interfcae
{
    public interface ISupportTicketService
    {
        Task AddTicketAsync(CreateTicketDTO dto);
        Task<List<TicketResponseDTO>> FetchTicketsAsync();                  // officer: all tickets
        Task<List<TicketResponseDTO>> FetchMyTicketsAsync(int customerId);  // customer: own tickets
        Task<TicketResponseDTO?> FetchTicketByIdAsync(int id);
        Task<bool> RespondTicketAsync(int id, OfficerResponseDTO dto);
        Task<bool> DeleteTicketAsync(int id);
    }
}
