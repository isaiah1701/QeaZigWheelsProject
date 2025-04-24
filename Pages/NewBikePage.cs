using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;

namespace BikeProject.Pages
{
    class NewBikePage : BasePage // Inherit from BasePage
    {
        // Constructor to initialize the WebDriver
        public NewBikePage(IWebDriver webDriver) : base(webDriver) // Call base constructor
        {
        }

        // Locators for elements on the New Bikes page
        private By popularTab = By.CssSelector("li[data-track-label='popular-tab']");
        private By latestTab = By.CssSelector("li[data-track-label='latest-tab']");
        private By upcomingTab = By.CssSelector("li[data-track-label='upcoming-tab']");
        private By allUpcomingBikesLink = By.XPath("//*[@id='zw-cmnSilder']/div[2]/a"); // Updated locator
        private By upcomingBikes = By.CssSelector(".upcoming-bikes .bike-card a");

        // Method to navigate to the Popular section
        public void GoToPopular()
        {
            driver.FindElement(popularTab).Click();
        }

        // Method to navigate to the Latest section
        public void GoToLatest()
        {
            driver.FindElement(latestTab).Click();
        }

        // Method to navigate to the Upcoming section
        public void GoToUpcoming()
        {
            Thread.Sleep(1000); 
            driver.FindElement(upcomingTab).Click();
            Thread.Sleep(2000); // Allow time for the tab to load
        }

        // Method to navigate to "All Upcoming Bikes" 
        public void NavigateToSeeAllUpcomingBikes()
        {
            // Click on the Upcoming tab
            GoToUpcoming();
            Thread.Sleep(2000); // Allow time for the tab to load

            // Scroll down a bit to make the "All Upcoming Bikes" link visible
            Scroll(300);
            Thread.Sleep(2000); // Allow time for the page to adjust

            // Wait for the "All Upcoming Bikes" link to be visible and clickable
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            IWebElement element = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='/upcoming-bikes']")));

            // Click the element
            element.Click();

            Thread.Sleep(2000); // Allow time for the new page to load
        }
    }
}
