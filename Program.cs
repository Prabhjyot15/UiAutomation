using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;

namespace UIAutomationDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Start Slack
            var process = Process.Start("C:\\Users\\Prabhjyot\\AppData\\Local\\slack\\slack.exe");
            process.WaitForInputIdle();

            // Wait for Slack to fully load
            await Task.Delay(10000); // Increase this delay as needed

            using (var automation = new UIA3Automation())
            {
                // Asynchronous call to find Slack window
                var slackWindow = await Task.Run(() => automation.GetDesktop().FindFirstDescendant(cf => cf.ByName("Slack"))?.AsWindow());

                if (slackWindow != null)
                {
                    Console.WriteLine("Slack window found.");

                    // Print the UI tree
                    var uiTree = slackWindow.ToTreeString(int.MaxValue); // Use a large number to ensure no elements are missed
                    Console.WriteLine("UI Tree:");
                    Console.WriteLine(uiTree);
                }
                else
                {
                    Console.WriteLine("Slack window not found.");
                }
            }

            // Close Slack after a delay
            await Task.Delay(5000);
            process.Kill();
        }
    }

    public static class AutomationElementExtensions
    {
        public static string ToTreeString(this AutomationElement element, int maxDepth, int currentDepth = 0)
        {
            if (element == null) return string.Empty;
            if (currentDepth > maxDepth) return string.Empty;

            var indent = new string(' ', currentDepth * 2);
            var info = $"{indent}Element";

            // Collect basic properties of the element
            info += $": ControlType: {element.ControlType.ToString()}";
            info += $", Name: {element.Name ?? "Not Available"}";

            try
            {
                info += $", AutomationId: {element.AutomationId ?? "Not Available"}";
            }
            catch (FlaUI.Core.Exceptions.PropertyNotSupportedException)
            {
                info += ", AutomationId: Not Supported";
            }

            try
            {
                info += $", ClassName: {element.ClassName ?? "Not Available"}";
            }
            catch (FlaUI.Core.Exceptions.PropertyNotSupportedException)
            {
                info += ", ClassName: Not Supported";
            }

            try
            {
                info += $", IsEnabled: {element.IsEnabled}";
            }
            catch (FlaUI.Core.Exceptions.PropertyNotSupportedException)
            {
                info += ", IsEnabled: Not Supported";
            }

            // Add Path info
            try
            {
                info += $", Path: {GetElementPath(element)}";
            }
            catch (Exception ex)
            {
                info += $", Path: Error - {ex.Message}";
            }

            // Add children info
            var childrenInfo = string.Empty;
            var children = element.FindAllChildren();
            foreach (var child in children)
            {
                childrenInfo += child.ToTreeString(maxDepth, currentDepth + 1);
            }

            return $"{info}\n{childrenInfo}";
        }

        private static string GetElementPath(AutomationElement element)
        {
            var path = string.Empty;
            var currentElement = element;

            while (currentElement != null)
            {
                var controlType = currentElement.ControlType.ToString();
                var name = currentElement.Name;

                // Build the path string
                path = $"{controlType} -> {path}";

                // Move to the parent element
                currentElement = currentElement.Parent;
            }

            return path.TrimEnd(" -> ".ToCharArray());
        }
    }
}
