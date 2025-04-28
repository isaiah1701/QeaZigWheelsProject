using Reqnroll;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using BikeProject.Pages;
using DotNetEnv;
using OpenQA.Selenium.DevTools; // For environment variable loading

namespace BikeProject.Hooks
{
    [Binding]
    public class Hooks
    {
        private readonly ScenarioContext _scenarioContext;

        public Hooks(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            // Load environment variables BEFORE creating HomePage
            Env.Load();

            // Create ChromeDriver options
            var options = new ChromeOptions();

            // Add common arguments for CI/CD and local environments
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--window-size=1920,1080");
            options.AddUserProfilePreference("profile.default_content_settings_values.popups", 1);
            options.AddArgument("--disable-popup-blocking");

            // Add headless mode if running in CI/CD
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HEADLESS")) && Environment.GetEnvironmentVariable("HEADLESS") == "true")
            {
                options.AddArgument("--headless=new");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--disable-extensions");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--window-size=1920,1080");
                options.AddArgument("--start-maximized");
                options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                options.AddUserProfilePreference("profile.default_content_settings_values.popups",1);
                options.AddArgument("--disable-popup-blocking");
                
                Console.WriteLine("BeforeScenario: Running Chrome in headless mode.");
            }

            // Initialize WebDriver
            IWebDriver driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();

            bool isCI = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";
            int timeoutSeconds = isCI ? 60 : 30; // Set timeout based on environment    


            // Inject WebDriver into ScenarioContext
            _scenarioContext["WebDriver"] = driver;

            // Check if BASE_URL_ENCODED exists, use fallback if not
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BASE_URL_ENCODED")))
            {
                // Set default URL in environment
                Environment.SetEnvironmentVariable("BASE_URL_ENCODED", "aHR0cHM6Ly93d3cuemlnd2hlZWxzLmNvbQ==");
                Console.WriteLine("BeforeScenario: Setting default BASE_URL_ENCODED");
            }

            // Now create HomePage after environment variables are loaded
            _scenarioContext["HomePage"] = new HomePage(driver);

            Console.WriteLine("BeforeScenario: WebDriver initialized and HomePage stored in ScenarioContext.");
        }

        [AfterScenario]
        public void AfterScenario()
        {
            // Retrieve from context to ensure it's the correct driver instance
            if (_scenarioContext.TryGetValue("WebDriver", out var driverObj) && driverObj is IWebDriver driver)
            {
                try
                {
                    driver.Quit();
                    Console.WriteLine("AfterScenario: WebDriver quit.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"AfterScenario: Failed to quit WebDriver. Error: {ex.Message}");
                }
            }
        }
    }
}