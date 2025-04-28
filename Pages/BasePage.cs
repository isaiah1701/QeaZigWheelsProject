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
            TimeSpan timeout = TimeSpan.FromSeconds(60); // Longer timeout for CI
            WebDriverWait wait = new WebDriverWait(driver, timeout);

            string mainWindow = driver.CurrentWindowHandle;
            int originalWindowCount = driver.WindowHandles.Count;

            try
            {
                Console.WriteLine($"Starting Google login process. Current URL: {driver.Url}");

                // Step 1: Click forum login title
                Retry(() =>
                {
                    var forumLoginTitle = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id='forum_login_title_lg']")));
                    Console.WriteLine($"Forum login title visible: {forumLoginTitle.Displayed}, enabled: {forumLoginTitle.Enabled}");
                    forumLoginTitle.Click();
                    Console.WriteLine("Login title element clicked.");
                }, retries);

                // Step 2: Click Google Sign-In button
                Retry(() =>
                {
                    Thread.Sleep(2000); // Give modal time
                    var googleSignInButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("div[data-track-label='Popup_Login/Register_with_Google']")));
                    Console.WriteLine($"Google Sign-In button visible: {googleSignInButton.Displayed}, enabled: {googleSignInButton.Enabled}");
                    googleSignInButton.Click();
                    Console.WriteLine("Google Sign-In button clicked.");
                }, retries);

                // Step 3: Switch to new window
                Retry(() =>
                {
                    Console.WriteLine($"Waiting for new window... Existing windows: {driver.WindowHandles.Count}");

                    // Wait for new window with longer timeout
                    wait.Until(driver => driver.WindowHandles.Count > originalWindowCount);

                    // Get all window handles and switch to the new one
                    var handles = driver.WindowHandles.ToList();
                    Console.WriteLine($"Window handles found: {handles.Count}");

                    foreach (var handle in handles)
                    {
                        if (handle != mainWindow)
                        {
                            driver.SwitchTo().Window(handle);
                            Console.WriteLine($"Switched to window. URL: {driver.Url}, Title: {driver.Title}");
                            break;
                        }
                    }

                    // Wait for the page to load completely
                    ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete");

                }, retries);

                // Step 4: Improved iframe handling
                Retry(() =>
                {
                    Console.WriteLine($"Looking for iframes on page: {driver.Url}");

                    // Wait for the DOM to be fully loaded
                    ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete");

                    // Check how many iframes are present
                    var iframes = driver.FindElements(By.TagName("iframe"));
                    Console.WriteLine($"Found {iframes.Count} iframes on the page");

                    bool frameFound = false;

                    // Try to switch to default content first to ensure we're at the top level
                    driver.SwitchTo().DefaultContent();

                    // First try for common Google iframe IDs or name attributes
                    string[] googleFrameIdentifiers = new[] {
                        "gsi_427162_788768", // Example Google iframe ID
                        "signin-frame",
                        "googleIdentityFrame",
                        "gsi_427162_515810"
                    };

                    // First attempt: Try direct iframe identifiers
                    foreach (var id in googleFrameIdentifiers)
                    {
                        try
                        {
                            Console.WriteLine($"Trying to find iframe with ID/name: {id}");
                            driver.SwitchTo().Frame(id);
                            Console.WriteLine($"Successfully switched to iframe: {id}");
                            frameFound = true;
                            break;
                        }
                        catch (NoSuchFrameException)
                        {
                            Console.WriteLine($"No iframe with ID/name: {id} found.");
                            // Continue to the next ID
                            driver.SwitchTo().DefaultContent();
                        }
                    }

                    // Second attempt: if no specific iframes found, try all iframes on page
                    if (!frameFound && iframes.Count > 0)
                    {
                        for (int i = 0; i < iframes.Count; i++)
                        {
                            try
                            {
                                driver.SwitchTo().Frame(i);
                                Console.WriteLine($"Switched to iframe index: {i}");

                                // Check if this iframe has the Gmail login elements
                                bool hasEmailField = driver.FindElements(By.Id("identifierId")).Count > 0;

                                if (hasEmailField)
                                {
                                    Console.WriteLine("Found iframe with email field!");
                                    frameFound = true;
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("This iframe does not contain the email field.");
                                    driver.SwitchTo().DefaultContent();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error switching to iframe {i}: {ex.Message}");
                                driver.SwitchTo().DefaultContent();
                            }
                        }
                    }

                    // If we couldn't find the right iframe, try without iframe switching
                    if (!frameFound)
                    {
                        Console.WriteLine("Could not find appropriate iframe. Attempting login without iframe switching.");
                        driver.SwitchTo().DefaultContent();
                    }

                }, retries);

                // Step 5: Try to find the email input element regardless of iframe
                Retry(() =>
                {
                    // Try different strategies to locate the email input
                    IWebElement emailInput = null;

                    // Strategy 1: Direct ID
                    try
                    {
                        emailInput = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("identifierId")));
                        Console.WriteLine("Found email input by ID.");
                    }
                    catch (WebDriverTimeoutException)
                    {
                        // Strategy 2: Try by name
                        try
                        {
                            emailInput = wait.Until(ExpectedConditions.ElementIsVisible(By.Name("identifier")));
                            Console.WriteLine("Found email input by name.");
                        }
                        catch (WebDriverTimeoutException)
                        {
                            // Strategy 3: Try by XPath
                            try
                            {
                                emailInput = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//input[@type='email']")));
                                Console.WriteLine("Found email input by XPath.");
                            }
                            catch (WebDriverTimeoutException)
                            {
                                // Throw an informative exception
                                throw new Exception("Could not locate the email input field using multiple strategies");
                            }
                        }
                    }

                    // Once we have the email input, interact with it
                    emailInput.Clear();
                    emailInput.SendKeys(emailOrPhoneInput);
                    Console.WriteLine("Email entered.");

                    // Strategy for finding the Next button
                    IWebElement nextButton = null;

                    try
                    {
                        nextButton = driver.FindElement(By.XPath("//*[@id=\"identifierNext\"]/div/button/span"));
                        Console.WriteLine("Found Next button by XPath.");
                    }
                    catch (NoSuchElementException)
                    {
                        try
                        {
                            nextButton = driver.FindElement(By.CssSelector("[id='identifierNext'] button"));
                            Console.WriteLine("Found Next button by CSS selector.");
                        }
                        catch (NoSuchElementException)
                        {
                            try
                            {
                                nextButton = driver.FindElement(By.XPath("//button[contains(.,'Next')]"));
                                Console.WriteLine("Found Next button by text content.");
                            }
                            catch (NoSuchElementException)
                            {
                                throw new Exception("Could not locate the Next button using multiple strategies");
                            }
                        }
                    }

                    nextButton.Click();
                    Console.WriteLine("Next button clicked.");

                }, retries);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR during Google login: {ex.Message}");
                Console.WriteLine($"Current URL at failure: {driver.Url}");
                Console.WriteLine($"Page Title: {driver.Title}");
                Console.WriteLine($"Window Handles: {string.Join(", ", driver.WindowHandles)}");

                // Try to log the page source for debugging
                try
                {
                    Console.WriteLine($"Page source length: {driver.PageSource?.Length ?? 0} characters");
                }
                catch { }

                throw;
            }
        }

        // Helper to retry flaky actions
        private void Retry(Action action, int retries)
        {
            for (int attempt = 0; attempt < retries; attempt++)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    if (attempt == retries - 1)
                        throw;

                    Console.WriteLine($"Retrying action... attempt {attempt + 1}/{retries}. Error: {ex.Message}");
                    Thread.Sleep(2000); // Wait a bit before retry
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
