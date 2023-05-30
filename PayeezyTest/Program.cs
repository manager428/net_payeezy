using Microsoft.EntityFrameworkCore;
using PayeezyTest.Dto;
using PayeezyTest.Models;
using PayeezyTest.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<PayeezySettings>(builder.Configuration.GetSection("PayeezySettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<FtpSettings>(builder.Configuration.GetSection("FtpSettings"));

builder.Services.AddDbContext<PayeezyDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("PayeezyAppDBContext")));

builder.Services.AddControllersWithViews();

builder.Services.AddTransient<IPatientService, PatientService>();
builder.Services.AddTransient<IFtpService, FtpService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
