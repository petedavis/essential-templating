// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace Essential.Templating.Razor.Host.Rendering
{
    public class HelperResult
    {
        private readonly Action<TextWriter> _action;

        public HelperResult(Action<TextWriter> action)
        {
            Contract.Requires<ArgumentNullException>(action != null);
            _action = action;
        }

        public Action<TextWriter> WriteAction
        {
            get { return _action; }
        }

        public virtual void WriteTo(TextWriter writer)
        {
            Contract.Requires<ArgumentNullException>(writer != null);
            _action(writer);
        }
    }
}