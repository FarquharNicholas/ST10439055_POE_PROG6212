using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ST10439055_POE_PROG6212.Services
{
    public interface IFileUploadService
    {
        Task<(bool Success, string FileName, string FilePath, string ErrorMessage)> UploadFileAsync(IFormFile file, int claimId);
        bool IsValidFileType(string fileName);
        bool IsValidFileSize(long fileSize);
        string GetFileExtension(string fileName);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;
        private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".xlsx", ".doc", ".xls" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<(bool Success, string FileName, string FilePath, string ErrorMessage)> UploadFileAsync(IFormFile file, int claimId)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return (false, string.Empty, string.Empty, "No file selected.");
                }

                if (!IsValidFileType(file.FileName))
                {
                    return (false, string.Empty, string.Empty, "Invalid file type. Only PDF, DOCX, and XLSX files are allowed.");
                }

                if (!IsValidFileSize(file.Length))
                {
                    return (false, string.Empty, string.Empty, "File size exceeds the 10MB limit.");
                }

                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "claims", claimId.ToString());
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var fileExtension = GetFileExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("File uploaded successfully: {FileName} for claim {ClaimId}", file.FileName, claimId);

                return (true, file.FileName, filePath, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file for claim {ClaimId}", claimId);
                return (false, string.Empty, string.Empty, "An error occurred while uploading the file.");
            }
        }

        public bool IsValidFileType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            var extension = GetFileExtension(fileName).ToLowerInvariant();
            return _allowedExtensions.Contains(extension);
        }

        public bool IsValidFileSize(long fileSize)
        {
            return fileSize <= MaxFileSize;
        }

        public string GetFileExtension(string fileName)
        {
            return Path.GetExtension(fileName);
        }
    }
}
