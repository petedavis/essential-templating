﻿using System;
using System.Diagnostics;
using System.Globalization;
using Essential.Templating.Caching;
using Essential.Templating.Configuration;
using Essential.Templating.Storage;
using Essential.Templating.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Essential.Templating.Tests
{
    [TestClass]
    [DeploymentItem("Templates", "Templates")]
    [DeploymentItem(@"ru\Essential.Templating.Tests.resources.dll", "ru")]
    public class TemplateEngineTest
    {
        private readonly ITemplateEngine _templateEngine;

        public TemplateEngineTest()
        {
            var config = new TemplateEngineConfiguration
            {
                ResourceProvider = new FileSystemResourceProvider("Templates"),
                CachePolicy = CachePolicy.Instance
            };
            _templateEngine = new TemplateEngine(config);
        }

        [TestMethod]
        public void RenderLocalizedTemplate_RendersInSpecifiedCulture()
        {
            var template = _templateEngine.Render("Test.cshtml", null, new CultureInfo("ru-RU"));
            
            Assert.IsNotNull(template);
            Debug.WriteLine(template);

            Assert.IsTrue(!string.IsNullOrEmpty(template));
        }

        [TestMethod]
        public void RenderTemplateWithModel_RendersCorrectText()
        {
            var template = _templateEngine.Render("Test.cshtml", "Model", null, CultureInfo.InvariantCulture);

            Assert.IsNotNull(template);
            Debug.WriteLine(template);

            Assert.IsTrue(template == "Rendered string: Model");
        }

        [TestMethod]
        public void RenderTemplateWithViewBag_RendersCorrectText()
        {
            var template = _templateEngine.Render("ViewBag.cshtml", new {Hello = "Hello, World!"}, null);

            Assert.IsNotNull(template);
            Debug.WriteLine(template);

            Assert.IsTrue(template == "Hello, World!");
        }

        [TestMethod]
        public void RenderExposingTemplate_RendersCorrectTemplateStructure()
        {
            var templateStructure = _templateEngine.Render("Exposing.cshtml",
                renderer: new TemplateStructureRenderer());

            Assert.IsNotNull(templateStructure);
            Debug.WriteLine(templateStructure.Body);

            Assert.IsTrue(templateStructure.StartCalled);
            Assert.IsTrue(templateStructure.EndCalled);
            Assert.IsTrue(templateStructure.Body.Length > 0);
            Assert.IsTrue(templateStructure.Sections.Contains("Section1"));
            Assert.IsTrue(templateStructure.Sections.Contains("Section2"));
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void TemplateEngine_OnEmptyPath_ThrowsArgumentException()
        {
            _templateEngine.Render("");
        }

        [TestMethod]
        public void TemplateEngine_RenderAsync_Executes()
        {
            var template =
                _templateEngine.RenderAsync("Test.cshtml", "Model", null, CultureInfo.InvariantCulture).Result;

            Assert.IsNotNull(template);
            Assert.IsTrue(template.Length > 0);
        }
    }
}