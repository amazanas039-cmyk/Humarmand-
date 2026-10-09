using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Hunarmand.Data;
using Hunarmand.Hubs;
using Hunarmand.Repositories;
using Hunarmand.Services;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:5053");

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o => {
    o.IdleTimeout = TimeSpan.FromMinutes(120);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("Nominatim", c => {
    c.BaseAddress = new Uri(builder.Configuration["Geocoding:NominatimBaseUrl"] ?? "https://nominatim.openstreetmap.org");
    c.DefaultRequestHeaders.UserAgent.ParseAdd(builder.Configuration["Geocoding:UserAgent"] ?? "Hunarmand/1.0");
});

// Repositories
builder.Services.AddScoped<DatabaseConnection>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<LabourerRepository>();
builder.Services.AddScoped<JobRepository>();
builder.Services.AddScoped<ReviewRepository>();
builder.Services.AddScoped<DisputeRepository>();
builder.Services.AddScoped<NotificationRepository>();
builder.Services.AddScoped<AnalyticsRepository>();
builder.Services.AddScoped<ChatRepository>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILabourerService, LabourerService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IMatchingService, MatchingService>();
builder.Services.AddScoped<IReputationService, ReputationService>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IDisputeService, DisputeService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IGeocodingService, GeocodingService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapHub<DisputeChatHub>("/hubs/dispute");
app.MapHub<NotificationHub>("/hubs/notify");

app.Run();

