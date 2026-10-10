using AutoMapper;
using LoanApplication.Application.DTO;
using LoanApplication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Mapper
{
    public class SupportTicketProfile : Profile
    {
        public SupportTicketProfile()
        {
            // request -> entity
            CreateMap<CreateTicketDTO, SupportTicket>();

            // entity -> response
            CreateMap<SupportTicket, TicketResponseDTO>()
                .ForMember(d => d.CustomerName,
                    o => o.MapFrom(s => s.Customer != null
                        ? s.Customer.FirstName + " " + s.Customer.LastName : ""))
                .ForMember(d => d.LoanAccountNo,
                    o => o.MapFrom(s => s.LoanAccount != null
                        ? s.LoanAccount.LoanAccountNo : ""));
        }
    }
}
