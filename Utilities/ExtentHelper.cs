
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Drawing;
using System.Drawing.Imaging;

namespace BikeProject.Utilities
{


    public class ExtentHelper
    {
        private static ExtentReports extent;
        private static ExtentTest test;
        private IWebDriver driver;
        private string reportName;
        private string screenshotsDirectory;

        public ExtentHelper(IWebDriver webDriver, string reportName = "extent")
        {
            driver = webDriver;
            this.reportName = reportName;
            this.screenshotsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Screenshots", reportName);

            string reportPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", $"{reportName}.html");
            Console.WriteLine($"Extent Report initialized at path: {reportPath}");
        }

        public void InitializeReport()
        {
            try
            {
                string reportFileName = $"{reportName}.html";
                string reportPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", reportFileName);

                // Ensure the Reports directory exists
                if (!Directory.Exists(Path.GetDirectoryName(reportPath)))
                {
                    Console.WriteLine("Creating Reports directory...");
                    Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
                }

                // Ensure screenshots directory exists
                if (!Directory.Exists(screenshotsDirectory))
                {
                    Console.WriteLine("Creating Screenshots directory...");
                    Directory.CreateDirectory(screenshotsDirectory);
                }

                Console.WriteLine($"Initializing ExtentSparkReporter with path: {reportPath}");
                ExtentSparkReporter htmlReporter = new ExtentSparkReporter(reportPath);
                htmlReporter.Config.DocumentTitle = "Zig Wheels Report";
                htmlReporter.Config.ReportName = "Site Testing Results";

                // Additional configuration for better report appearance
                htmlReporter.Config.Theme = AventStack.ExtentReports.Reporter.Config.Theme.Dark;
                htmlReporter.Config.CSS = ".badge-primary { background-color: #1976d2 }";
                htmlReporter.Config.JS = "";

                extent = new ExtentReports();
                extent.AttachReporter(htmlReporter);

                // Add system info to report
                Console.WriteLine("Adding system information to the report...");
                extent.AddSystemInfo("Environment", "Test");

                string browserInfo = GetBrowserInfo();
                Console.WriteLine($"Browser Info: {browserInfo}");
                extent.AddSystemInfo("Browser", browserInfo);

                string osInfo = Environment.OSVersion.ToString();
                Console.WriteLine($"OS Info: {osInfo}");
                extent.AddSystemInfo("OS", osInfo);

                Console.WriteLine("Extent report initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during report initialization: {ex.Message}");
                throw;
            }
        }

        private string GetBrowserInfo()
        {
            try
            {
                return ((IJavaScriptExecutor)driver).ExecuteScript(
                    "return navigator.userAgent;").ToString();
            }
            catch
            {
                return "Unknown";
            }
        }

        public ExtentTest CreateTest(string testName, string description = "")
        {
            return string.IsNullOrEmpty(description)
                ? extent.CreateTest(testName)
                : extent.CreateTest(testName, description);
        }

        public void LogInfo(ExtentTest test, string message)
        {
            test.Log(Status.Info, message);
        }

        public void LogPass(ExtentTest test, string message)
        {
            test.Log(Status.Pass, message);
        }

        public void LogFail(ExtentTest test, string message)
        {
            test.Log(Status.Fail, message);

            // Automatically take screenshot on failure
            TakeScreenshot(test, $"failure_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}");
        }

        public void LogWarning(ExtentTest test, string message)
        {
            test.Log(Status.Warning, message);
        }

        public void LogSkip(ExtentTest test, string message)
        {
            test.Log(Status.Skip, message);
        }

        public void TakeScreenshot(ExtentTest test, string screenshotName)
        {
            try
            {
                ITakesScreenshot ts = (ITakesScreenshot)driver;
                Screenshot screenshot = ts.GetScreenshot();
                string screenshotPath = Path.Combine(screenshotsDirectory, $"{screenshotName}.png");

                // Save the screenshot
                screenshot.SaveAsFile(screenshotPath);

                // Add the screenshot to the report
                test.AddScreenCaptureFromPath(screenshotPath);

                LogInfo(test, $"Screenshot captured: {screenshotName}");
            }
            catch (Exception ex)
            {
                LogWarning(test, $"Failed to capture screenshot: {ex.Message}");
            }
        }

        public void TakeScreenshotWithHighlight(ExtentTest test, string screenshotName, IWebElement elementToHighlight)
        {
            try
            {
                // First take a normal screenshot
                ITakesScreenshot ts = (ITakesScreenshot)driver;
                Screenshot screenshot = ts.GetScreenshot();
                string screenshotPath = Path.Combine(screenshotsDirectory, $"{screenshotName}.png");

                // Save the screenshot
                screenshot.SaveAsFile(screenshotPath);

                // Highlight the element with JavaScript
                string originalStyle = (string)((IJavaScriptExecutor)driver).ExecuteScript(
                    "var s = arguments[0].getAttribute('style'); " +
                    "arguments[0].setAttribute('style', 'border: 2px solid red; background-color: yellow; ' + s); " +
                    "return s;", elementToHighlight);

                // Take another screenshot with the highlight
                screenshot = ts.GetScreenshot();
                string highlightedScreenshotPath = Path.Combine(screenshotsDirectory, $"{screenshotName}_highlighted.png");
                screenshot.SaveAsFile(highlightedScreenshotPath);

                // Restore the element's original style
                ((IJavaScriptExecutor)driver).ExecuteScript(
                    "arguments[0].setAttribute('style', arguments[1]);",
                    elementToHighlight, originalStyle);

                // Add the highlighted screenshot to the report
                test.AddScreenCaptureFromPath(highlightedScreenshotPath);

                LogInfo(test, $"Screenshot with highlight captured: {screenshotName}");
            }
            catch (Exception ex)
            {
                LogWarning(test, $"Failed to capture screenshot with highlight: {ex.Message}");

                // Fall back to normal screenshot
                TakeScreenshot(test, screenshotName);
            }
        }

        public void TakeFullPageScreenshot(ExtentTest test, string screenshotName)
        {
            try
            {
                // Get the total height of the page
                long totalHeight = (long)((IJavaScriptExecutor)driver).ExecuteScript(
                    "return document.body.scrollHeight");
                long totalWidth = (long)((IJavaScriptExecutor)driver).ExecuteScript(
                    "return document.body.scrollWidth");

                // Get the current window size
                long viewportHeight = (long)((IJavaScriptExecutor)driver).ExecuteScript(
                    "return window.innerHeight");
                long viewportWidth = (long)((IJavaScriptExecutor)driver).ExecuteScript(
                    "return window.innerWidth");

                // Save original scroll position
                long originalScrollPosition = (long)((IJavaScriptExecutor)driver).ExecuteScript(
                    "return window.pageYOffset");

                // If the page is small enough, just take a regular screenshot
                if (totalHeight <= viewportHeight)
                {
                    TakeScreenshot(test, screenshotName);
                    return;
                }

                // Take multiple screenshots and stitch them together
                // For simplicity in this implementation, we'll just take a screenshot of the current viewport
                // A complete solution would require more complex image processing to stitch multiple screenshots

                string screenshotPath = Path.Combine(screenshotsDirectory, $"{screenshotName}_full.png");

                // Scroll to top
                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, 0)");

                // Take the screenshot
                Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                screenshot.SaveAsFile(screenshotPath);

                // Restore original scroll position
                ((IJavaScriptExecutor)driver).ExecuteScript($"window.scrollTo(0, {originalScrollPosition})");

                // Add the screenshot to the report
                test.AddScreenCaptureFromPath(screenshotPath);

                LogInfo(test, $"Full page screenshot captured: {screenshotName}");
            }
            catch (Exception ex)
            {
                LogWarning(test, $"Failed to capture full page screenshot: {ex.Message}");

                // Fall back to normal screenshot
                TakeScreenshot(test, screenshotName);
            }
        }

        public void ScreenshotWithAnnotation(ExtentTest test, string screenshotName, string annotation)
        {
            try
            {
                // First take a normal screenshot
                ITakesScreenshot ts = (ITakesScreenshot)driver;
                Screenshot screenshot = ts.GetScreenshot();

                string tempPath = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.png");
                screenshot.SaveAsFile(tempPath);

                // Load the image for annotation
                using (Bitmap bitmap = new Bitmap(tempPath))
                using (Graphics graphics = Graphics.FromImage(bitmap))
                using (Font font = new Font("Arial", 12, FontStyle.Bold))
                using (SolidBrush brush = new SolidBrush(Color.Red))
                {
                    // Add annotation text at the bottom of the image
                    graphics.DrawString(annotation, font, brush, 10, bitmap.Height - 30);

                    // Save the annotated image
                    string annotatedPath = Path.Combine(screenshotsDirectory, $"{screenshotName}_annotated.png");
                    bitmap.Save(annotatedPath, ImageFormat.Png);

                    // Add the annotated screenshot to the report
                    test.AddScreenCaptureFromPath(annotatedPath);
                }

                // Delete the temporary file
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                LogInfo(test, $"Annotated screenshot captured: {screenshotName}");
            }
            catch (Exception ex)
            {
                LogWarning(test, $"Failed to capture annotated screenshot: {ex.Message}");

                // Fall back to normal screenshot
                TakeScreenshot(test, screenshotName);
            }
        }

        public void FlushReport()
        {
            extent.Flush();
        }
    }
}