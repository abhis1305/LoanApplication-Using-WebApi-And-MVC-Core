
using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace LoanApplication.Infrastructure.Repositories
{
    public class KycService : IKycService
    {
        private readonly AppDBContext db;
        private readonly IWebHostEnvironment environment;

        public KycService(AppDBContext db,IWebHostEnvironment environment)
        {
            this.db = db;
            this.environment = environment;
        }

        // Get CustomerId using logged-in UserId
        private async Task<int?> GetCustomerIdAsync(int userId)
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.UserId == userId);
            return user?.CustomerId;
        }

        // Customer uploads document
        public async Task<bool> UploadDocumentAsync(int userId,UploadKycDocumentDto dto)
        {
            var customerId = await GetCustomerIdAsync(userId);
            if (customerId == null || dto.File == null ||dto.File.Length == 0)
                return false;

            if (dto.File.Length > 5 * 1024 * 1024)
                return false;

            string extension =Path.GetExtension(dto.File.FileName).ToLowerInvariant();

            if (extension != ".pdf" &&
                extension != ".jpg" &&
                extension != ".jpeg" &&
                extension != ".png")
                return false;
            string folder = Path.Combine(environment.ContentRootPath, "KycUploads");

            Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid().ToString() + extension;
            string filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var document = new KycDocument
            {
                CustomerId = customerId.Value,
                DocumentType = dto.DocumentType,
                FilePath = filePath,
                VerificationStatus = "Pending"
            };

            db.KycDocuments.Add(document);
            await db.SaveChangesAsync();

            return true;
        }

        // Customer sees their documents
        public async Task<List<KycDocumentResponseDto>>GetMyDocumentsAsync(int userId)
        {
            var customerId = await GetCustomerIdAsync(userId);
            if (customerId == null)
                return new List<KycDocumentResponseDto>();

            return await db.KycDocuments.Where(x => x.CustomerId == customerId.Value)
                .Select(x => new KycDocumentResponseDto
                {
                    DocumentId = x.DocumentId,
                    DocumentType = x.DocumentType,
                    VerificationStatus = x.VerificationStatus,
                    RejectionReason = x.RejectionReason
                })
                .ToListAsync();
        }

        // Customer can access only their own document
        public async Task<string?> GetMyDocumentFilePathAsync(int userId, int documentId)
        {
            var customerId = await GetCustomerIdAsync(userId);
            if (customerId == null)
                return null;

            var document = await db.KycDocuments.FirstOrDefaultAsync(x =>
                    x.DocumentId == documentId &&
                    x.CustomerId == customerId.Value);

            if (document == null ||!System.IO.File.Exists(document.FilePath))
                return null;

            return document.FilePath;
        }


        // Officer sees customers having pending documents
        public async Task<List<KycCustomerListDto>>GetPendingCustomersAsync()
        {
            return await db.Customers.Where(c => c.KycDocuments!.Any(d => d.VerificationStatus == "Pending"))
                .Select(c => new KycCustomerListDto
                {
                    CustomerId = c.CustomerId,
                    CustomerName = c.FirstName + " " + c.LastName,
                    Email = c.Email,
                    MobileNo = c.MobileNo ?? ""
                })
                .ToListAsync();
        }

        // Officer opens customer KYC details
        public async Task<KycCustomerDetailsDto?>GetCustomerKycAsync(int customerId)
        {
            var customer = await db.Customers.Include(c => c.KycDocuments)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customer == null)
                return null;
            return new KycCustomerDetailsDto
            {
                CustomerId = customer.CustomerId,
                CustomerName = customer.FirstName + " " + customer.LastName,
                Email = customer.Email,

                Documents = customer.KycDocuments!
                    .Select(d => new KycDocumentResponseDto
                    {
                        DocumentId = d.DocumentId,
                        DocumentType = d.DocumentType,
                        VerificationStatus = d.VerificationStatus,
                        RejectionReason = d.RejectionReason
                    })
                    .ToList()
            };
        }

        // Officer views a document
        public async Task<string?> GetOfficerDocumentFilePathAsync(int documentId)
        {
            var document = await db.KycDocuments
                .FirstOrDefaultAsync(d => d.DocumentId == documentId);
            if (document == null)
                return null;
            if (!System.IO.File.Exists(document.FilePath))
                return null;

            return document.FilePath;
        }

        // Officer approves selected pending documents
        public async Task<bool> ApproveDocumentsAsync(int customerId,List<int> documentIds)
        {
            if (documentIds == null || documentIds.Count == 0)
                return false;

            var documents = await db.KycDocuments
                .Where(d =>
                    d.CustomerId == customerId &&
                    documentIds.Contains(d.DocumentId) &&
                    d.VerificationStatus == "Pending")
                .ToListAsync();

            if (documents.Count != documentIds.Distinct().Count())
                return false;

            foreach (var document in documents)
            {
                document.VerificationStatus = "Approved";
                document.RejectionReason = null;
            }

            await db.SaveChangesAsync();
            return true;
        }

        // Officer rejects selected pending documents
        public async Task<bool> RejectDocumentsAsync(int customerId,List<int> documentIds,string rejectionReason)
        {
            if (documentIds == null ||
                documentIds.Count == 0 ||
                string.IsNullOrWhiteSpace(rejectionReason))
                return false;

            var documents = await db.KycDocuments.Where(d =>
                    d.CustomerId == customerId &&
                    documentIds.Contains(d.DocumentId) &&
                    d.VerificationStatus == "Pending")
                .ToListAsync();

            if (documents.Count != documentIds.Distinct().Count())
                return false;
            foreach (var document in documents)
            {
                document.VerificationStatus = "Rejected";
                document.RejectionReason = rejectionReason;
            }
            await db.SaveChangesAsync();
            return true;
        }

    }
}
