# QeaZigWheelsProject

This is a Quality Engineering and Assurance (QEA) test automation project for the ZigWheels website. The framework is built using C#, SpecFlow (BDD), Selenium WebDriver, and follows the Page Object Model (POM) design pattern.

## 🔍 Project Scope

This project automates and validates key user workflows and accessibility features on ZigWheels.com. It includes:

1. **Google Sign-In Validation** – Tests error handling and validation during login via Google OAuth.  
2. **Bike Search: Honda Cruiser** – Automates navigation to Honda Cruiser bikes and verifies data integrity and UI flow.  
3. **Car Data Scraping** – Extracts structured information (e.g., car names and prices) for selected categories and logs the output for verification.  
4. **Accessibility Testing** – Ensures web elements are compatible with screen readers and keyboard navigation, confirming WCAG alignment.

## 🧱 Tech Stack

- **Language**: C#  
- **Framework**: SpecFlow (BDD)  
- **Browser Automation**: Selenium WebDriver  
- **Test Design Pattern**: Page Object Model (POM)  
- **Accessibility Tools**: Axe (or manual keyboard navigation)  
- **Reporting**: ExtentReports  
- **Performance (optional)**: JMeter  

## 📁 Structure

QeaZigWheelsProject/
│
├── Features/ # BDD Feature files
│ ├── GoogleLogin.feature
│ ├── HondaCruiserSearch.feature
│ ├── CarScraper.feature
│ └── AccessibilityCheck.feature
│
├── Pages/ # Page Object Model implementations
├── StepDefinitions/ # Step bindings for SpecFlow
├── Drivers/ # WebDriver initialization logic
└── Reports/ # Test report output (ExtentReports)



## ✅ How to Run

1. Clone the repo:  
   `git clone https://github.com/isaiah1701/QeaZigWheelsProject.git`  
2. Open the solution in Visual Studio.  
3. Restore NuGet packages.  
4. Update browser drivers if needed.  
5. Run tests via Test Explorer or use:  
   `dotnet test`

## 📌 Notes

- Reports are generated using **ExtentReports**.  
- Modular and scalable structure for future test case additions.  
- Designed with cross-browser support in mind.

---

Feel free to fork and adapt this project for your own UI automation and accessibility testing needs.
