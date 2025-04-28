using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ClosedXML.Excel;

using System.Text;

namespace BikeProject.Pages
{


    public class UsedCarsPage : BasePage
    {
        // Locators for elements on the Used Cars page
        private By searchBox = By.Id("gs_input5");
        private By searchButton = By.Id("searchButton");
        private By filterOptions = By.ClassName("filterOptions");
        bool executed = false;
        public UsedCarsPage(IWebDriver webDriver) : base(webDriver)
        {


        }

        // Method to search for a used car
        public void SearchForUsedCar(string carName)
        {
            driver.FindElement(searchBox).SendKeys(carName);

            Thread.Sleep(100000);
        }






        public void LoadZigWheelsAndFilterCarsBelow400K()
        {
            // Step 1: Navigate to the used cars page directly
            driver.Navigate().GoToUrl("https://www.zigwheels.com/used-car/Chennai");

            Thread.Sleep(2000); // Allow time for popup

            driver.Manage().Window.Maximize();

            // Step 2: Handle consent popup
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                IWebElement consentBtn = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions
                    .ElementToBeClickable(By.CssSelector("button[aria-label='Consent']")));
                consentBtn.Click();
                Console.WriteLine("Consent button clicked.");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("Consent button not found in time. Proceeding without clicking.");
            }

            // Step 3: Wait for DOM ready
            WebDriverWait wait2 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait2.Until(drv => ((IJavaScriptExecutor)drv)
                .ExecuteScript("return document.readyState").ToString().Equals("complete"));

            // Step 4: Inject JS
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            string script = @"
(() => {
const hideExpensiveCars = () => {
const listings = document.querySelectorAll('div.zw-sr-searchTarget.col-lg-4');
listings.forEach(card => {
const priceAnchor = card.querySelector('div[data-price]');
if (priceAnchor) {
const price = parseInt(priceAnchor.getAttribute('data-price'));
if (!isNaN(price)) {
if (price >= 400000) {
card.style.display = 'none';
console.log('Hid card with price:', price); } else { card.style.display = '';

// Force fix image rendering
const img = card.querySelector('img.reviewImage-used');
if (img) {
img.style.height = 'auto';
img.style.width = '100%';

// Force reload if broken (optional)
if (!img.complete || img.naturalHeight === 0) { const src = img.getAttribute('src'); if (src) img.setAttribute('src', src); } } } } } }); };

window.expensiveCarFilter = setInterval(hideExpensiveCars, 300); console.log('Live filtering for cars ≥ 400000 is active...'); })();
 ";

            js.ExecuteScript(script);

        }






        public bool ExtractPopularCars()
        {
            // Step 1: Handle consent popup
            try
            {



                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                IWebElement consentBtn = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions
                .ElementToBeClickable(By.CssSelector("button[aria-label='Consent']")));
                consentBtn.Click();
                Console.WriteLine("Consent button clicked.");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("Consent button not found in time. Proceeding without clicking.");
            }

            // Step 2: Wait for page load
            WebDriverWait wait2 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait2.Until(drv => ((IJavaScriptExecutor)drv)
            .ExecuteScript("return document.readyState").ToString().Equals("complete"));

            int carCount = 0;
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            // Create a new workbook or open an existing one
            string reportsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
            Directory.CreateDirectory(reportsFolderPath); // Ensure the Reports folder exists
            string filePath = Path.Combine(reportsFolderPath, "CarNamesAndPrices.xlsx");
            Console.WriteLine($"Saving file to: {filePath}");
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Car Names and Prices");

            // Add headers
            worksheet.Cell(1, 1).Value = "Popular Models";
            worksheet.Cell(1, 2).Value = "Year";
            worksheet.Cell(1, 3).Value = "Price (Lakhs)";

            int row = 2;

            // List of popular models to filter
            List<string> popularModels = new List<string> { "Maruti 800", "Maruti Swift", "Hyundai I10", "Hyundai Santro Xing", "Honda City", "Toyota Innova", "Toyota Fortuner", "Mahindra XUV500" };

            // Use HashSet to track unique car entries
            HashSet<string> uniqueCarEntries = new HashSet<string>();

            while (carCount < 15)
            {
                // Step 3: Get all car cards
                var carCards = driver.FindElements(By.CssSelector("div.zw-sr-searchTarget.col-lg-4"));

                foreach (var card in carCards)
                {
                    try
                    {
                        // Extract car name
                        var nameElement = card.FindElement(By.CssSelector("a.zw-sr-headingPadding"));
                        string carName = nameElement.Text.Trim();

                        // Check if the car name contains any of the popular models
                        if (popularModels.Any(model => carName.Contains(model, StringComparison.OrdinalIgnoreCase)))
                        {
                            // Extract year
                            var yearElement = card.FindElement(By.XPath(".//li[contains(text(), '20')]"));
                            string year = yearElement.Text.Trim();

                            // Extract price
                            var priceElement = card.FindElement(By.CssSelector("div[data-price]"));
                            string priceString = priceElement.GetAttribute("data-price");

                            // Parse price
                            int price = int.TryParse(priceString, out int parsedPrice) ? parsedPrice : 0;

                            // Convert price to lakhs
                            double priceInLakhs = price / 100000.0;

                            // Create a unique key for this car entry
                            string uniqueKey = $"{carName}|{year}|{priceInLakhs:N2}";

                            // Check if we've already recorded this car
                            if (!uniqueCarEntries.Contains(uniqueKey))
                            {
                                // Add to our set of unique cars
                                uniqueCarEntries.Add(uniqueKey);

                                // Write to console
                                Console.WriteLine($"Car Name: {carName}, Year: {year}, Price: ₹{priceInLakhs:N2} Lakhs");

                                // Write to Excel
                                worksheet.Cell(row, 1).Value = carName;
                                worksheet.Cell(row, 2).Value = year;
                                worksheet.Cell(row, 3).Value = priceInLakhs;
                                row++;

                                carCount++;

                                if (carCount >= 15)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Skipping duplicate: {carName}, Year: {year}, Price: ₹{priceInLakhs:N2} Lakhs");
                            }
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        Console.WriteLine("Missing name, year, or price on a card.");
                    }
                }

                if (carCount < 15)
                {
                    // Scroll down 200 pixels to load more results
                    js.ExecuteScript("window.scrollBy(0, 400);");
                    Thread.Sleep(400); // Allow time for new results to load
                }
            }

            // Save the workbook
            try
            {
                workbook.SaveAs(filePath);
                Console.WriteLine($"File saved successfully with {carCount} unique car entries.");
                executed = true;
                return executed;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
                return executed;
            }
        }
        public void NavigateToCity(string cityName)
        {
            // Locate the city element based on the city name
            var cityElement = driver.FindElement(By.XPath($"//ul[@id='popularCityList']//a[contains(@title, '{cityName}')]"));

            // Click on the city element
            cityElement.Click();
        }
    }

}

   

