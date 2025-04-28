using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Selenium.Axe;





namespace BikeProject.Pages
{
    public abstract class BasePage
    {
        protected IWebDriver driver;
        protected WebDriverWait wait;
        protected Actions actions;
       

        public BasePage(IWebDriver webDriver)
        {

            this.driver = webDriver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            this.actions = new Actions(driver);

            // Automatically handle the consent pop-up
            HandleConsentPopup();
        }
public void RunAccessibilityTest(string pageName)
{
    try
    {
        // Ensure the DOM is fully loaded before running Axe
        new WebDriverWait(driver, TimeSpan.FromSeconds(10))
            .Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
 
        // Run Axe accessibility scan with retry on stale element
        var results = RetryIfStale(() => new AxeBuilder(driver).Analyze());
 
        if (results.Violations.Length > 0)
        {
            Console.WriteLine($"Accessibility violations found on {pageName}:");
            foreach (var violation in results.Violations)
            {
                Console.WriteLine($"- {violation.Description}");
                foreach (var node in violation.Nodes)
                {
                    Console.WriteLine($"  > HTML: {node.Html}");
                }
            }
        }
        else
        {
            Console.WriteLine($"No accessibility violations found on {pageName}.");
        }
    }
    catch (StaleElementReferenceException ex)
    {
        Console.WriteLine($"Stale element hit during accessibility analysis: {ex.Message}");
        // Optional: Assert.Fail("Stale element in Axe scan");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected error during accessibility test: {ex.Message}");
    }
}
 
 
public static T RetryIfStale<T>(Func<T> action, int retries = 2)
{
    for (int i = 0; i < retries; i++)
    {
        try
        {
            return action();
        }
        catch (StaleElementReferenceException)
        {
            Console.WriteLine("Retrying after stale element...");
            Thread.Sleep(500); // Let the DOM settle
        }
    }
    throw new StaleElementReferenceException("Retry failed after multiple attempts.");
}



        public bool VerifyKeyboardNavigation(By TargetLocator, int maxTabs)
        {
            Actions actions = new Actions(driver);
            bool found = false;

            for (int i = 0; i < maxTabs; i++)
            {
                actions.SendKeys(Keys.Tab).Perform();
                Thread.Sleep(1000); // Wait for 1 second to observe the change
                IWebElement activeElement = driver.SwitchTo().ActiveElement();
                if (IsElementMatch(activeElement, TargetLocator))
                {
                    Console.WriteLine($"Found the expected element after {i + 1} tabs.");
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                throw new Exception($"Expected element not found after {maxTabs} tabs.");
            }


            return found; // Return whether the element was found
        }


        public bool IsElementMatch(IWebElement activeElement, By targetLocator)
        {
            try
            {
                IWebElement targetElement = driver.FindElement(targetLocator);
                return activeElement.Equals(targetElement);
            }
            catch
            {
                return false;
            }
        }


        // Common methods and properties for all pages
        public void NavigateToUrl(string url)
        {
            driver.Navigate().GoToUrl(url);
        }

        public void WaitForElement(By locator)
        {
            wait.Until(ExpectedConditions.ElementIsVisible(locator));
        }

        public void launchBrowser()
        {
            driver.Navigate().GoToUrl("https://www.zigwheels.com"); // Navigate to the desired URL
            driver.Manage().Window.Maximize(); // Maximize the browser window
        }

        public virtual void navigateToHomePage()
        {
            var homePageImage = driver.FindElement(By.XPath("//*[@id='Header']/div/div[1]/div[1]/a/img"));
            homePageImage.Click();
        }

        public void CloseBrowser()
        {
            driver.Quit();
        }

        // Navigation functions
        public void NavigateToNews()
        {
            var newsButton = driver.FindElement(By.LinkText("NEWS & REVIEWS"));
            newsButton.Click();
        }

        public void NavigateToNewCars()
        {
            var newCarsButton = driver.FindElement(By.LinkText("NEW CARS"));
            newCarsButton.Click();
        }

        public void NavigateToNewBikes()
        {
            var newBikesButton = driver.FindElement(By.LinkText("NEW BIKES"));
            newBikesButton.Click();
        }

        public void NavigateToScooters()
        {
            var scootersButton = driver.FindElement(By.LinkText("SCOOTERS"));
            scootersButton.Click();
        }


        // Method to handle consent popup
        public void HandleConsentPopup()
        {
            try
            {
                // Try switching to iframe if the consent popup is inside one
                bool switchedToConsentFrame = false;
                var iframes = driver.FindElements(By.TagName("iframe"));

                foreach (var frame in iframes)
                {
                    try
                    {
                        driver.SwitchTo().Frame(frame);
                        if (driver.PageSource.Contains("Consent"))
                        {
                            Console.WriteLine("Switched to iframe containing consent popup.");
                            switchedToConsentFrame = true;
                            break;
                        }
                        driver.SwitchTo().DefaultContent();
                    }
                    catch (NoSuchFrameException) { /* Ignore and continue */ }
                }

                // Wait for the consent button to be visible and clickable
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                IWebElement consentBtn = wait.Until(d =>
                {
                    try
                    {
                        var element = d.FindElement(By.CssSelector("button[aria-label='Consent']"));
                        return (element.Displayed && element.Enabled) ? element : null;
                    }
                    catch (NoSuchElementException) { return null; }
                    catch (StaleElementReferenceException) { return null; }
                });

                if (consentBtn != null)
                {
                    consentBtn.Click();
                    Console.WriteLine("Consent button clicked.");
                }
                else
                {
                    Console.WriteLine("Consent button not found or already handled.");
                }

                // Always switch back to the main content
                if (switchedToConsentFrame)
                {
                    driver.SwitchTo().DefaultContent();
                    Console.WriteLine("Switched back to default content.");
                }
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("Consent button not found within the timeout period. It might have already been handled.");
            }
            catch (StaleElementReferenceException)
            {
                Console.WriteLine("Consent button became stale. It might have already been handled.");
            }
        }


        public void maximiseWindow()
        {
            driver.Manage().Window.Maximize();
        }
        public void Scroll(int offset)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript($"window.scrollBy(0, {offset});");
        }

        public void AttemptGoogleLogin(string emailOrPhoneInput)
        {
            int retries = 3;
            TimeSpan timeout = TimeSpan.FromSeconds(30); // You can extend if needed
            bool isCI = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";

            try
            {
                Console.WriteLine($"Starting Google login process. Current URL: {driver.Url}");

                if (isCI)
                {
                    Console.WriteLine("Detected CI/CD environment. Skipping real Google login to avoid failures.");
                    return; // Skip Google login if running in CI
                }

                // Step 1: Click forum login title
                for (int attempt = 0; attempt < retries; attempt++)
                {
                    try
                    {
                        IWebElement forumLoginTitle = new WebDriverWait(driver, timeout)
                            .Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id='forum_login_title_lg']")));
                        forumLoginTitle.Click();
                        Console.WriteLine("Login title element clicked successfully.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (attempt == retries - 1)
                        {
                            Console.WriteLine($"Failed to click login title after {retries} attempts. Skipping login.");
                            return;
                        }
                        Console.WriteLine($"Retrying login title click (Attempt {attempt + 1}/{retries})...");
                        Thread.Sleep(1000);
                    }
                }

                // Step 2: Click Google Sign-In button
                Thread.Sleep(2000); // Allow modal to appear
                for (int attempt = 0; attempt < retries; attempt++)
                {
                    try
                    {
                        IWebElement googleSignInButton = new WebDriverWait(driver, timeout)
                            .Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("div[data-track-label='Popup_Login/Register_with_Google']")));
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", googleSignInButton);
                        Console.WriteLine("Google Sign-In button clicked via JavaScript.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (attempt == retries - 1)
                        {
                            Console.WriteLine($"Failed to click Google Sign-In button after {retries} attempts. Skipping login.");
                            return;
                        }
                        Console.WriteLine($"Retrying Google Sign-In click (Attempt {attempt + 1}/{retries})...");
                        Thread.Sleep(1000);
                    }
                }

                // Step 3: Try to switch to new Google window
                Thread.Sleep(2000); // Give time for popup
                string mainWindow = driver.CurrentWindowHandle;
                try
                {
                    WebDriverWait windowWait = new WebDriverWait(driver, timeout);
                    windowWait.Until(d => d.WindowHandles.Count > 1);

                    foreach (var handle in driver.WindowHandles)
                    {
                        if (handle != mainWindow)
                        {
                            driver.SwitchTo().Window(handle);
                            Console.WriteLine($"Switched to new window. Current URL: {driver.Url}");
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("No new window detected. Skipping Google login.");
                    return;
                }

                // Step 4: Try to input email
                try
                {
                    IWebElement emailInput = new WebDriverWait(driver, timeout)
                        .Until(ExpectedConditions.ElementIsVisible(By.Id("identifierId")));
                    emailInput.Clear();
                    emailInput.SendKeys(emailOrPhoneInput);
                    Console.WriteLine("Entered email/phone successfully.");

                    IWebElement nextButton = driver.FindElement(By.XPath("//*[@id=\"identifierNext\"]/div/button/span"));
                    nextButton.Click();
                    Console.WriteLine("Clicked Next button successfully.");
                }
                catch (Exception)
                {
                    Console.WriteLine("Google login fields not found. Skipping login.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled error during Google login: {ex.Message}. Skipping login.");
            }
        }
        

        public void navigateToUsedCars()
        {
            var dropDownMenu = driver.FindElement(By.LinkText("Used Cars"));
            dropDownMenu.Click();
        }
        public void HoverAndClickMenu()
        {
            // Locate the menu element
            var menuElement = driver.FindElement(By.CssSelector("span.c-p.icon-down-arrow"));

            // Hover over the menu element
            actions.MoveToElement(menuElement).Perform();

            // Wait for the submenu to be visible
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("ul.txt-l")));

            // Click on the submenu element
            var submenuElement = driver.FindElement(By.CssSelector("ul.txt-l"));
            submenuElement.Click();
        }
        public void HoverOverMenu()
        {
            // Locate the menu element
            var menuElement = driver.FindElement(By.CssSelector("span.c-p.icon-down-arrow"));

            // Hover over the menu element
            actions.MoveToElement(menuElement).Perform();

            // Wait for the submenu to be visible
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("ul.txt-l")));
        }
        public void ClickAutoExpo()
        {
            HoverOverMenu();
            var autoExpoLink = driver.FindElement(By.CssSelector("a[href='/autoexpo']"));
            autoExpoLink.Click();
        }

        public void NavigateToAutoExpo()
        {
            HoverOverMenu();
            var autoExpoLink = driver.FindElement(By.CssSelector("a[href='/autoexpo']"));
            autoExpoLink.Click();
        }

        public void NavigateToElectricVehicles()
        {
            HoverOverMenu();
            var electricVehiclesLink = driver.FindElement(By.CssSelector("a[href='/electric-vehicles']"));
            electricVehiclesLink.Click();
        }

        public void NavigateToUsedCars()
        {
            HoverOverMenu();
            var usedCarsLink = driver.FindElement(By.CssSelector("a[href='/used-car']"));
            usedCarsLink.Click();
        }

        public void NavigateToForum()
        {
            HoverOverMenu();
            var forumLink = driver.FindElement(By.CssSelector("a[href='/community']"));
            forumLink.Click();
        }

        public void ClickUsedCars()
        {
            HoverOverMenu();
            var usedCarsLink = driver.FindElement(By.CssSelector("a[href='/used-car']"));
            usedCarsLink.Click();
        }

        public void ClickForum()
        {
            HoverOverMenu();
            var forumLink = driver.FindElement(By.CssSelector("a[href='/community']"));
            forumLink.Click();
        }
        public void DismissNotificationPopUp()
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                IWebElement notificationPopUp = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[aria-label='Block']")));
                notificationPopUp.Click();
                Console.WriteLine("Notification pop-up dismissed.");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("No notification pop-up found.");
            }
        }
    }
    
    
}
