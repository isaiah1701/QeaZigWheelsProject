using Microsoft.AspNetCore.Razor.Language.Intermediate;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Text; // For Encoding
using DotNetEnv; // For environment variable loading

namespace BikeProject.Pages
{
    public class HomePage : BasePage
    {
        private By newsAndReviewsLink = By.LinkText("NEWS & REVIEWS");
        private By newCarsLink = By.LinkText("NEW CARS");
        private By newBikesLink = By.LinkText("NEW BIKES");
        private By scootersLink = By.LinkText("SCOOTERS");
        private By moreMenu = By.LinkText("MORE");
        private By usedCarsLink = By.LinkText("Used Cars");
        private string baseUrl;

        public HomePage(IWebDriver webDriver) : base(webDriver)
        {
            // Load environment variables if they haven't been loaded already
            Env.Load();

            // Get encoded base URL from environment variable and decode it
            string encodedBaseUrl = Environment.GetEnvironmentVariable("BASE_URL_ENCODED");
            baseUrl = Base64Decode(encodedBaseUrl);
        }

        // Base64 decoding method
        public static string Base64Decode(string encodedData)
        {
            if (string.IsNullOrEmpty(encodedData))
                throw new ArgumentNullException(nameof(encodedData), "Encoded data cannot be null or empty.");

            byte[] data = Convert.FromBase64String(encodedData);
            return Encoding.UTF8.GetString(data);
        }

        // Method to navigate to the Home Page using the decoded URL from environment variables
        public void NavigateToHomePage()
        {
            driver.Navigate().GoToUrl(baseUrl);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.ElementIsVisible(By.LinkText("NEW BIKES")));
        }

        // Overload to allow passing a URL directly if needed
        public void NavigateToHomePage(string url)
        {
            driver.Navigate().GoToUrl(url);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.ElementIsVisible(By.LinkText("NEW BIKES")));
        }
    }
}
