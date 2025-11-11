using Microsoft.AspNetCore.Mvc;
using InsurancePolicyForm.Models;
using InsurancePolicyForm.Services;

namespace InsurancePolicyForm.Controllers
{
    public class InsuranceController : Controller
    {
        private readonly IVirusScanService _virusScanService;

        public InsuranceController(IVirusScanService virusScanService)
        {
            _virusScanService = virusScanService;
        }

        public IActionResult Upload()
        {
            ViewData["Title"] = "Upload Insurance Declaration";
            
            // Create a list of insurance providers for the dropdown
            ViewBag.InsuranceProviders = new List<string>
            {
                "Select Insurance Provider",
                "State Farm",
                "Allstate",
                "GEICO",
                "Progressive",
                "Liberty Mutual",
                "Nationwide",
                "American Family",
                "Farmers Insurance",
                "Travelers",
                "USAA"
            };
            
            return View(new InsuranceDeclarationModel
            {
                EffectiveDate = DateTime.Today,
                ExpirationDate = DateTime.Today.AddYears(1),
                LeaseStartDate = DateTime.Today
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upload(InsuranceDeclarationModel model)
        {
            if (ModelState.IsValid)
            {
                // In a real application, we would process the form data here
                // For this static UI, we'll just redirect to a success view
                TempData["SuccessMessage"] = "Insurance policy uploaded successfully!";
                return RedirectToAction("Success");
            }

            // If model validation fails, repopulate the dropdown and return to the form
            ViewBag.InsuranceProviders = new List<string>
            {
                "Select Insurance Provider",
                "State Farm",
                "Allstate",
                "GEICO",
                "Progressive",
                "Liberty Mutual",
                "Nationwide",
                "American Family",
                "Farmers Insurance",
                "Travelers",
                "USAA"
            };
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ScanFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "No file provided" });
            }

            try
            {
                var scanResult = await _virusScanService.ScanFileAsync(file);
                return Json(new { success = true, result = scanResult, fileName = file.FileName });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error scanning file: {ex.Message}" });
            }
        }

        public IActionResult Success()
        {
            ViewData["Title"] = "Upload Successful";
            return View();
        }
    }
}
