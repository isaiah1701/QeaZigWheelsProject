using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

using System;

namespace BikeProject.Pages
{
    class UpcomingBikesPage : BasePage // Implement BasePage
    {
        // Constructor to initialize the WebDriver
        public UpcomingBikesPage(IWebDriver webDriver) : base(webDriver) // Call base constructor
        {
        }

        // Locators for elements on the Upcoming Bikes page
        private By manufacturerDropdown = By.CssSelector("select.custom-select[name='makeId']"); // Updated locator based on provided HTML
        private By priceFilterInput = By.Id("priceFilterInputId"); // Replace with actual ID
        private By applyFilterButton = By.Id("applyFilterButtonId"); // Replace with actual ID
        private By bikeElements = By.ClassName("bikeClassName"); // Replace with actual class name
        private By moreMenu = By.LinkText("MORE");
        private By usedCarsLink = By.LinkText("Used Cars");
        private By cruiserButton = By.Id("Cruiser"); // Locator for the Cruiser button
        private bool clicked = false; 

        // Method to navigate to the Upcoming Bikes page
        public void NavigateToPage()
        {
            NavigateToUrl("https://www.zigwheels.com/upcoming-bikes"); // Use method from BasePage
        }

        // Method to select a manufacturer by clicking an anchor element with a specific href
        public void SelectManufacturer(string manufacturer)
        {
            Scroll(1000);
            
            try
            {
                // Locate the anchor element with the specified href (e.g., "upcoming-honda-bikes")
                By manufacturerLink = By.CssSelector("a[href='upcoming-honda-bikes']");

                // Wait for the link to be clickable
                var linkElement = wait.Until(ExpectedConditions.ElementToBeClickable(manufacturerLink));

                // Scroll to the link and click it
                actions.MoveToElement(linkElement).Perform();
                linkElement.Click();

                Console.WriteLine($"Successfully clicked the link for manufacturer: {manufacturer}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to click the link for manufacturer {manufacturer}: {ex.Message}");
                throw;
            }
        }


        // Method to click the Cruiser button
        public bool ClickCruiserButton()
        {
            var cruiserButtonElement = wait.Until(ExpectedConditions.ElementToBeClickable(cruiserButton));
            cruiserButtonElement.Click();
            clicked = true;
            return clicked; 
        }



        public void LoadZigWheelsAndFilterUpcomingBikesBelow4L()
        {
            // Step 1: Navigate to the upcoming bikes page
            
            Thread.Sleep(2000); // Allow time for page elements to load

            maximiseWindow();

            

            // Step 3: Wait for DOM ready
            WebDriverWait wait2 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait2.Until(drv => ((IJavaScriptExecutor)drv)
                .ExecuteScript("return document.readyState").ToString().Equals("complete"));

            // Step 4: Inject JavaScript
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            string script = @"
    (() => {
        const hideExpensiveBikes = () => {
            const cards = document.querySelectorAll('li.modelItem[data-price]');
            cards.forEach(card => {
                const priceAttr = card.getAttribute('data-price');
                const price = parseInt(priceAttr);
                if (!isNaN(price)) {
                    if (price >= 400000) {
                        card.style.display = 'none';
                        console.log('Hid Honda bike with price:', price);
                    } else {
                        card.style.display = '';

                        // Fix image rendering
                        const img = card.querySelector('img.lazy_image');
                        if (img) {
                            img.style.height = 'auto';
                            img.style.width = '100%';

                            if (!img.complete || img.naturalHeight === 0) {
                                const src = img.getAttribute('src');
                                if (src) img.setAttribute('src', src);
                            }
                        }
                    }
                }
            });
        };

        window.hondaBikePriceFilter = setInterval(hideExpensiveBikes, 300);
        console.log('Live filtering for Honda bikes ≥ 400000 is active...'); })(); ";

            js.ExecuteScript(script);
        }
    }
}