using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SkillBridge.Models;
using SkillBridge.Services;

namespace SkillBridge.Pages.Auth
{
    public class LabourerRegisterModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IGeocodingService _geocodingService;
        private readonly ICategoryService _categoryService; // Assume you have a service to fetch categories

        public LabourerRegisterModel(IUserService userService, IGeocodingService geocodingService, ICategoryService categoryService)
        {
            _userService = userService;
            _geocodingService = geocodingService;
            _categoryService = categoryService;
        }

        // Step handling – default to 1 if not set
        [BindProperty]
        public int Step { get; set; } = 1;

        // Main input model – holds all fields across steps
        [BindProperty]
        public LabourerInputModel Input { get; set; } = new LabourerInputModel();

        // Helper for rendering category dropdown
        public IEnumerable<SelectListItem> CategoryOptions => _categoryService.GetAllCategories()
            .Select(c => new SelectListItem { Value = c.CategoryID.ToString(), Text = c.Name });

        public List<string> DaysOfWeek => new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        // Load the current step from TempData (persisted across redirects)
        public void OnGet()
        {
            if (TempData.ContainsKey("CurrentStep"))
            {
                Step = (int)TempData["CurrentStep"]!;
                TempData.Keep("CurrentStep");
            }
            if (TempData.ContainsKey("LabourerReg"))
                TempData.Keep("LabourerReg");
            // Keep Step in TempData for subsequent postbacks
            TempData["CurrentStep"] = Step;
        }

        // Helper to persist the aggregated registration data between steps
        private void SavePartialRegistration()
        {
            LabourerRegistrationDto? existing = null;
            if (TempData.ContainsKey("LabourerReg"))
            {
                existing = JsonSerializer.Deserialize<LabourerRegistrationDto>(TempData["LabourerReg"].ToString()!);
            }
            existing ??= new LabourerRegistrationDto();

            // Merge current input into the DTO (only fields relevant to the current step are populated)
            existing.FullName = Input!.FullName ?? existing.FullName;
            existing.Email = Input!.Email ?? existing.Email;
            existing.Phone = Input!.Phone ?? existing.Phone;
            existing.Password = Input!.Password ?? existing.Password;
            existing.CategoryId = Input!.CategoryId ?? existing.CategoryId;
            existing.ExperienceYears = Input!.ExperienceYears ?? existing.ExperienceYears;
            existing.HourlyRate = Input!.HourlyRate ?? existing.HourlyRate;
            existing.Bio = Input!.Bio ?? existing.Bio;
            existing.AvailableDays = Input!.AvailableDays ?? existing.AvailableDays;
            existing.StartTimes = Input!.StartTimes ?? existing.StartTimes;
            existing.EndTimes = Input!.EndTimes ?? existing.EndTimes;
            existing.Latitude = Input!.Latitude ?? existing.Latitude;
            existing.Longitude = Input!.Longitude ?? existing.Longitude;

            TempData["LabourerReg"] = JsonSerializer.Serialize(existing);
        }

        // Load the DTO back into Input for a given step
        private void LoadPartialRegistration()
        {
            if (TempData.ContainsKey("LabourerReg"))
            {
                var json = TempData.Peek("LabourerReg")?.ToString();
                if (json != null)
                {
                    var dto = JsonSerializer.Deserialize<LabourerRegistrationDto>(json);
                    if (dto != null)
                    {
                        Input.FullName = dto.FullName;
                        Input.Email = dto.Email;
                        Input.Phone = dto.Phone;
                        Input.Password = dto.Password;
                        Input.CategoryId = dto.CategoryId;
                        Input.ExperienceYears = dto.ExperienceYears;
                        Input.HourlyRate = dto.HourlyRate;
                        Input.Bio = dto.Bio;
                        Input.AvailableDays = dto.AvailableDays;
                        Input.StartTimes = dto.StartTimes;
                        Input.EndTimes = dto.EndTimes;
                        Input.Latitude = dto.Latitude;
                        Input.Longitude = dto.Longitude;
                    }
                }
                TempData.Keep("LabourerReg");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // ── Per-step validation: remove errors for fields that don't belong to this step ──
            if (Step == 1)
            {
                // Only validate step-1 fields; remove everything else
                var keepKeys = new[] { "Input.FullName", "Input.Email", "Input.Phone", "Input.Password", "Input.ConfirmPassword" };
                foreach (var key in ModelState.Keys.Where(k => !keepKeys.Contains(k)).ToList())
                    ModelState.Remove(key);
            }
            else if (Step == 2)
            {
                // Only validate step-2 fields
                var removeKeys = new[] { "Input.FullName", "Input.Email", "Input.Phone", "Input.Password", "Input.ConfirmPassword" };
                foreach (var key in removeKeys) ModelState.Remove(key);
            }
            else if (Step == 3)
            {
                // Availability — no required server-side rules, clear everything
                ModelState.Clear();
            }
            else if (Step == 4)
            {
                // Location — no required server-side rules, clear everything
                ModelState.Clear();
            }

            if (!ModelState.IsValid)
            {
                TempData.Keep("LabourerReg");
                TempData["CurrentStep"] = Step;
                return Page();
            }

            // Persist inputs for this step
            SavePartialRegistration();

            if (Step < 4)
            {
                // Move to next step
                Step++;
                TempData["CurrentStep"] = Step;
                LoadPartialRegistration();
                return Page();
            }

            // Final step – create Labourer entity and register
            var reg = JsonSerializer.Deserialize<LabourerRegistrationDto>(TempData["LabourerReg"].ToString()!);
            if (reg == null)
            {
                ModelState.AddModelError(string.Empty, "Registration data missing.");
                TempData["CurrentStep"] = 1;
                return Page();
            }

            // Resolve coordinates if not already set (fallback to geocoding service based on a placeholder address)
            double lat = reg.Latitude ?? 0;
            double lng = reg.Longitude ?? 0;
            if (lat == 0 && lng == 0 && !string.IsNullOrWhiteSpace(reg.Address))
            {
                var coords = await _geocodingService.GeocodeAsync(reg.Address);
                if (coords != null)
                {
                    lat = coords.Value.lat;
                    lng = coords.Value.lng;
                }
            }

            var labourer = new SkillBridge.Models.Labourer
            {
                FullName = reg!.FullName,
                Email = reg!.Email,
                Phone = reg!.Phone,
                CategoryID = reg!.CategoryId ?? 0,
                ExperienceYears = reg!.ExperienceYears ?? 0,
                HourlyRate = reg!.HourlyRate ?? 0,
                Bio = reg!.Bio,
                AvailabilitySchedule = BuildAvailabilityJson(reg),
                Latitude = (decimal)lat,
                Longitude = (decimal)lng,
                Role = "Labourer",
                IsActive = true
            };

            bool success = await _userService.RegisterLabourerAsync(labourer, reg.Password ?? string.Empty);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "A user with this email already exists.");
                TempData["CurrentStep"] = 1;
                return Page();
            }

            // Clean up TempData
            TempData.Remove("LabourerReg");
            TempData.Remove("CurrentStep");
            return RedirectToPage("/Auth/LabourerLogin", new { registered = 1 });
        }

        // Helper to serialize availability into JSON expected by the DB (simplified example)
        private string BuildAvailabilityJson(LabourerRegistrationDto reg)
        {
            var schedule = new List<object>();
            if (reg.AvailableDays != null)
            {
                foreach (var day in reg.AvailableDays)
                {
                    string? start = null;
                    string? end = null;
                    reg.StartTimes?.TryGetValue(day, out start);
                    reg.EndTimes?.TryGetValue(day, out end);
                    schedule.Add(new { Day = day, Start = start, End = end });
                }
            }
            return JsonSerializer.Serialize(schedule);
        }
    }

    // Consolidated DTO that lives in TempData between steps
    public class LabourerRegistrationDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Password { get; set; }
        // Step 2 fields
        public int? CategoryId { get; set; }
        public int? ExperienceYears { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? Bio { get; set; }
        // Step 3 fields
        public List<string>? AvailableDays { get; set; }
        public Dictionary<string, string>? StartTimes { get; set; }
        public Dictionary<string, string>? EndTimes { get; set; }
        // Step 4 fields
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Address { get; set; }
    }

    public class LabourerInputModel
    {
        // Step 1
        [Required]
        [StringLength(150)]
        public string? FullName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string? Email { get; set; }

        [Required]
        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        // Step 2
        public int? CategoryId { get; set; }
        public int? ExperienceYears { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? Bio { get; set; }

        // Step 3 – availability
        public List<string>? AvailableDays { get; set; }
        public Dictionary<string, string>? StartTimes { get; set; }
        public Dictionary<string, string>? EndTimes { get; set; }

        // Step 4 – location
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
