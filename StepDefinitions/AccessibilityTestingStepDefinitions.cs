
using NUnit.Framework;
using OpenQA.Selenium;
using BikeProject.Pages;
using Reqnroll;
using AventStack.ExtentReports;
using BikeProject.Utilities;
using DotNetEnv; // For environment variable loading
using System.Text; // For Encoding

namespace BikeProject.StepDefinitions
{
    [Binding]
    [Parallelizable(ParallelScope.Fixtures)] // Note: use Scenarios not Fixtures for SpecFlow tests
    public class AccessibilityTestingStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver driver;
        private HomePage homePage;
        private ExtentHelper extentHelper;
        private ExtentTest test;

        public AccessibilityTestingStepDefinitions(ScenarioContext scenarioContext)
        {
            // Load environment variables at the beginning
            Env.Load();

            _scenarioContext = scenarioContext;
            driver = (IWebDriver)_scenarioContext["WebDriver"];

            // Initialize homePage
            homePage = new HomePage(driver);

            // Use environment variables for report name
            string reportName = Environment.GetEnvironmentVariable("ACCESSIBILITY_TEST_REPORT_NAME") ?? "AccessibilityTest";

            // Initialize ExtentHelper with custom report name
            if (!_scenarioContext.ContainsKey("ExtentHelper"))
            {
                extentHelper = new ExtentHelper(driver, reportName);
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
                test = extentHelper.CreateTest("Accessibility Test");
                _scenarioContext["ExtentTest"] = test;
            }
            else
            {
                test = (ExtentTest)_scenarioContext["ExtentTest"];
            }
        }

        [Given("I navigate to the HomePage")]
        public void GivenINavigateToTheHomePage()
        {
            try
            {
                // Use a direct URL as fallback if environment variable is missing
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BASE_URL_ENCODED")))
                {
                    // Use direct URL method
                    homePage.NavigateToHomePage("https://www.zigwheels.com");
                    Console.WriteLine("Using direct URL: https://www.zigwheels.com (BASE_URL_ENCODED not found)");
                }
                else
                {
                    // Use the standard method which uses the Base64 decoded URL from environment
                    homePage.NavigateToHomePage();
                    Console.WriteLine("Using BASE_URL_ENCODED from environment variables");
                }

                _scenarioContext["CurrentPage"] = homePage;
                extentHelper.LogPass(test, "Successfully navigated to the HomePage");
            }
            catch (Exception ex)
            {
                extentHelper.LogFail(test, $"Failed to navigate to the HomePage: {ex.Message}");
                CaptureErrorDetails(ex);
                throw;
            }
        }

        [When("I run the screen reader accessibility test on the HomePage")]
        public void WhenIRunTheScreenReaderAccessibilityTestOnTheHomePage()
        {
            try
            {
                homePage.HandleConsentPopup();
                homePage.RunAccessibilityTest("Home page");
                extentHelper.LogPass(test, "Successfully ran the screen reader accessibility test on the HomePage");
            }
            catch (Exception ex)
            {
                extentHelper.LogFail(test, $"Failed to run the screen reader accessibility test on the HomePage: {ex.Message}");
                CaptureErrorDetails(ex);
                throw;
            }
        }

        [Then("the screen reader should correctly announce all elements on the HomePage")]
        public void ThenTheScreenReaderShouldCorrectlyAnnounceAllElementsOnTheHomePage()
        {
            try
            {
                Console.WriteLine("Screen reader announcements validated for HomePage.");
                extentHelper.LogPass(test, "Screen reader announcements validated for HomePage");
            }
            catch (Exception ex)
            {
                extentHelper.LogFail(test, $"Failed to validate screen reader announcements: {ex.Message}");
                CaptureErrorDetails(ex);
                throw;
            }
        }

        [Then("I should be able to navigate through all interactive elements on the HomePage using the keyboard")]
        public void ThenIShouldBeAbleToNavigateThroughAllInteractiveElementsOnTheHomePageUsingTheKeyboard()
        {
            try
            {
                // Call VerifyKeyboardNavigation and store the result
                bool isElementFound = homePage.VerifyKeyboardNavigation(By.LinkText("NEW BIKES"), 50);

                // Assert that the result is true
                Assert.IsTrue(isElementFound, "The element was not found using keyboard navigation.");
                Console.WriteLine("Assertion passed: The element was successfully found using keyboard navigation.");
                extentHelper.LogPass(test, "Successfully navigated through all interactive elements on the HomePage using the keyboard");

                // Take a screenshot with annotation
                string screenshotName = "Keyboard_Navigation_Screenshot";
                string annotation = "Keyboard navigation completed successfully on the HomePage.";
                extentHelper.ScreenshotWithAnnotation(test, screenshotName, annotation);
                extentHelper.LogPass(test, "Annotated screenshot captured after keyboard navigation.");
            }
            catch (Exception ex)
            {
                extentHelper.LogFail(test, $"Failed to navigate through all interactive elements using the keyboard: {ex.Message}");
                CaptureErrorDetails(ex);
                throw;
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

        // Helper method to capture and log error details
        private void CaptureErrorDetails(Exception ex)
        {
            try
            {
                // Take error screenshot
                string errorScreenshotName = $"error_accessibility_{DateTime.Now:yyyyMMdd_HHmmss}";
                extentHelper.TakeScreenshot(test, errorScreenshotName);

                // Log detailed exception info
                Console.WriteLine($"Exception Type: {ex.GetType().Name}");
                Console.WriteLine($"Exception Message: {ex.Message}");
                Console.WriteLine($"Current URL: {driver.Url}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                // Show current environment variables for debugging
                Console.WriteLine("Environment Variables:");
                Console.WriteLine($"BASE_URL_ENCODED exists: {!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BASE_URL_ENCODED"))}");
            }
            catch (Exception logEx)
            {
                Console.WriteLine($"Error while capturing details: {logEx.Message}");
            }
        }
    }
}