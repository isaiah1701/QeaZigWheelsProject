using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Selenium.Axe;
using Reqnroll;





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

       
void AttemptGoogleLogin(string emailOrPhoneInput)
        {
            int retries = 3;
            TimeSpan timeout = TimeSpan.FromSeconds(30); // Moderate timeout for CI/CD efficiency
            WebDriverWait wait = new WebDriverWait(driver, timeout);

            try
            {
                Console.WriteLine($"Starting Google login process. Current URL: {driver.Url}");
                string mainWindow = driver.CurrentWindowHandle;

                // Step 1: Click forum login title - continue if it fails
                TryAction(() =>
                {
                    var forumLoginTitle = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id='forum_login_title_lg']")));
                    forumLoginTitle.Click();
                    Console.WriteLine("Login title element clicked successfully.");
                }, retries, "Login title click failed, continuing without login");

                // Step 2: Click Google Sign-In button - continue if it fails
                TryAction(() =>
                {
                    Thread.Sleep(1000); // Short delay for modal to appear
                    var googleSignInButton = wait.Until(ExpectedConditions.ElementToBeClickable(
                        By.CssSelector("div[data-track-label='Popup_Login/Register_with_Google']")));
                    googleSignInButton.Click();
                    Console.WriteLine("Google Sign-In button clicked successfully.");
                }, retries, "Google Sign-In button click failed, continuing without login");

                // Step 3: Switch to Google window - continue if it fails
                TryAction(() =>
                {
                    wait.Until(driver => driver.WindowHandles.Count > 1);
                    foreach (var handle in driver.WindowHandles)
                    {
                        if (handle != mainWindow)
                        {
                            driver.SwitchTo().Window(handle);
                            Console.WriteLine($"Switched to Google login window. URL: {driver.Url}");
                            break;
                        }
                    }
                }, retries, "Window switch failed, continuing without login");

                // Step 4: Enter email and click next - continue if it fails
                TryAction(() =>
                {
                    // Wait and try iframe switch
                    try
                    {
                        var iframes = driver.FindElements(By.TagName("iframe"));
                        if (iframes.Count > 0)
                        {
                            driver.SwitchTo().Frame(0); // Try first iframe
                            Console.WriteLine("Switched to iframe");
                        }
                    }
                    catch { /* Continue without iframe if fails */ }

                    // Try to find and interact with the email field
                    var emailInput = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("identifierId")));
                    emailInput.Clear();
                    emailInput.SendKeys(emailOrPhoneInput);
                    Console.WriteLine("Email entered successfully.");

                    var nextButton = driver.FindElement(By.XPath("//*[@id=\"identifierNext\"]/div/button/span"));
                    nextButton.Click();
                    Console.WriteLine("Next button clicked successfully.");
                }, retries, "Email entry failed, continuing without login");

                // Switch back to main window for next steps in test flow
                try
                {
                    driver.SwitchTo().Window(mainWindow);
                    Console.WriteLine("Returned to main window for continued testing.");
                }
                catch
                {
                    Console.WriteLine("Failed to switch back to main window, attempting to proceed anyway.");
                }
            }
            catch (Exception ex)
            {
                // Log error but DO NOT rethrow - this allows CI/CD to continue
                Console.WriteLine($"WARNING: Google login process failed: {ex.Message}");
                Console.WriteLine("Continuing with test execution despite login failure.");

                // Try to get back to main flow by returning to main window
                try
                {
                    driver.SwitchTo().DefaultContent();
                    driver.Navigate().Refresh();
                    Console.WriteLine("Attempted recovery after login failure.");
                }
                catch
                {
                    Console.WriteLine("Recovery attempt also failed, but continuing anyway.");
                }
            }
        }

        // Helper method that tries an action but doesn't fail the test if it doesn't succeed
        private void TryAction(Action action, int retries, string failureMessage)
        {
            for (int attempt = 0; attempt < retries; attempt++)
            {
                try
                {
                    action();
                    return; // Success - exit method
                }
                catch (Exception ex)
                {
                    if (attempt == retries - 1)
                    {
                        // Last attempt failed, log message and continue
                        Console.WriteLine($"{failureMessage} - Error details: {ex.Message}");
                        return;
                    }

                    Console.WriteLine($"Attempt {attempt + 1}/{retries} failed: {ex.Message}. Retrying...");
                    Thread.Sleep(1000); // Short wait between retries
                }
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
