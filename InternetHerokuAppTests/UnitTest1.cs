using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace InternetHerokuAppTests;

public class UnitTest1 : IDisposable
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;
    private const string BaseUrl = "https://the-internet.herokuapp.com";

    public UnitTest1()
    {
        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");

        _driver = new ChromeDriver(options);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
    }


    [Fact]
    public void Test_01_AddRemoveElements()
    {
        _driver.Navigate().GoToUrl($"{BaseUrl}/add_remove_elements/");

        var addButton = _driver.FindElement(By.XPath("//button[text()='Add Element']"));
        addButton.Click();
        addButton.Click();

        var deleteButtons = _driver.FindElements(By.ClassName("added-manually"));
        Assert.Equal(2, deleteButtons.Count);

        deleteButtons[0].Click();
        deleteButtons = _driver.FindElements(By.ClassName("added-manually"));
        Assert.Single(deleteButtons);

        System.Threading.Thread.Sleep(5000);

    }


    [Fact]
    public void Test_02_Checkboxes()
    {
        _driver.Navigate().GoToUrl($"{BaseUrl}/checkboxes");

        var checkboxes = _driver.FindElements(By.CssSelector("#checkboxes input[type='checkbox']"));
        var checkbox1 = checkboxes[0];
        var checkbox2 = checkboxes[1];

        Assert.False(checkbox1.Selected);
        Assert.True(checkbox2.Selected);

        checkbox1.Click();
        checkbox2.Click();

        Assert.True(checkbox1.Selected);
        Assert.False(checkbox2.Selected);

        System.Threading.Thread.Sleep(5000);
    }


    [Fact]
    public void Test_03_Dropdown()
    {
        _driver.Navigate().GoToUrl($"{BaseUrl}/dropdown");

        var dropdownElement = _driver.FindElement(By.Id("dropdown"));
        var select = new SelectElement(dropdownElement);

        select.SelectByText("Option 1");
        Assert.Equal("Option 1", select.SelectedOption.Text);

        select.SelectByValue("2");
        Assert.Equal("Option 2", select.SelectedOption.Text);

        System.Threading.Thread.Sleep(5000);
    }


    [Fact]
    public void Test_04_Inputs()
    {
        _driver.Navigate().GoToUrl($"{BaseUrl}/inputs");

        var inputField = _wait.Until(d => d.FindElement(By.CssSelector(".example input")));
        inputField.SendKeys("42");
        Assert.Equal("42", inputField.GetAttribute("value"));

        inputField.SendKeys(Keys.ArrowUp);
        Assert.Equal("43", inputField.GetAttribute("value"));

        System.Threading.Thread.Sleep(5000);
    }


    [Fact]
    public void Test_05_StatusCodes()
    {
        _driver.Navigate().GoToUrl($"{BaseUrl}/status_codes");

        var link200 = _driver.FindElement(By.LinkText("200"));
        link200.Click();

        var content = _wait.Until(d => d.FindElement(By.CssSelector(".example p"))).Text;
        Assert.Contains("200 status code", content);

        System.Threading.Thread.Sleep(5000);
    }


    [Fact]
    public void Test_06_DynamicControls()
    {
        _driver.Navigate().GoToUrl($"{BaseUrl}/dynamic_controls");


        var removeButton = _driver.FindElement(By.XPath("//form[@id='checkbox-example']/button"));
        removeButton.Click();

        var message = _wait.Until(d => d.FindElement(By.Id("message")));
        Assert.Equal("It's gone!", message.Text);

        var enableButton = _driver.FindElement(By.XPath("//form[@id='input-example']/button"));
        enableButton.Click();

        var input = _wait.Until(d =>
        {
            var el = d.FindElement(By.XPath("//form[@id='input-example']/input"));
            return el.Enabled ? el : null;
        });
        Assert.NotNull(input);

        System.Threading.Thread.Sleep(5000);
    }


    [Fact]
    public void Test_07_FileUpload()
    {
        _driver.Navigate().GoToUrl($"{BaseUrl}/upload");

        string tempFilePath = Path.Combine(Path.GetTempPath(), "test_upload.txt");
        File.WriteAllText(tempFilePath, "Selenium File Upload Test");

        try
        {
            var fileUploadInput = _driver.FindElement(By.Id("file-upload"));
            fileUploadInput.SendKeys(tempFilePath);

            _driver.FindElement(By.Id("file-submit")).Click();


            var uploadedFiles = _wait.Until(d => d.FindElement(By.Id("uploaded-files")));
            Assert.Contains("test_upload.txt", uploadedFiles.Text.Trim());
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }

        System.Threading.Thread.Sleep(5000);
    }


    [Fact]
    public void Test_08_JavaScriptAlerts()
    {
        _driver.Navigate().GoToUrl($"{BaseUrl}/javascript_alerts");


        _driver.FindElement(By.XPath("//button[text()='Click for JS Alert']")).Click();
        var alert = _driver.SwitchTo().Alert();
        Assert.Equal("I am a JS Alert", alert.Text);
        alert.Accept();

        var result = _driver.FindElement(By.Id("result"));
        Assert.Equal("You successfully clicked an alert", result.Text);


        _driver.FindElement(By.XPath("//button[text()='Click for JS Confirm']")).Click();
        var confirmAlert = _driver.SwitchTo().Alert();
        confirmAlert.Dismiss();
        Assert.Equal("You clicked: Cancel", result.Text);


        _driver.FindElement(By.XPath("//button[text()='Click for JS Prompt']")).Click();
        var promptAlert = _driver.SwitchTo().Alert();
        promptAlert.SendKeys("Automated Test");
        promptAlert.Accept();
        Assert.Equal("You entered: Automated Test", result.Text);

        System.Threading.Thread.Sleep(5000);
    }


    [Fact]
    public void Test_09_MultipleWindows()
    {
        _driver.Navigate().GoToUrl($"{BaseUrl}/windows");

        string originalWindow = _driver.CurrentWindowHandle;
        _driver.FindElement(By.LinkText("Click Here")).Click();

        _wait.Until(d => d.WindowHandles.Count > 1);

        foreach (var handle in _driver.WindowHandles)
        {
            if (handle != originalWindow)
            {
                _driver.SwitchTo().Window(handle);
                break;
            }
        }

        var heading = _driver.FindElement(By.TagName("h3"));
        Assert.Equal("New Window", heading.Text);

        _driver.Close();
        _driver.SwitchTo().Window(originalWindow);
        Assert.Equal("Opening a new window", _driver.FindElement(By.TagName("h3")).Text);

        System.Threading.Thread.Sleep(5000);
    }


    [Fact]
    public void Test_10_NotificationMessage()
    {
        _driver.Navigate().GoToUrl($"{BaseUrl}/notification_message_rendered");

        var triggerLink = _driver.FindElement(By.CssSelector(".example a"));
        triggerLink.Click();

        var flash = _wait.Until(d => d.FindElement(By.Id("flash")));
        string flashText = flash.Text;

        Assert.Contains("Action", flashText);

        System.Threading.Thread.Sleep(5000);
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}