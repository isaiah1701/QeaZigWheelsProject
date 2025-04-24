using Reqnroll;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using BikeProject.Pages;
using BikeProject.Utilities;
using AventStack.ExtentReports;
using OpenQA.Selenium.Support.UI;
using System.Text; // For Encoding
using DotNetEnv; // For environment variable loading


[Binding]
[Parallelizable(ParallelScope.Fixtures)]
public class HondaCruiserSteps
{
    private readonly ScenarioContext _scenarioContext;
    private IWebDriver driver;
    private HomePage homePage;
    private NewBikePage newBikePage;
    private UpcomingBikesPage upcomingBikesPage;
    private ExtentHelper extentHelper;
    private ExtentTest test;
    private string decodedManufacturer;
    private string baseUrl;
    private string upcomingBikesUrlTemplate;

    public HondaCruiserSteps(ScenarioContext scenarioContext)
    {
        // Load environment variables
        Env.Load();

        // Get encoded URLs from environment variables and decode them
        string encodedBaseUrl = Environment.GetEnvironmentVariable("BASE_URL_ENCODED");
        string encodedUrlTemplate = Environment.GetEnvironmentVariable("UPCOMING_BIKES_URL_TEMPLATE_ENCODED");

        baseUrl = Base64Decode(encodedBaseUrl);
        upcomingBikesUrlTemplate = Base64Decode(encodedUrlTemplate);

        _scenarioContext = scenarioContext;
        driver = (IWebDriver)_scenarioContext["WebDriver"];
        homePage = new HomePage(driver);

        // Initialize ExtentHelper with custom report name
        if (!_scenarioContext.ContainsKey("ExtentHelper"))
        {
            extentHelper = new ExtentHelper(driver, "NavigateToHondaCruiserBike");
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
            test = extentHelper.CreateTest("Navigate to Honda Cruiser Bike Test");
            _scenarioContext["ExtentTest"] = test;
        }
        else
        {
            test = (ExtentTest)_scenarioContext["ExtentTest"];
        }
    }

    // Base64 decoding method
    public static string Base64Decode(string encodedData)
    {
        if (string.IsNullOrEmpty(encodedData))
            throw new ArgumentNullException(nameof(encodedData), "Encoded data cannot be null or empty.");

        byte[] data = Convert.FromBase64String(encodedData);
        return Encoding.UTF8.GetString(data); // Encoding is now recognized
    }

    [Given(@"I navigate to the home page")]
    public void GivenINavigateToTheHomePage()
    {
        try
        {
            // Pass the decoded base URL to the NavigateToHomePage method
            // Assuming the method accepts a URL parameter - you may need to update HomePage.cs
            homePage.NavigateToUrl(baseUrl);
            extentHelper.LogPass(test, "Successfully navigated to the home page");
        }
        catch (Exception ex)
        {
            extentHelper.LogFail(test, $"Failed to navigate to home page: {ex.Message}");
            throw;
        }
    }

    [Given(@"I handle the consent popup")]
    public void GivenIHandleTheConsentPopup()
    {
        try
        {
            homePage.HandleConsentPopup();
            extentHelper.LogPass(test, "Successfully handled the consent popup");
        }
        catch (Exception ex)
        {
            extentHelper.LogFail(test, $"Failed to handle consent popup: {ex.Message}");
            throw;
        }
    }

    [When(@"I navigate to the New Bikes page")]
    public void WhenINavigateToTheNewBikesPage()
    {
        try
        {
            homePage.NavigateToNewBikes();
            extentHelper.LogPass(test, "Successfully navigated to the New Bikes page");
        }
        catch (Exception ex)
        {
            extentHelper.LogFail(test, $"Failed to navigate to New Bikes page: {ex.Message}");
            throw;
        }
    }

    [When(@"I navigate to see all upcoming bikes")]
    public void WhenINavigateToSeeAllUpcomingBikes()
    {
        try
        {
            newBikePage = new NewBikePage(driver);
            newBikePage.NavigateToSeeAllUpcomingBikes();
            _scenarioContext["newBikePage"] = newBikePage;
            extentHelper.LogPass(test, "Successfully navigated to see all upcoming bikes");
        }
        catch (Exception ex)
        {
            extentHelper.LogFail(test, $"Failed to navigate to see all upcoming bikes: {ex.Message}");
            throw;
        }
    }

    [When(@"I select the manufacturer ""(.*)""")]
    public void WhenISelectTheManufacturer(string encodedManufacturer)
    {
        try
        {
            // Decode the manufacturer name from the Base64 encoded string
            decodedManufacturer = Base64Decode(encodedManufacturer);

            upcomingBikesPage = new UpcomingBikesPage(driver);
            upcomingBikesPage.SelectManufacturer(decodedManufacturer);
            _scenarioContext["upcomingBikesPage"] = upcomingBikesPage;

            extentHelper.LogPass(test, $"Successfully selected manufacturer: {decodedManufacturer}");
        }
        catch (Exception ex)
        {
            extentHelper.LogFail(test, $"Failed to select manufacturer: {ex.Message}");
            throw;
        }
    }

    [When(@"I click the Cruiser button")]
    public void WhenIClickTheCruiserButton()
    {
        try
        {
            upcomingBikesPage = (UpcomingBikesPage)_scenarioContext["upcomingBikesPage"];
            upcomingBikesPage.ClickCruiserButton();
            extentHelper.LogPass(test, "Successfully clicked the Cruiser button");
        }
        catch (Exception ex)
        {
            extentHelper.LogFail(test, $"Failed to click Cruiser button: {ex.Message}");
            throw;
        }
    }

    [Then(@"I should see the list of Honda Cruiser bikes")]
    public void ThenIShouldSeeTheListOfHondaCruiserBikes()
    {
        try
        {
            // Wait for the page to fully load
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

            // Format the URL template with the manufacturer name
            string expectedUrl = string.Format(upcomingBikesUrlTemplate, decodedManufacturer.ToLower());

            // Assert: Verify the URL contains the expected path
            string currentUrl = driver.Url;
            Assert.IsTrue(currentUrl.Contains(expectedUrl),
                $"URL does not contain the expected path. Current URL: {currentUrl}");
            extentHelper.LogPass(test, $"URL verification passed: URL contains '{expectedUrl}'");

            // Take a full-page screenshot and add it to the Extent report
            string screenshotName = $"honda_cruiser_fullpage_{DateTime.Now:yyyyMMdd_HHmmss}";
            extentHelper.TakeFullPageScreenshot(test, screenshotName);
            extentHelper.LogPass(test, "Full-page screenshot captured successfully");
        }
        catch (Exception ex)
        {
            extentHelper.LogFail(test, $"Test failed with error: {ex.Message}");
            Assert.Fail($"Test failed with error: {ex.Message}");
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
