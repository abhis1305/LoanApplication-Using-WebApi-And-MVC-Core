using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;


    namespace LoanApplication.Infrastructure.Services
    {
        public class CustomerProfileService : ICustomerProfileService
        {
            private readonly AppDBContext db;

            public CustomerProfileService(AppDBContext context)
            {
                db = context;
            }

            // GET CUSTOMER PROFILE
            public async Task<CustomerProfileDto?> GetMyProfileAsync(
                string identity,
                CancellationToken cancellationToken = default)
            {
                var user = await FindUserAsync(identity, cancellationToken);

                if (user?.Customer == null)
                {
                    return null;
                }

                return MapToDto(user);
            }

            // UPDATE CUSTOMER PROFILE
            public async Task<CustomerProfileDto?> UpdateMyProfileAsync(
                string identity,
                UpdateCustomerProfileDto dto,
                CancellationToken cancellationToken = default)
            {
                var user = await FindUserAsync(identity, cancellationToken);

                if (user?.Customer == null)
                {
                    return null;
                }

                var customer = user.Customer;
                var email = dto.Email.Trim();

                // Check duplicate email in User table
                bool userEmailExists = await db.Users.AnyAsync(
                    u => u.Email == email && u.UserId != user.UserId,
                    cancellationToken);

                if (userEmailExists)
                {
                    throw new InvalidOperationException(
                        "This email is already registered to another user.");
                }

                // Check duplicate email in Customer table
                bool customerEmailExists = await db.Customers.AnyAsync(
                    c => c.Email == email &&
                         c.CustomerId != customer.CustomerId,
                    cancellationToken);

                if (customerEmailExists)
                {
                    throw new InvalidOperationException(
                        "This email is already registered to another customer.");
                }

                // Update Customer table
                customer.FirstName = dto.FirstName.Trim();
                customer.LastName = dto.LastName.Trim();
                customer.Email = email;
                customer.MobileNo = dto.MobileNo.Trim();
                customer.Age = dto.Age;
                customer.EmploymentType = dto.EmploymentType.Trim();
                customer.MonthlyIncome = dto.MonthlyIncome;
                customer.MonthlyInvestment = dto.MonthlyInvestment;

                // Keep common account details in User table synchronized
                user.FirstName = customer.FirstName;
                user.LastName = customer.LastName;
                user.Email = customer.Email;
                user.Mobile = customer.MobileNo;

                await db.SaveChangesAsync(cancellationToken);

                return MapToDto(user);
            }

            // FIND THE LOGGED-IN USER AND LOAD LINKED CUSTOMER
            private async Task<User?> FindUserAsync(
                string identity,
                CancellationToken cancellationToken)
            {
                if (string.IsNullOrWhiteSpace(identity))
                {
                    return null;
                }

                if (int.TryParse(identity, out int userId))
                {
                    return await db.Users
                        .Include(u => u.Customer)
                        .FirstOrDefaultAsync(
                            u => u.UserId == userId,
                            cancellationToken);
                }

                return await db.Users
                    .Include(u => u.Customer)
                    .FirstOrDefaultAsync(
                        u => u.Email == identity,
                        cancellationToken);
            }

            // MAP ENTITY TO DTO
            private static CustomerProfileDto MapToDto(User user)
            {
                var customer = user.Customer!;

                return new CustomerProfileDto
                {
                    UserId = user.UserId,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    MobileNo = customer.MobileNo ?? user.Mobile ?? string.Empty,
                    Pan = customer.Pan ?? string.Empty,
                    AadhaarNo = MaskAadhaar(customer.AadhaarNo),
                    EmploymentType = customer.EmploymentType ?? string.Empty,
                    MonthlyIncome = customer.MonthlyIncome ?? 0,
                    MonthlyInvestment = customer.MonthlyInvestment ?? 0,
                    Age = customer.Age ?? 0
                };
            }

            // MASK AADHAAR FOR PRIVACY
            private static string MaskAadhaar(string? aadhaar)
            {
                if (string.IsNullOrWhiteSpace(aadhaar))
                {
                    return string.Empty;
                }

                string digits = new string(
                    aadhaar.Where(char.IsDigit).ToArray());

                if (digits.Length < 4)
                {
                    return "XXXX";
                }

                return $"XXXX XXXX {digits[^4..]}";
            }
        }
    }


