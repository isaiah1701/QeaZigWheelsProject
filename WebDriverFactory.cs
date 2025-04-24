using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace BikeProject.Utilities
{
    public static class WebDriverFactory
    {
        public static IWebDriver CreateChromeDriver(bool headless = false)
        {
            var options = new ChromeOptions();
            
            // Add headless mode if running in CI/CD
            if (headless || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HEADLESS")))
            {
                options.AddArgument("--headless=new");
            }
            
            // Add other options needed for CI environments
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--window-size=1920,1080");
            
            // Create and return the WebDriver
            return new ChromeDriver(options);
        }
    }
}
