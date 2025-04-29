
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using BikeProject.Pages;
using Reqnroll;
using System.Text; // Add this namespace at the top of the file


using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System;
using System.IO;
using BikeProject.Utilities;
using DotNetEnv;
namespace BikeProject.StepDefinitions
{
    [Binding]
    [Parallelizable(ParallelScope.Fixtures)]
    public class StepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private IWebDriver driver;
        private HomePage homePage;
        private UsedCarsPage usedCarsPage;
        private ExtentHelper extentHelper;
        private ExtentTest test;

        // Modify the constructor in StepDefinitions class where ExtentHelper is initialized
        public StepDefinitions(ScenarioContext scenarioContext)
        {
            // Load environment variables
            Env.Load();
            string encodedCity = Environment.GetEnvironmentVariable("CITY_NAME");
            string city = Base64Decode(encodedCity);  // This would decode to "Chennai"
            _scenarioContext = scenarioContext;
            driver = (IWebDriver)_scenarioContext["WebDriver"];
            // Check if HomePage exists in ScenarioContext, if not create it
            if (!_scenarioContext.ContainsKey("HomePage") || _scenarioContext["HomePage"] == null)
            {
                homePage = new HomePage(driver);
                _scenarioContext["HomePage"] = homePage;
                Console.WriteLine("Created and stored new HomePage in ScenarioContext");
            }
            else
            {
                homePage = (HomePage)_scenarioContext["HomePage"];
                Console.WriteLine("Retrieved existing HomePage from ScenarioContext");
            }

            // Initialize ExtentHelper with custom report name
            if (!_scenarioContext.ContainsKey("ExtentHelper"))
            {
                extentHelper = new ExtentHelper(driver, "ExtractPopularCarsTest");
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
                test = extentHelper.CreateTest("Extract Popular Cars Test");
                _scenarioContext["ExtentTest"] = test;
            }
            else
            {
                test = (ExtentTest)_scenarioContext["ExtentTest"];
            }
        }

        [Given(@"I have opened the home page")]
        public void GivenIHaveOpenedTheHomePage()
        {
            try
            {
                driver = (IWebDriver)_scenarioContext["WebDriver"];
                homePage = (HomePage)_scenarioContext["HomePage"];
                homePage.NavigateToUrl("https://www.zigwheels.com");
                _scenarioContext["homePage"] = homePage;

                extentHelper.LogPass(test, "Successfully navigated to the home page");
            }
            catch (Exception ex)
            {
                extentHelper.LogFail(test, $"Failed to navigate to home page: {ex.Message}");
                throw;
            }
        }

        [Given(@"I have handled the consent popup")]
        public void GivenIHaveHandledTheConsentPopup()
        {
            try
            {
                homePage = (HomePage)_scenarioContext["homePage"];
                homePage.HandleConsentPopup();

                extentHelper.LogPass(test, "Successfully handled the consent popup");
            }
            catch (Exception ex)
            {
                extentHelper.LogFail(test, $"Failed to handle consent popup: {ex.Message}");
                throw;
            }
        }

        [When(@"I navigate to the used cars page")]
        public void WhenINavigateToTheUsedCarsPage()
        {
            try
            {
                homePage = (HomePage)_scenarioContext["homePage"];
                homePage.NavigateToUsedCars();
                usedCarsPage = new UsedCarsPage((IWebDriver)_scenarioContext["WebDriver"]);
                _scenarioContext["usedCarsPage"] = usedCarsPage;

                extentHelper.LogPass(test, "Successfully navigated to the used cars page");
            }
            catch (Exception ex)
            {
                extentHelper.LogFail(test, $"Failed to navigate to used cars page: {ex.Message}");
                throw;
            }
        }

        [When(@"I dismiss the notification popup")]
        public void WhenIDismissTheNotificationPopup()
        {
            try
            {
                usedCarsPage = (UsedCarsPage)_scenarioContext["usedCarsPage"];
                usedCarsPage.DismissNotificationPopUp();

                extentHelper.LogPass(test, "Successfully dismissed the notification popup");
            }
            catch (Exception ex)
            {
                extentHelper.LogFail(test, $"Failed to dismiss notification popup: {ex.Message}");
                throw;
            }
        }

        [When(@"I navigate to the city {string} # Ensure this is a valid Base{int} string")]
        public void WhenINavigateToTheCity(string encodedCityName, int baseType)
        {
            try
            {
                // Decode the Base-64 encoded city name
                string cityName = Base64Decode(encodedCityName);

                // Call the method to navigate to the city
                var usedCarsPage = new UsedCarsPage(driver);
                usedCarsPage.NavigateToCity(cityName);

                Console.WriteLine($"Successfully navigated to the city: {cityName}");
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Invalid Base-{baseType} string: {encodedCityName}. Error: {ex.Message}");
                throw;
            }
        }

        [Then(@"I should be able to extract popular cars")]
        public void ThenIShouldBeAbleToExtractPopularCars()
        {
            try
            {
                // Retrieve the UsedCarsPage instance from ScenarioContext
                usedCarsPage = (UsedCarsPage)_scenarioContext["usedCarsPage"];

                // Call ExtractPopularCars and store the result
                bool isExtractionSuccessful = usedCarsPage.ExtractPopularCars();

                // Assert that the extraction was successful
                Assert.IsTrue(isExtractionSuccessful, "Failed to extract popular cars.");

                extentHelper.LogPass(test, "Successfully extracted popular cars");
                Console.WriteLine("Assertion passed: Popular cars were successfully extracted.");
            }
            catch (Exception ex)
            {
                extentHelper.LogFail(test, $"Failed to extract popular cars: {ex.Message}");
                Assert.Fail($"An error occurred while extracting popular cars: {ex.Message}");
            }
            finally
            {
                try
                {
                    // Take screenshot before quitting
                    string screenshotName = $"extract_cars_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";
                    extentHelper.TakeScreenshot(test, screenshotName);
                }
                catch (Exception ex)
                {
                    extentHelper.LogWarning(test, $"Failed to capture screenshot: {ex.Message}");
                }

                // Quit the WebDriver
                driver = (IWebDriver)_scenarioContext["WebDriver"];
                driver.Quit();

                // Flush the report to save it
                extentHelper.FlushReport();
            }
        }

        // Helper method to decode Base-64 strings
        private string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
