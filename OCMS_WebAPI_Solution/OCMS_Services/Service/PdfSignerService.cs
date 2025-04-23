﻿using OCMS_BOs.Entities;
using OCMS_Repositories;
using OCMS_Services.IService;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using HtmlAgilityPack;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using QuestPDF.Elements;
using System.Text.RegularExpressions;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using System.IO;
using System.Drawing;
using System;
using Microsoft.Extensions.Configuration;
using AngleSharp;
using AngleSharp.Html.Parser;
using SkiaSharp;
using SkiaSharp.Extended.Svg;
using Microsoft.AspNetCore.Components;
using System.Runtime.InteropServices;
using PuppeteerSharp;

namespace OCMS_Services.Service
{
    public class PdfSignerService : IPdfSignerService
    {
        private readonly HttpClient _httpClient;
        private readonly IHsmAuthService _tokenService;
        private readonly IBlobService _blobService;
        private readonly UnitOfWork _unitOfWork;
        private readonly ICertificateService _certificateService;
        private readonly IEmailService _emailService;
        public PdfSignerService(HttpClient httpClient, IHsmAuthService tokenService, UnitOfWork unitOfWork, IBlobService blobService, ICertificateService certificateService, IEmailService emailService)
        {
            _httpClient = httpClient;
            _certificateService = certificateService ?? throw new ArgumentNullException(nameof(certificateService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        #region Helper Methods
        private async Task<string> SignPdfBase64Async(string fileDataBase64)
        {
            var token = await _tokenService.GetTokenAsync();
            const string url = "https://demohsm.wgroup.vn/hsm/pdf";

            var body = new
            {
                options = new
                {
                    PAGENO = 1,
                    POSITIONIDENTIFIER = "Vùng dành cho chữ ký số",
                    RECTANGLESIZE = "180,50",
                    RECTANGLEOFFSET = "-50,-40",
                    VISIBLESIGNATURE = true,
                    VISUALSTATUS = false,
                    SHOWSIGNERINFO = true,
                    SIGNERINFOPREFIX = "Ký bởi:",
                    SHOWREASON = true,
                    SIGNREASONPREFIX = "Lý do:",
                    SIGNREASON = "Tôi đồng ý",
                    SHOWDATETIME = true,
                    DATETIMEPREFIX = "Ký ngày:",
                    SHOWLOCATION = true,
                    LOCATIONPREFIX = "Nơi ký:",
                    LOCATION = "Hồ Chí Minh",
                    TEXTDIRECTION = "RIGHT",
                    TEXTCOLOR = "red",
                    IMAGEANDTEXT = true,
                    BACKGROUNDIMAGE = "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",
                    SIGNATUREIMAGE = "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"
                },
                file_data = fileDataBase64
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        public byte[] ConvertSvgToPng(string base64Svg, int width = 150, int height = 150)
        {
            try
            {
                // Decode the base64 string to get the raw SVG bytes
                var svgBytes = Convert.FromBase64String(base64Svg);

                var svg = new SkiaSharp.Extended.Svg.SKSvg();
                using var svgStream = new MemoryStream(svgBytes);
                svg.Load(svgStream);

                // Create a new bitmap surface to render the PNG
                using var bitmap = new SKBitmap(width, height);
                using var canvas = new SKCanvas(bitmap);
                canvas.Clear(SKColors.White);

                // Calculate scale factor to fit
                var scaleX = width / svg.CanvasSize.Width;
                var scaleY = height / svg.CanvasSize.Height;
                var scale = Math.Min(scaleX, scaleY);

                // Apply transform and draw
                canvas.Scale(scale);
                canvas.DrawPicture(svg.Picture);
                canvas.Flush();

                // Encode to PNG
                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                return data.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SVG to PNG conversion failed: {ex.Message}");
                return Array.Empty<byte>();
            }
        }
        public async Task<byte[]> ConvertHtmlToPdfPuppet(string htmlContent)
        {
            try
            {
                // Wrap HTML with fixed size and adjustments
                htmlContent = $$"""
<!DOCTYPE html>
<html>
<head>
    <style>
        @page {
            size: A4;
            margin: 0;
        }
        body {
            margin: 0;
            padding: 0;
            font-size: 10pt;
            width: 100%;
            box-sizing: border-box;
        }
        .pdf-container {
            width: 1123px; /* A4 landscape */
            height: 794px;
            padding: 40px;
            box-sizing: border-box;
            position: relative;
        }
        img {
            max-width: 100%;
            height: auto;
        }
    </style>
</head>
<body>
    <div class="pdf-container">
        {{htmlContent}}
    </div>
</body>
</html>
""";

                // Define executable path
                string executablePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe" // Adjust if your Chrome is elsewhere
                    : Environment.GetEnvironmentVariable("PUPPETEER_EXECUTABLE_PATH") ?? "/usr/bin/chromium";

                var launchOptions = new LaunchOptions
                {
                    Headless = true,
                    ExecutablePath = executablePath,
                    Args = new[]
                    {
                "--no-sandbox",
                "--disable-setuid-sandbox",
                "--disable-dev-shm-usage",
                "--disable-gpu",
                "--font-render-hinting=none",
                "--disable-web-security"
            }
                };

                using var browser = await Puppeteer.LaunchAsync(launchOptions);
                using var page = await browser.NewPageAsync();

                await page.SetViewportAsync(new ViewPortOptions
                {
                    Width = 1240,
                    Height = 1754,
                    DeviceScaleFactor = 1.5
                });

                await page.SetContentAsync(htmlContent, new PuppeteerSharp.NavigationOptions
                {
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle0 },
                    Timeout = 60000
                });

                var pdfOptions = new PdfOptions
                {
                    Format = PuppeteerSharp.Media.PaperFormat.A4,
                    PrintBackground = true,
                    MarginOptions = new PuppeteerSharp.Media.MarginOptions
                    {
                        Top = "5mm",
                        Bottom = "5mm",
                        Left = "5mm",
                        Right = "5mm"
                    },
                    Scale = 0.9m
                };

                byte[] pdfBytes = await page.PdfDataAsync(pdfOptions);
                return pdfBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generating PDF: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner: {ex.InnerException.Message}");
                throw;
            }
        }
        private async Task<byte[]> ConvertHtmlToPdf(string htmlContent)
        {
            try
            {
                // Load HTML with AngleSharp
                var config = Configuration.Default;
                var context = BrowsingContext.New(config);
                var parser = context.GetService<IHtmlParser>();
                var document = await parser.ParseDocumentAsync(htmlContent);

                using var stream = new MemoryStream();
                QuestPDF.Settings.License = LicenseType.Community;

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(20);
                        page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                        page.Content().Element(body =>
                        {
                            body.Column(column =>
                            {
                                // Header
                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(document.QuerySelector(".header-left")?.TextContent?.Trim() ?? "").LineHeight(1.5f);
                                    row.RelativeItem().AlignRight().Text(document.QuerySelector(".header-right")?.TextContent?.Trim() ?? "").LineHeight(1.5f);
                                });

                                column.Item().Height(20);

                                // Title
                                column.Item().AlignCenter().Text(document.QuerySelector(".title")?.TextContent?.Trim() ?? "")
                                    .FontSize(24).Bold().FontColor(Colors.Red.Darken2);

                                column.Item().Height(20);

                                // Content
                                column.Item().Row(row =>
                                {
                                    var imgElement = document.QuerySelector(".photo img");
                                    if (imgElement != null)
                                    {
                                        var src = imgElement.GetAttribute("src");
                                        if (!string.IsNullOrEmpty(src) && src.StartsWith("data:image"))
                                        {
                                            var base64 = src.Split(',')[1];
                                            byte[] imageBytes = null;

                                            if (src.Contains("svg+xml"))
                                            {
                                                // SVG: Convert to PNG
                                                imageBytes = ConvertSvgToPng(base64); // Make sure this method is defined
                                            }
                                            else
                                            {
                                                // PNG/JPEG etc.
                                                imageBytes = Convert.FromBase64String(base64);
                                            }

                                            if (imageBytes != null && imageBytes.Length > 0)
                                            {
                                                row.ConstantItem(150).Height(205).Element(imageContainer =>
                                                {
                                                    imageContainer.Border(1)
                                                                  .Padding(2)
                                                                  .Image(imageBytes)
                                                                  .FitArea();
                                                });
                                            }
                                        }
                                    }
                                ;

                                row.ConstantItem(20);

                                    row.RelativeItem().Column(details =>
                                    {
                                        var detailsElement = document.QuerySelector(".details");
                                        foreach (var line in detailsElement?.ChildNodes ?? Enumerable.Empty<AngleSharp.Dom.INode>())
                                        {
                                            if (!string.IsNullOrWhiteSpace(line.TextContent))
                                                details.Item().Text(line.TextContent.Trim()).LineHeight(1.5f);
                                        }
                                    });
                                });

                                column.Item().Height(20);

                                // Signature Section
                                column.Item().AlignRight().Width(300).Column(signature =>
                                {
                                    var signatureHtml = document.QuerySelector(".signature-area")?.InnerHtml;
                                    var lines = signatureHtml?.Split("<br>") ?? Array.Empty<string>();
                                    foreach (var line in lines)
                                    {
                                        signature.Item().AlignCenter().Text(line.Trim()).FontSize(12).LineHeight(1.5f);
                                    }

                                    signature.Item().Height(10);

                                    signature.Item().AlignCenter().Width(250).Height(60)
                                        .Border(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .AlignMiddle().AlignCenter()
                                        .Text("Vùng dành cho chữ ký số")
                                        .FontSize(10)
                                        .FontColor(Colors.Grey.Medium);
                                });

                                // Footer
                                column.Item().Height(20);
                                column.Item().Row(footer =>
                                {
                                    footer.RelativeItem().Text(document.QuerySelector(".footer div:first-child")?.TextContent?.Trim() ?? "");
                                    footer.RelativeItem().AlignRight().Text(document.QuerySelector(".footer div:last-child")?.TextContent?.Trim() ?? "");
                                });
                            });
                        });
                    });
                }).GeneratePdf(stream);

                return stream.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generating PDF: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner: {ex.InnerException.Message}");
                throw;
            }
        }
        #endregion

        #region Sign Pdf
        public async Task<byte[]> SignPdfAsync(string certificateId, string approvedByUserId)
        {
            // Step 1: Validate input and dependencies
            if (string.IsNullOrWhiteSpace(certificateId))
                throw new ArgumentException("Certificate ID cannot be null or empty.");
            if (_unitOfWork?.CertificateRepository == null)
                throw new InvalidOperationException("Certificate repository is not initialized.");
            if (_unitOfWork?.UserRepository == null)
                throw new InvalidOperationException("User repository is not initialized.");
            if (_blobService == null)
                throw new InvalidOperationException("Blob service is not initialized.");
            if (_emailService == null)
                throw new InvalidOperationException("Email service is not initialized.");

            // Step 2: Get the certificate info
            var certificate = await _unitOfWork.CertificateRepository.GetByIdAsync(certificateId);
            if (certificate == null || string.IsNullOrWhiteSpace(certificate.CertificateURL))
                throw new InvalidOperationException("Certificate not found or missing URL.");
            if (certificate.Status != OCMS_BOs.Entities.CertificateStatus.Pending)
                throw new InvalidOperationException($"Certificate is not in Pending status. Current status: {certificate.Status}");

            // Step 3: Get user info for email
            var user = await _unitOfWork.UserRepository.GetByIdAsync(certificate.UserId);
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                throw new InvalidOperationException("User not found or missing email.");

            // Step 4: Generate SAS URL for HTML file
            string sasUrl = await _blobService.GetBlobUrlWithSasTokenAsync(certificate.CertificateURL, TimeSpan.FromHours(1));
            if (string.IsNullOrEmpty(sasUrl))
                throw new InvalidOperationException("Failed to generate SAS URL for the HTML file.");

            // Step 5: Download the HTML content from the SAS URL
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(sasUrl);
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Failed to retrieve HTML content from URL. Status: {response.StatusCode}");

            var htmlContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(htmlContent))
                throw new InvalidOperationException("Downloaded HTML content is empty.");

            // Step 6: Convert HTML to PDF
            byte[] pdfBytes;
            try
            {
                pdfBytes = await ConvertHtmlToPdf(htmlContent);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to convert HTML to PDF.", ex);
            }
            if (pdfBytes == null || pdfBytes.Length == 0)
                throw new InvalidOperationException("Converted PDF content is empty.");

            // Step 7: Convert PDF to Base64
            string pdfBase64 = Convert.ToBase64String(pdfBytes);

            // Step 8: Sign the PDF
            var result = await SignPdfBase64Async(pdfBase64);
            if (string.IsNullOrEmpty(result))
                throw new InvalidOperationException("PDF signing service returned an empty result.");

            // Step 9: Deserialize and extract file_data with error handling
            byte[] signedPdfBytes;
            try
            {
                var jsonDoc = JsonDocument.Parse(result);

                if (!jsonDoc.RootElement.TryGetProperty("result", out var resultElement))
                    throw new InvalidOperationException($"The 'result' property is missing. Raw response: {result}");

                if (!resultElement.TryGetProperty("file_data", out var fileDataElement))
                    throw new InvalidOperationException($"The 'file_data' property is missing in 'result'. Raw result: {resultElement}");

                string fileData = fileDataElement.GetString();
                if (string.IsNullOrEmpty(fileData))
                    throw new InvalidOperationException("The 'file_data' value is empty or null.");

                signedPdfBytes = Convert.FromBase64String(fileData);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to parse the signing service response. Raw: {result}", ex);
            }

            // Step 10: Upload signed PDF to blob storage
            string containerName = "certificates";
            string blobName = $"signed/{certificateId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string newCertificateUrl;
            using (var stream = new MemoryStream(signedPdfBytes))
            {
                try
                {
                    newCertificateUrl = await _blobService.UploadFileAsync(containerName, blobName, stream, "application/pdf");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to upload signed PDF to blob storage.", ex);
                }
                if (string.IsNullOrEmpty(newCertificateUrl))
                    throw new InvalidOperationException("Blob storage returned an empty URL for the signed PDF.");
            }

            // Step 11: Update certificate with new URL and status
            await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    certificate.CertificateURL = newCertificateUrl;
                    certificate.Status = CertificateStatus.Active;
                    certificate.SignDate = DateTime.Now;
                    certificate.ApprovebyUserId = approvedByUserId;
                    await _unitOfWork.CertificateRepository.UpdateAsync(certificate);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new InvalidOperationException("Failed to update certificate status and URL.", ex);
                }
            });

            // Step 12: Send user with new certificate URL
            await SendCertificateByEmailAsync(certificateId);

            //// Step 13: Return signed PDF bytes
            return signedPdfBytes;
        }
        #endregion

        #region Send Email with Certificate
        public async Task SendCertificateByEmailAsync(string certificateId)
        {
            if (_emailService == null)
                throw new InvalidOperationException("Email service is not initialized.");
            if (_blobService == null)
                throw new InvalidOperationException("Blob service is not initialized.");
            var certificate = await _unitOfWork.CertificateRepository.GetByIdAsync(certificateId);
            var user = await _unitOfWork.UserRepository.GetByIdAsync(certificate.UserId);
            try
            {

                // Generate SAS URL for the signed PDF
                string sasUrl = await _blobService.GetBlobUrlWithSasTokenAsync(certificate.CertificateURL, TimeSpan.FromDays(7));
                if (string.IsNullOrEmpty(sasUrl))
                    throw new InvalidOperationException("Failed to generate SAS URL for the signed PDF.");

                // Prepare email
                string subject = "Your Signed Certificate";
                string body = $"Dear User,\n\nYour signed certificate is available for download at the following link:\n{sasUrl}\n\nThis link will expire in 7 days.\n\nBest regards,\nCertificate Team";

                await _emailService.SendEmailAsync(user.Email, subject, body);
                Console.WriteLine($"Email with SAS URL sent successfully to {user.Email} for certificate {certificateId}");
            }
            catch (SmtpException ex)
            {
                throw new InvalidOperationException(
                    $"SMTP error sending certificate {certificateId} to {user.Email}: StatusCode={ex.StatusCode}, Message={ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to send certificate {certificateId} to {user.Email}: {ex.Message}", ex);
            }
        }
        #endregion

        #region Sign Decision PDF
        public async Task<byte[]> SignDecisionAsync(string decisionId)
        {
            // Step 1: Validate input and dependencies
            if (string.IsNullOrWhiteSpace(decisionId))
                throw new ArgumentException("Decision ID cannot be null or empty.");
            if (_unitOfWork?.DecisionRepository == null)
                throw new InvalidOperationException("Decision repository is not initialized.");
            if (_blobService == null)
                throw new InvalidOperationException("Blob service is not initialized.");

            // Step 2: Get the decision info
            var decision = await _unitOfWork.DecisionRepository.GetByIdAsync(decisionId);
            if (decision == null || string.IsNullOrWhiteSpace(decision.Content))
                throw new InvalidOperationException("Decision not found or missing content.");
            if (decision.DecisionStatus != DecisionStatus.Draft)
                throw new InvalidOperationException($"Decision is not in Draft status. Current status: {decision.DecisionStatus}");

            // Step 3: Generate SAS URL for HTML file
            string sasUrl = await _blobService.GetBlobUrlWithSasTokenAsync(decision.Content, TimeSpan.FromHours(1));
            if (string.IsNullOrEmpty(sasUrl))
                throw new InvalidOperationException("Failed to generate SAS URL for the decision content.");

            // Step 4: Download the HTML content from the SAS URL
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(sasUrl);
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Failed to retrieve HTML content. Status: {response.StatusCode}");

            var htmlContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(htmlContent))
                throw new InvalidOperationException("Decision HTML content is empty.");

            // Step 5: Convert HTML to PDF
            byte[] pdfBytes;
            try
            {
                pdfBytes = await ConvertHtmlToPdf(htmlContent);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to convert HTML to PDF.", ex);
            }
            if (pdfBytes == null || pdfBytes.Length == 0)
                throw new InvalidOperationException("Converted PDF content is empty.");

            // Step 6: Convert PDF to Base64
            string pdfBase64 = Convert.ToBase64String(pdfBytes);

            // Step 7: Sign the PDF
            var result = await SignPdfBase64Async(pdfBase64);
            if (string.IsNullOrEmpty(result))
                throw new InvalidOperationException("PDF signing service returned an empty result.");

            // Step 8: Deserialize and extract file_data
            byte[] signedPdfBytes;
            try
            {
                var jsonDoc = JsonDocument.Parse(result);
                var resultElement = jsonDoc.RootElement.GetProperty("result");

                if (!resultElement.TryGetProperty("file_data", out var fileDataElement))
                    throw new KeyNotFoundException("The 'file_data' key was not found in the response.");

                string fileData = fileDataElement.GetString();
                if (string.IsNullOrEmpty(fileData))
                    throw new InvalidOperationException("The 'file_data' value is empty or null.");

                signedPdfBytes = Convert.FromBase64String(fileData);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to parse the signing service response.", ex);
            }
            catch (KeyNotFoundException ex)
            {
                throw new InvalidOperationException("Required data missing in the signing service response.", ex);
            }

            // Step 9: Upload signed PDF to blob storage
            string containerName = "decisions";
            string blobName = $"signed/{decisionId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string newDecisionUrl;
            using (var stream = new MemoryStream(signedPdfBytes))
            {
                try
                {
                    newDecisionUrl = await _blobService.UploadFileAsync(containerName, blobName, stream, "application/pdf");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to upload signed PDF to blob storage.", ex);
                }
                if (string.IsNullOrEmpty(newDecisionUrl))
                    throw new InvalidOperationException("Blob storage returned an empty URL for the signed PDF.");
            }

            // Step 10: Update decision with new URL and status
            await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    decision.Content = newDecisionUrl;
                    decision.DecisionStatus = DecisionStatus.Signed;
                    decision.SignDate = DateTime.Now;
                    await _unitOfWork.DecisionRepository.UpdateAsync(decision);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new InvalidOperationException("Failed to update decision status and URL.", ex);
                }
            });

            // Step 11: Return signed PDF bytes
            return signedPdfBytes;
        }
        #endregion
    }
}
