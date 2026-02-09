using AIResumeAnalyzer.Data;
using AIResumeAnalyzer.Models;
using AIResumeAnalyzer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIResumeAnalyzer.Controllers
{
    public class ResumeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ResumeAnalyzer _analyzer;
        private readonly ILogger<ResumeController> _logger;

        public ResumeController(ApplicationDbContext context, ResumeAnalyzer analyzer, ILogger<ResumeController> logger)
        {
            _context = context;
            _analyzer = analyzer;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile resumeFile, string jobDescription)
        {
            if (resumeFile == null || resumeFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a valid PDF file.");
                return View();
            }

            if (string.IsNullOrWhiteSpace(jobDescription))
            {
                ModelState.AddModelError("", "Please enter a job description.");
                return View();
            }

            if (!resumeFile.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Only PDF files are allowed.");
                return View();
            }

            try
            {
                using var stream = resumeFile.OpenReadStream();
                // We must copy the stream because PdfPig might need to seek, or if we want to save file content later. 
                // However, ResumeAnalyzer reads it.
                // Let's just pass the stream.
                var (resumeText, aiResult, score) = await _analyzer.AnalyzeAsync(stream, jobDescription);

                var resume = new Resume
                {
                    FileName = resumeFile.FileName,
                    ResumeText = resumeText,
                    JobDescription = jobDescription,
                    AIResult = aiResult,
                    Score = score,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Resumes.Add(resume);
                await _context.SaveChangesAsync();

                return RedirectToAction("Result", new { id = resume.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing resume.");
                ModelState.AddModelError("", "An error occurred while analyzing the resume. Please try again.");
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Result(int id)
        {
            var resume = await _context.Resumes.FindAsync(id);
            if (resume == null)
            {
                return NotFound();
            }
            return View(resume);
        }
    }
}
