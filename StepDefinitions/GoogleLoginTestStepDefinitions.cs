using Reqnroll;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using BikeProject.Pages;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using AventStack.ExtentReports;
using BikeProject.Utilities;
using DotNetEnv;
using System.Text;

namespace BikeProject.StepDefinitions
{
    [Binding]
    [Parallelizable(ParallelScope.Fixtures)]
    public class GoogleLoginTestStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly IWebDriver driver;
        private readonly HomePage homePage;
        private ExtentHelper extentHelper;
        private ExtentTest test;

        public GoogleLoginTestStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            driver = (IWebDriver)_scenarioContext["WebDriver"];
            homePage = new HomePage(driver); // Initialize homePage in the constructor

            // Load environment variables from .env file
            Env.Load();

            // Get encoded email and password from environment variables
            string? encodedEmail = Environment.GetEnvironmentVariable("GOOGLE_EMAIL");
            string? encodedPassword = Environment.GetEnvironmentVariable("GOOGLE_PASSWORD");

            // Decode the base64 encoded email and password (with null checks)
            string email = encodedEmail != null ? Base64Decode(encodedEmail) : string.Empty;
            string password = encodedPassword != null ? Base64Decode(encodedPassword) : string.Empty;

            // Store decoded values in scenario context for later use
            _scenarioContext["GoogleEmail"] = email;
            _scenarioContext["GooglePassword"] = password;

            // Initialize ExtentHelper with custom report name
            if (!_scenarioContext.ContainsKey("ExtentHelper"))
            {
                extentHelper = new ExtentHelper(driver, "GoogleLoginTest");
                extentHelper.InitializeReport();
                _scenarioContext["ExtentHelper"] = extentHelper;
            }
            else
            {
                extentHelper = (ExtentHelper)_scenarioContext["ExtentHelper"];
            }

            // Create test if not already created
            if (!_scenarioContext.ContainsKey("ExtentTest"))
            {
                test = extentHelper.CreateTest("Google Login Test");
                _scenarioContext["ExtentTest"] = test;
            }
            else
            {
                test = (ExtentTest)_scenarioContext["ExtentTest"];
            }
        }

        // Base64 decoding method
        private string Base64Decode(string base64EncodedData)
        {
            if (string.IsNullOrEmpty(base64EncodedData))
                throw new ArgumentNullException(nameof(base64EncodedData), "Encoded data cannot be null or empty.");

            byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        [Given(@"I navigate to the Google login home page")]
        public void GivenINavigateToTheGoogleLoginHomePage()
        {
            try
            {
                homePage.NavigateToHomePage(); // Navigate to home page as a prerequisite
                _scenarioContext["HomePage"] = homePage; // Store homePage in ScenarioContext

                extentHelper.LogPass(test, "Successfully navigated to the Google login home page");
            }
            catch (Exception ex)
            {
                extentHelper.LogFail(test, $"Failed to navigate to the Google login home page: {ex.Message}");
                throw;
            }
        }

        [Given(@"I handle the Google login consent popup")]
        public void GivenIHandleTheGoogleLoginConsentPopup()
        {
            try
            {
                homePage.HandleConsentPopup();

                extentHelper.LogPass(test, "Successfully handled the Google login consent popup");
            }
            catch (Exception ex)
            {
                extentHelper.LogFail(test, $"Failed to handle the Google login consent popup: {ex.Message}");
                throw;
            }
        }

        [When(@"I attempt to login with Google using email ""(.*)"" and password ""(.*)""")]
        public void WhenIAttemptToLoginWithGoogleUsingEmailAndPassword(string encodedEmail, string encodedPassword)
        {
            try
            {
                // Decode the Base64-encoded email and password
                string email = Base64Decode(encodedEmail);
                string password = Base64Decode(encodedPassword);

                // Wait for the page to load
                Thread.Sleep(2000);

                // Attempt Google login using the decoded email
                homePage.AttemptGoogleLogin(email);

                // Log success
                extentHelper.LogPass(test, $"Successfully attempted to login with email: {email}");
            }
            catch (Exception ex)
            {
                // Log failure
                extentHelper.LogFail(test, $"Failed to attempt login with email {encodedEmail}: {ex.Message}");
                throw;
            }
        }

        [Then(@"I should see a warning after entering the Google login username")]
        public void ThenIShouldSeeAWarningAfterEnteringTheGoogleLoginUsername()
        {
            try
            {
                // Wait for the URL to contain the expected substring
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                bool isUrlCorrect = wait.Until(d => d.Url.Contains("https://accounts.google.com/v3/signin/rejected"));

                // Assert that the URL contains the expected substring
                Assert.IsTrue(isUrlCorrect, "The URL does not contain 'https://accounts.google.com/v3/signin/rejected'.");
                extentHelper.LogPass(test, "The URL contains 'https://accounts.google.com/v3/signin/rejected'.");
                Console.WriteLine("The URL contains 'https://accounts.google.com/v3/signin/rejected'.");

                // Use JavaScript to ensure the DOM is fully loaded
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                bool isDomReady = wait.Until(d => (bool)js.ExecuteScript("return document.readyState === 'complete'"));
                Assert.IsTrue(isDomReady, "The DOM did not fully load.");
                extentHelper.LogPass(test, "The DOM is fully loaded.");

                // Wait for a specific element on the new page to appear
                By warningElement = By.XPath("//div[contains(text(), 'Couldn’t sign you in')]");
                wait.Until(ExpectedConditions.ElementIsVisible(warningElement));

                // Take a screenshot named "Warning Screen"
                extentHelper.TakeScreenshot(test, "Warning Screen");
                extentHelper.LogPass(test, "Screenshot captured: Warning Screen");

                // Write down all the text on the page
                string pageText = driver.FindElement(By.TagName("body")).Text;
                Console.WriteLine("Page Text:");
                Console.WriteLine(pageText);
            }
            catch (WebDriverTimeoutException)
            {
                extentHelper.LogFail(test, "The URL did not contain 'https://accounts.google.com/v3/signin/rejected' or the warning element did not appear within the timeout period.");
                Assert.Fail("The URL did not contain 'https://accounts.google.com/v3/signin/rejected' or the warning element did not appear within the timeout period.");
            }
            catch (Exception ex)
            {
                extentHelper.LogFail(test, $"An error occurred while verifying the URL or extracting page text: {ex.Message}");
                Assert.Fail($"An error occurred while verifying the URL or extracting page text: {ex.Message}");
            }
            finally
            {
                try
                {
                    // Flush the report to save it
                    extentHelper.FlushReport();
                }
                catch (Exception ex)
                {
                    extentHelper.LogWarning(test, $"Failed to flush the report: {ex.Message}");
                }

                // Quit the WebDriver
                driver.Quit();
            }
        }
    }
}

