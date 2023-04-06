﻿using ILogger = Serilog.ILogger;

namespace PerfumeShop.Web.Configurations;


public static class WebDependencies
{
    public static ILogger SetLogger(IConfiguration configuration, ILoggingBuilder logging)
    {
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        logging.ClearProviders();
        logging.AddSerilog(logger);

        return logger;
    }

    public static void SetServices(IServiceCollection services)
    {
        services.AddCookieSettings();
        services.AddControllersWithViews();
        services.AddCoreServices();
        services.AddWebServices();       
    }

    public static void SetMiddleware(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseRequestLocalization("en-US", "en-US");
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapRazorPages();

        app.MapAreaControllerRoute(
            name: "UserDefault",
            areaName: "User",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapAreaControllerRoute(
            name: "AdminDefault",
            areaName: "Admin",
            pattern: "{area=Admin}/{controller=ManageProduct}/{action=Index}/{id?}");
    }
}
