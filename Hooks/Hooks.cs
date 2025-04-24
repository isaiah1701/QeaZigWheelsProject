using Reqnroll;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using BikeProject.Pages;
using DotNetEnv; // For environment variable loading

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

            var options = new ChromeOptions();

            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--window-size=1920,1080");

            // New instance per scenario ensures thread safety
            IWebDriver driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();

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
                driver.Quit();
                Console.WriteLine("AfterScenario: WebDriver quit.");
            }
        }
    }
}