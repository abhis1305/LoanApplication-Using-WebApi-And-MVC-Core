using AutoMapper;
using LoanApplication.Application.DTO;
using LoanApplication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Mapper
{
    public class LoanDealMappingData : Profile
    {
        public LoanDealMappingData()
        {
            CreateMap<CreateLoanDealDto, LoanDeal>();

            CreateMap<LoanDeal, LoanDealDto>()
                  .ForMember(
                      dest => dest.CustomerName,
                      opt => opt.MapFrom(src =>
                          src.Customer == null
                              ? ""
                              : src.Customer.FirstName + " " + src.Customer.LastName));


            CreateMap<LoanDeal, ApprovedLoanDto>()
           .ForMember(
               dest => dest.CustomerName,
               opt => opt.MapFrom(src =>
                   src.Customer == null
                       ? ""
                       : (src.Customer.FirstName + " " + src.Customer.LastName).Trim()
               )
           )
           .ForMember(
               dest => dest.RequestedAmount,
               opt => opt.MapFrom(src => src.LoanAmount)
           );


            CreateMap<SanctionLetter, SanctionLetterDocumentDto>().ForMember(dest => dest.FileName,
                       opt => opt.MapFrom(src => $"SanctionLetter_{src.DealId}.pdf"));


            CreateMap<LoanDeal, SanctionLetterDataDto>()
                .ForMember(d => d.ApplicationNo,
                    o => o.MapFrom(s => $"APP{s.DealId:D3}"))
                .ForMember(d => d.ApplicantName,
                    o => o.MapFrom(s => s.Customer == null
                        ? ""
                        : $"{s.Customer.FirstName} {s.Customer.LastName}".Trim()))
                .ForMember(d => d.MobileNo,
                    o => o.MapFrom(s => s.Customer == null
                        ? ""
                        : s.Customer.MobileNo))
                .ForMember(d => d.LoanAmountSanctioned,
                    o => o.MapFrom(s => s.ApprovedAmount))
                .ForMember(d => d.LoanTenure,
                    o => o.MapFrom(s => s.TenureMonths))
                .ForMember(d => d.AmountOfEmi,
                    o => o.MapFrom(s => s.EmiAmount));
        }
    

    }
}
