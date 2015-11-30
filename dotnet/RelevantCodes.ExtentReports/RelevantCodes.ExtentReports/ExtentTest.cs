﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RelevantCodes.ExtentReports.Model;
using RelevantCodes.ExtentReports.View;

namespace RelevantCodes.ExtentReports
{
    /// <summary>
    /// Defines a node in the report file
    /// 
    /// By default, each started node is top-level. If <code>appendChild</code> method
    /// is used on any test, it automatically becomes a child-node. When this happens:
    /// 
    /// <list type="bullet">
    ///     <item>
    ///         <description>parent test -> <code>hasChildNodes = true</description>
    ///     </item>
    ///     <item>
    ///         <description>child test -> <code>isChildNode = true</description>
    ///     </item>
    /// </list>
    /// </summary>
    public class ExtentTest
    {
        private LogStatus _testStatus = LogStatus.Unknown;
        private Test _test;

        /// <summary>
        /// Creates a test node as a top-most level test
        /// </summary>
        /// 
        /// <param name="Name">Test name</param>
        /// <param name="Description">A short description of the test</param>
        public ExtentTest(string Name, string Description)
        {
            _test = new Test();

            _test.Name = Name;
            _test.Description = Description;
        }

        /// <summary>
        /// Logs events for the test
        /// 
        /// Log event is shown in the report with 4 columns:
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <description>Timestamp</description>
        ///     </item>
        ///     <item>
        ///         <description>Status</description>
        ///     </item>
        ///     <item>
        ///         <description>StepName</description>
        ///     </item>
        ///     <item>
        ///         <description>Details</description>
        ///     </item>
        /// </list>
        /// </summary>
        /// 
        /// <param name="Status">Status</param>
        /// <param name="StepName">Name of the step</param>
        /// <param name="Details">Details of the step</param>
        public void Log(LogStatus Status, string StepName, string Details)
        {
            var log = new Log();

            log.Timestamp = DateTime.Now; 
            log.LogStatus = Status;
            log.StepName = StepName;
            log.Details = Details;

            _test.LogList.Add(log);

            _test.TrackLastRunStatus();
            _testStatus = _test.Status;
        }

        /// <summary>
        /// Logs events for the test
        /// 
        /// Log event is shown in the report with 3 columns:
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <description>Timestamp</description>
        ///     </item>
        ///     <item>
        ///         <description>Status</description>
        ///     </item>
        ///     <item>
        ///         <description>Details</description>
        ///     </item>
        /// </list>
        /// </summary>
        /// 
        /// <param name="Status">Status</param>
        /// <param name="Details">Details of the step</param>
        public void Log(LogStatus Status, string Details)
        {
            Log(Status, null, Details);
        }

        /// <summary>
        /// Logs events for the test
        /// 
        /// Log event is shown in the report with 3 columns:
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <description>Timestamp</description>
        ///     </item>
        ///     <item>
        ///         <description>Status</description>
        ///     </item>
        ///     <item>
        ///         <description>Details of exception</description>
        ///     </item>
        /// </list>
        /// </summary>
        /// 
        /// <param name="Status">Status</param>
        /// <param name="StepName">Name of the step</param>
        /// <param name="Exception">Exception</param>
        public void Log(LogStatus Status, Exception Exception)
        {
            Log(Status, null, Exception);
        }

        /// <summary>
        /// Logs events for the test
        /// 
        /// Log event is shown in the report with 4 columns:
        /// 
        /// <list type="bullet">
        ///     <item>
        ///         <description>Timestamp</description>
        ///     </item>
        ///     <item>
        ///         <description>Status</description>
        ///     </item>
        ///     <item>
        ///         <description>StepName</description>
        ///     </item>
        ///     <item>
        ///         <description>Details of exception</description>
        ///     </item>
        /// </list>
        /// </summary>
        /// 
        /// <param name="Status">Status</param>
        /// <param name="StepName">Name of the step</param>
        /// <param name="Exception">Exception</param>
        public void Log(LogStatus Status, string StepName, Exception Exception)
        {
            string details = string.Format("<pre>{0}</pre>", Exception.ToString());

            Log(Status, StepName, details);
        }

        /// <summary>
        /// Adds a snapshot to the log event details
        /// 
        /// Note: this method does not attach the screen-cast to the report, it only
        /// links to the path
        /// </summary>
        /// 
        /// <param name="ImagePath">Path of the image</param>
        /// <returns>A formed HTML img tag with the supplied path</returns>
        public string AddScreenCapture(string ImagePath)
        {
            string screenCaptureHtml;

            if (IsPathRelative(ImagePath))
            {
                screenCaptureHtml = ScreenCaptureHtml.GetSource(ImagePath).Replace("file:///", "");
            }
            else
            {
                screenCaptureHtml = ScreenCaptureHtml.GetSource(ImagePath);
            }

            var img = new ScreenCapture();
            img.Source = screenCaptureHtml;
            img.TestName = _test.Name;
            img.TestID = _test.ID;

            _test.ScreenCapture.Add(img);

            return screenCaptureHtml;
        }

        /// <summary>
        /// Adds a screen cast to the log event details
        /// 
        /// Note: this method does not attach the screen-cast to the report, it only
        /// links to the path
        /// </summary>
        /// 
        /// <param name="ScreencastPath">Path of the screencast</param>
        /// <returns>A formed HTML video tag with the supplied path</returns>
        public string AddScreencast(string ScreencastPath)
        {
            if (IsPathRelative(ScreencastPath))
            {
                ScreencastPath = ScreencastHtml.GetSource(ScreencastPath).Replace("file:///", "");
            }
            else
            {
                ScreencastPath = ScreencastHtml.GetSource(ScreencastPath);
            }

            var sc = new Screencast();
            sc.Source = ScreencastPath;
            sc.TestName = _test.Name;
            sc.TestID = _test.ID;

            _test.Screencast.Add(sc);

            return ScreencastPath;
        }

        /// <summary>
        /// Determines if the path is relative or absolute
        /// </summary>
        /// 
        /// <param name="FilePath">Path of the file</param>
        /// <returns>bool</returns>
        private Boolean IsPathRelative(string FilePath)
        {
            if (FilePath.StartsWith("http") || !Path.IsPathRooted(FilePath))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Assigns category to test
        /// 
        /// Usage: <code>test.assignCategory("ExtentAPI");</code>
        /// Usage: <code>test.assignCategory("ExtentAPI", "Regression", ...);</code>
        /// </summary>
        /// 
        /// <param name="Category">Category name</param>
        /// <returns>An ExtentTest object</returns>
        public ExtentTest AssignCategory(params string[] Category)
        {
            Category.ToList().ForEach(c =>
            {
                _test.AddCategory(c);
            });

            return this;
        }

        /// <summary>
        /// Assigns author(s) to test
        /// 
        /// Usage: <code>test.assignAuthor("AuthorName");</code>
        /// Usage: <code>test.assignAuthor("Author1", "Author2", ...);</code>
        /// </summary>
        /// 
        /// <param name="Author">Author name</param>
        /// <returns>An ExtentTest object</returns>
        public ExtentTest AssignAuthor(params string[] Author)
        {
            Author.ToList().ForEach(a =>
            {
                _test.AddAuthor(a);
            });

            return this;
        }

        /// <summary>
        /// Appends a child test to the current test
        /// </summary>
        /// 
        /// <param name="Node">An <code>ExtentTest</code> object. Test that is added as the node</param>
        /// <returns>An <code>ExtentTest</code> object, which is the test that adds the child node</returns>
        public ExtentTest AppendChild(ExtentTest Node)
        {
            var node = Node.GetTest();

            node.ChildNode = true;
            node.Ended = true;

            node.TrackLastRunStatus();

            node.CategoryList.ForEach(c => AssignCategory(c.Name));
            node.AuthorList.ForEach(a => AssignAuthor(a.Name));

            _test.NodeList.Add(node);
            _test.ContainsChildNodes = true;

            return this;
        }

        /// <summary>
        /// Provides the current run status of the test
        /// </summary>
        /// 
        /// <returns>LogStatus</returns>
        public LogStatus GetCurrentStatus()
        {
            return _testStatus;
        }

        /// <summary>
        /// Returns the underlying test which controls the internal model
        /// 
        /// This allows manipulating the test instance by accessing the internal methods 
        /// and properties of the test
        /// </summary>
        /// 
        /// <returns>A <code>ExtentReports.Model.Test</code> object</returns>
        public Test GetTest()
        {
            return _test;
        }
    }
}