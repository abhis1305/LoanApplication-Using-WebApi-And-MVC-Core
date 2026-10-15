
using LoanApplication.Application.DTO;
using LoanApplication.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoanApplication.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KycController : ControllerBase
    {
        private readonly IKycService service;

        public KycController(IKycService service)
        {
            this.service = service;
        }


        [Authorize(Roles = "User")]
        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadKycDocumentDto dto)
        {
            // Get the logged-in user's ID from the JWT token
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }
            var result = await service.UploadDocumentAsync(userId, dto);
            if (!result)
            {
                return BadRequest(new
                {
                    message = "Document upload failed."
                });
            }
            return Ok(new
            {
                message = "Document uploaded successfully. Status: Pending."
            });
        }

        // CUSTOMER: Get My Documents

        [Authorize(Roles = "User")]
        [HttpGet]
        [Route("my-documents")]
        public async Task<IActionResult> MyDocuments()
        {
            // Get the logged-in user's ID from the JWT token
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }
            var result = await service.GetMyDocumentsAsync(userId);
            return Ok(result);
        }


        // CUSTOMER: View Own Document

        [Authorize(Roles = "User")]
        [HttpGet]
        [Route("my-documents/{documentId}/view")]
        public async Task<IActionResult> ViewMyDocument(int documentId)
        {
            // Get the logged-in user's ID from the JWT token
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }
            // Fetch only this user's document
            var filePath = await service.GetMyDocumentFilePathAsync(userId,documentId);
            if (filePath == null || !System.IO.File.Exists(filePath))
            {
                return NotFound(new
                {
                    message = "Document not found."
                });
            }
            return PhysicalFile(filePath,GetContentType(filePath),enableRangeProcessing: true            );
        }


        // OFFICER: Get Pending Customers
        [Authorize(Roles = "Loan Officer")]
        [HttpGet]
        [Route("pending-customers")]
        public async Task<IActionResult> PendingCustomers()
        {
            var result = await service.GetPendingCustomersAsync();
            return Ok(result);
        }

        // OFFICER: View Customer KYC Details
        [Authorize(Roles = "Loan Officer")]
        [HttpGet]
        [Route("customer/{customerId}")]
        public async Task<IActionResult> CustomerKyc(int customerId)
        {
            var result = await service.GetCustomerKycAsync(customerId);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Customer not found."
                });
            }

            return Ok(result);
        }

        // OFFICER: View Document
        [Authorize(Roles = "Loan Officer")]
        [HttpGet]
        [Route("documents/{documentId}/view")]
        public async Task<IActionResult> ViewDocument(int documentId)
        {
            var filePath =
                await service.GetOfficerDocumentFilePathAsync(documentId);

            if (filePath == null)
            {
                return NotFound(new
                {
                    message = "Document not found."
                });
            }

            return PhysicalFile(
                filePath,
                GetContentType(filePath),
                enableRangeProcessing: true);
        }

        // OFFICER: Approve Selected Documents
        [Authorize(Roles = "Loan Officer")]
        [HttpPut]
        [Route("customer/{customerId}/approve")]
        public async Task<IActionResult> Approve(
            int customerId,
            [FromBody] KycDocumentDecisionDto dto)
        {
            var result = await service.ApproveDocumentsAsync(
                customerId, dto.DocumentIds);

            if (!result)
            {
                return BadRequest(new
                {
                    message = "Documents could not be approved."
                });
            }

            return Ok(new
            {
                message = "Documents approved successfully."
            });
        }

        // OFFICER: Reject Selected Documents
        [Authorize(Roles = "Loan Officer")]
        [HttpPut]
        [Route("customer/{customerId}/reject")]
        public async Task<IActionResult> Reject(
            int customerId,
            [FromBody] RejectKycDocumentsDto dto)
        {
            var result = await service.RejectDocumentsAsync(
                customerId,
                dto.DocumentIds,
                dto.RejectionReason);

            if (!result)
            {
                return BadRequest(new
                {
                    message = "Documents could not be rejected."
                });
            }

            return Ok(new
            {
                message = "Documents rejected successfully."
            });
        }

        // Get file content type
        private static string GetContentType(string filePath)
        {
            string extension =
                Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }
    }
}
