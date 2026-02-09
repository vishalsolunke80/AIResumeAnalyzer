using System.Text;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace AIResumeAnalyzer.Services
{
    public class ResumeAnalyzer
    {
        private readonly OpenAIService _openAIService;

        public ResumeAnalyzer(OpenAIService openAIService)
        {
            _openAIService = openAIService;
        }

        public async Task<(string Text, string AIResult, int Score)> AnalyzeAsync(Stream fileStream, string jobDescription)
        {
            string resumeText = ExtractText(fileStream);
            if (string.IsNullOrWhiteSpace(resumeText))
            {
                return (string.Empty, "Could not extract text from PDF. Please ensure the PDF contains selectable text (not a scanned image).", 0);
            }

            string aiResult = await _openAIService.AnalyzeResumeAsync(resumeText, jobDescription);
            
            int score = ExtractScore(aiResult);

            return (resumeText, aiResult, score);
        }

        private string ExtractText(Stream fileStream)
        {
            try
            {
                // Reset stream position if needed
                if (fileStream.CanSeek) fileStream.Position = 0;
                
                var text = new StringBuilder();
                
                using (var pdfReader = new PdfReader(fileStream))
                using (var pdfDocument = new PdfDocument(pdfReader))
                {
                    for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                    {
                        var page = pdfDocument.GetPage(i);
                        var strategy = new LocationTextExtractionStrategy();
                        var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                        text.AppendLine(pageText);
                    }
                }
                
                return text.ToString().Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading PDF: {ex.Message}");
                return string.Empty;
            }
        }

        private int ExtractScore(string aiOutput)
        {
            // Try to find "Match score: X" or "1. Match score X" or just a percentage
            var match = Regex.Match(aiOutput, @"(?:Match\s*score|Score)[:\s]*(\d{1,3})", RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int s))
            {
                return Math.Clamp(s, 0, 100);
            }
            
            // Try to find any percentage pattern like "85%" or "85 percent"
            match = Regex.Match(aiOutput, @"(\d{1,3})\s*(?:%|percent)", RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int p))
            {
                return Math.Clamp(p, 0, 100);
            }
            
            return 0;
        }
    }
}
