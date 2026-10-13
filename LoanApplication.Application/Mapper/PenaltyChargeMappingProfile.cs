using AutoMapper;
using LoanApplication.Application.DTO;
using LoanApplication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Mapper
{
    public class PenaltyChargeMappingProfile : Profile
    {
        public PenaltyChargeMappingProfile()
        {
            CreateMap<PenaltyCharge, PenaltyChargeDto>();

            CreateMap<PenaltyCharge, CreatePenaltyChargeDto>().ReverseMap();
        }
    }
}
