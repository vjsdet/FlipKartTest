# Selenium Test Automation Framework

## Overview
This repository contains a Selenium test automation framework written in C# using NUnit. The framework is designed to automate web application tests and supports running tests from Visual Studio Test Explorer as well as from the command line.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Setup Instructions](#setup-instructions)
  - [Visual Studio Setup](#visual-studio-setup)
  - [Command Line Setup](#command-line-setup)
- [Running Tests](#running-tests)
  - [Using Test Explorer](#using-test-explorer)
  - [From Command Line](#from-command-line)
- [Project Structure](#project-structure)
- [Troubleshooting](#troubleshooting)

## Prerequisites
Before you start, make sure you have the following installed:
- **.NET SDK** - For running .NET applications and tests.
- **Visual Studio** - For development and running tests.
- **Google Chrome** or another supported browser.
- **Selenium WebDriver** - For browser automation.
- **NUnit Test Adapter** - For running NUnit tests in Visual Studio.

## Setup Instructions

### Visual Studio Setup

1. **Clone the Repository**
   - Open a command prompt or terminal and clone the repository:
     ```bash
     git clone https://github.com/your-repo/your-project.git
     cd your-project
     ```

2. **Open the Project**
   - Open the `.sln` file in Visual Studio.

3. **Restore NuGet Packages**
   - In Visual Studio, restore the required NuGet packages by right-clicking on the solution and selecting `Restore NuGet Packages`.

4. **Install Required Extensions**
   - Go to `Extensions > Manage Extensions > Online`, search for `NUnit Test Adapter`, and install it if it�s not already installed.

### Command Line Setup

1. **Install .NET SDK**
   - Ensure the .NET SDK is installed. You can download it from [here](https://dotnet.microsoft.com/download).

2. **Restore NuGet Packages**
   - Navigate to the project directory and restore the NuGet packages:
     ```bash
     dotnet restore
     ```

3. **Build the Project**
   - Build the project to ensure everything is set up correctly:
     ```bash
     dotnet build
     ```

## Running Tests

### Using Test Explorer

1. **Open Test Explorer**
   - In Visual Studio, go to `Test > Test Explorer` to open the Test Explorer window.

2. **Build the Solution**
   - Build your solution to discover the tests by selecting `Build > Build Solution`.

3. **Run Tests**
   - In the Test Explorer window, you will see a list of discovered tests. 
   - Click `Run All` to execute all tests, or select individual tests and click `Run`.

### From Command Line

1. **Navigate to Project Directory**
   - Open a command prompt or terminal and navigate to the directory containing the `.csproj` file:
     ```bash
     cd path/to/your/project
     ```

2. **Run Tests**
   - Use the following command to run tests using the .NET CLI:
     ```bash
     dotnet test
     ```
   - This command will build the project (if necessary) and execute the tests, displaying results in the console.

## Project Structure
- `/Pages`: Contains page object classes for Selenium.
- `/Tests`: Contains test classes and methods.
- `/Utils`: Utility classes, such as for logging or handling test data.
- `/Data`: Contains test data and configuration files.
- `BasePage.cs`: Base class for page objects with common functionality.
- `HomePage.cs`: Example page object class.
- `Tests.cs`: Example test class demonstrating test cases.

## Troubleshooting

- **Driver Issues**: Ensure that the correct WebDriver executable is available in your system's PATH or specify its path in the configuration.
- **Test Failures**: Check the test logs for detailed error messages. Review the test cases and application state for potential issues.
- **NuGet Package Problems**: Make sure all required packages are restored. Use `Tools > NuGet Package Manager > Package Manager Console` to manually restore packages if needed.
