﻿using System;
using System.IO;
using System.Threading.Tasks;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace Essential.Templating.Compilation
{
    public class VBTemplateCompiler : ITemplateCompiler
    {
        private readonly TemplateService _razorService;

        public VBTemplateCompiler()
        {
            _razorService = new TemplateService(new TemplateServiceConfiguration
            {
                Language = Language.VisualBasic,
                BaseTemplateType = typeof (Template)
            });
        }

        public void AddNamespace(string @namespace)
        {
            _razorService.AddNamespace(@namespace);
        }

        public Type Compile(Stream razorTemplate)
        {
            return _razorService.Compile(razorTemplate, null);
        }

        public Type Compile(Stream razorTemplate, Type modelType)
        {
            return _razorService.Compile(razorTemplate, modelType);
        }

        public Task<Type> CompileAsync(Stream razorTemplate)
        {
            return _razorService.CompileAsync(razorTemplate, null);
        }

        public Task<Type> CompileAsync(Stream razorTemplate, Type modelType)
        {
            return _razorService.CompileAsync(razorTemplate, modelType);
        }
    }
}