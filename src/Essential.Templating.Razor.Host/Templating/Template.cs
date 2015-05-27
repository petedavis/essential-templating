using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Essential.Templating.Razor.Host.Execution;
using Essential.Templating.Razor.Host.Rendering;

namespace Essential.Templating.Razor.Host.Templating
{
    public abstract class Template : ITemplate
    {
        private bool _renderedBody;
        private TemplateContext _templateContext;
        private readonly HashSet<string> _renderedSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, RenderAsyncDelegate> _sectionWriters =
            new Dictionary<string, RenderAsyncDelegate>(StringComparer.OrdinalIgnoreCase);

        private readonly ExpandoObject _viewBag = new ExpandoObject();

        public dynamic ViewBag
        {
            get { return _viewBag; }
        }

        protected ITemplate ExplicitTemplate
        {
            get { return this; }
        }

        public string Id { get; set; }
        public string FilePath { get; set; }
        public string LayoutId { get; set; }
        public bool IsPartial { get; set; }

        public TemplateContext Context
        {
            get { return _templateContext; }
            set { SetContext(value); }
        }

        Action<TextWriter> ITemplate.RenderBodyDelegate { get; set; }
        Func<string, object, TextWriter, Task> ITemplate.RenderPartialAsyncDelegate { get; set; }
        bool ITemplate.IsLayoutBeingRendered { get; set; }
        IDictionary<string, RenderAsyncDelegate> ITemplate.PreviousSectionWriters { get; set; }
        IDictionary<string, RenderAsyncDelegate> ITemplate.SectionWriters
        {
            get { return _sectionWriters; }
        }

        public void EnsureRenderedBodyOrSections()
        {
            // a) all sections defined for this page are rendered.
            // b) if no sections are defined, then the body is rendered if it's available.
            var explicitTemplate = (ITemplate) this;
            if (explicitTemplate.PreviousSectionWriters != null && explicitTemplate.PreviousSectionWriters.Count > 0)
            {
                var sectionsNotRendered = explicitTemplate.PreviousSectionWriters.Keys.Except(
                    _renderedSections,
                    StringComparer.OrdinalIgnoreCase).ToList();

                if (sectionsNotRendered.Any())
                {
                    var sectionNames = string.Join(", ", sectionsNotRendered);
                    var message = string.Format("Following sections were not rendered: {0}.", sectionNames);
                    throw new InvalidOperationException(message);
                }
            }
            else if (explicitTemplate.RenderBodyDelegate != null && !_renderedBody)
            {
                // There are no sections defined, but RenderBody was NOT called.
                // If a body was defined, then RenderBody should have been called.
                const string message = "RenderBody was not called.";
                throw new InvalidOperationException(message);
            }
        }

        public abstract Task ExecuteAsync();

        public void DefineSection(string name, RenderAsyncDelegate section)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name));
            Contract.Requires<ArgumentException>(section != null);
            if (ExplicitTemplate.SectionWriters.ContainsKey(name))
            {
                var message = string.Format("Section with name '{0}' was already defined.", name);
                throw new InvalidOperationException(message);
            }
            ExplicitTemplate.SectionWriters[name] = section;
        }

        public bool IsSectionDefined(string name)
        {
            EnsureMethodCanBeInvoked("IsSectionDefined");

            return ExplicitTemplate.PreviousSectionWriters.ContainsKey(name);
        }

        public HtmlString RenderSection(string name)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name));
            EnsureTemplateContextIsSet();

            return RenderSection(name, true);
        }

        public HtmlString RenderSection(string name, bool required)
        {
            EnsureMethodCanBeInvoked("RenderSection");
            EnsureTemplateContextIsSet();

            var task = RenderSectionAsyncCore(name, required);
            try
            {
                return task.Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public Task<HtmlString> RenderSectionAsync(string name)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name));
            EnsureTemplateContextIsSet();

            return RenderSectionAsync(name, true);
        }

        public async Task<HtmlString> RenderSectionAsync(string name, bool required)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name));
            EnsureTemplateContextIsSet();

            EnsureMethodCanBeInvoked("RenderSectionAsync");
            return await RenderSectionAsyncCore(name, required);
        }

        public void RenderPartial(string id, object model = null)
        {
            EnsureTemplateContextIsSet();
            try
            {
                RenderPartialAsync(id, model).Wait();
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public Task RenderPartialAsync(string id, object model = null)
        {
            EnsureTemplateContextIsSet();

            var templateExplicit = (ITemplate) this;
            if (templateExplicit.RenderPartialAsyncDelegate != null)
            {
                return templateExplicit.RenderPartialAsyncDelegate(id, model, Context.Writer);
            }
            return Task.FromResult<string>(null);
        }

        public virtual void Write(object value)
        {
            EnsureTemplateContextIsSet();

            WriteTo(Context.Writer, value);
        }

        public virtual void WriteLiteral(object value)
        {
            WriteLiteralTo(Context.Writer, value);
        }

        public virtual void WriteAttribute(
            string name, PositionTagged<string> prefix, PositionTagged<string> suffix,
            params AttributeValue[] values)
        {
            WriteAttributeTo(Context.Writer, name, prefix, suffix, values);
        }

        public virtual HelperResult RenderBody()
        {
            var explicitTemplate = (ITemplate) this;
            if (explicitTemplate.RenderBodyDelegate == null)
            {
                var message = string.Format("RenderBody of {0} cannot be called.", Id);
                throw new InvalidOperationException(message);
            }
            _renderedBody = true;
            return new HelperResult(explicitTemplate.RenderBodyDelegate);
        }

        protected virtual void SetContext(TemplateContext context)
        {
            _templateContext = context;
        }

        internal async Task<HtmlString> FlushAsync()
        {
            var explicitTemplate = (ITemplate) this;

            // Calls to Flush are allowed if the page does not specify a Layout or if it is executing a section in the
            // Layout.
            if (!explicitTemplate.IsLayoutBeingRendered && !string.IsNullOrEmpty(LayoutId))
            {
                const string message =
                    "Calls to Flush are allowed if the page does not specify a Layout or if it is executing a section in the Layout";
                throw new InvalidOperationException(message);
            }

            await Context.Writer.FlushAsync();
            return HtmlString.Empty;
        }

        private void WriteTo(TextWriter writer, object value)
        {
            EnsureTemplateContextIsSet();

            WriteTo(writer, value, false);
        }

        private static void WriteTo(
            TextWriter writer,
            object value,
            bool escapeQuotes)
        {
            if (value == null || value == HtmlString.Empty)
            {
                return;
            }

            var helperResult = value as HelperResult;
            if (helperResult != null)
            {
                helperResult.WriteTo(writer);
                return;
            }

            var htmlString = value as HtmlString;
            if (htmlString != null)
            {
                if (escapeQuotes)
                {
                    // In this case the text likely came directly from the Razor source. Since the original string is
                    // an attribute value that may have been quoted with single quotes, must handle any double quotes
                    // in the value. Writing the value out surrounded by double quotes.
                    //
                    // Do not combine following condition with check of escapeQuotes; htmlString.ToString() can be
                    // expensive when the HtmlString is created with a StringCollectionTextWriter.
                    var stringValue = htmlString.ToString();
                    if (stringValue.Contains("\""))
                    {
                        writer.Write(stringValue.Replace("\"", "&quot;"));
                        return;
                    }
                }

                htmlString.WriteTo(writer);
                return;
            }

            WriteTo(writer, value.ToString());
        }

        private static void WriteTo(TextWriter writer, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                WebUtility.HtmlEncode(value, writer);
            }
        }


        private static void WriteLiteralTo(TextWriter writer, object value)
        {
            if (value != null)
            {
                WriteLiteralTo(writer, value.ToString());
            }
        }

        private static void WriteLiteralTo(TextWriter writer, string value)
        {
            Contract.Requires<ArgumentNullException>(writer != null);
            if (!string.IsNullOrEmpty(value))
            {
                writer.Write(value);
            }
        }

        private void WriteAttributeTo(
            TextWriter writer,
            string name,
            PositionTagged<string> prefix,
            PositionTagged<string> suffix,
            params AttributeValue[] values)
        {
            Contract.Requires<ArgumentNullException>(writer != null);
            Contract.Requires<ArgumentNullException>(prefix != null);
            Contract.Requires<ArgumentNullException>(suffix != null);
            var first = true;
            var wroteSomething = false;
            if (values.Length == 0)
            {
                // Explicitly empty attribute, so write the prefix and suffix
                WritePositionTaggedLiteral(writer, prefix);
                WritePositionTaggedLiteral(writer, suffix);
            }
            else
            {
                for (var i = 0; i < values.Length; i++)
                {
                    var attrVal = values[i];
                    var val = attrVal.Value;
                    var next = i == values.Length - 1
                        ? suffix
                        : // End of the list, grab the suffix
                        values[i + 1].Prefix; // Still in the list, grab the next prefix

                    if (val.Value == null)
                    {
                        // Nothing to write
                        continue;
                    }

                    // The special cases here are that the value we're writing might already be a string, or that the
                    // value might be a bool. If the value is the bool 'true' we want to write the attribute name
                    // instead of the string 'true'. If the value is the bool 'false' we don't want to write anything.
                    // Otherwise the value is another object (perhaps an HtmlString) and we'll ask it to format itself.
                    string stringValue;

                    // Intentionally using is+cast here for performance reasons. This is more performant than as+bool?
                    // because of boxing.
                    if (val.Value is bool)
                    {
                        if ((bool) val.Value)
                        {
                            stringValue = name;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        stringValue = val.Value as string;
                    }

                    if (first)
                    {
                        WritePositionTaggedLiteral(writer, prefix);
                        first = false;
                    }
                    else
                    {
                        WritePositionTaggedLiteral(writer, attrVal.Prefix);
                    }

                    // The extra branching here is to ensure that we call the Write*To(string) overload where
                    // possible.
                    if (attrVal.Literal && stringValue != null)
                    {
                        WriteLiteralTo(writer, stringValue);
                    }
                    else if (attrVal.Literal)
                    {
                        WriteLiteralTo(writer, val.Value);
                    }
                    else if (stringValue != null)
                    {
                        WriteTo(writer, stringValue);
                    }
                    else
                    {
                        WriteTo(writer, val.Value);
                    }
                    wroteSomething = true;
                }
                if (wroteSomething)
                {
                    WritePositionTaggedLiteral(writer, suffix);
                }
            }
        }

        private void WritePositionTaggedLiteral(TextWriter writer, string value)
        {
            WriteLiteralTo(writer, value);
        }

        private void WritePositionTaggedLiteral(TextWriter writer, PositionTagged<string> value)
        {
            WritePositionTaggedLiteral(writer, value.Value);
        }

        private async Task<HtmlString> RenderSectionAsyncCore(string sectionName, bool required)
        {
            if (_renderedSections.Contains(sectionName))
            {
                var message = string.Format("The section with name '{0}' was already rendered.", sectionName);
                throw new InvalidOperationException(message);
            }

            RenderAsyncDelegate renderDelegate;
            var explicitTemplate = (ITemplate) this;
            if (explicitTemplate.PreviousSectionWriters.TryGetValue(sectionName, out renderDelegate))
            {
                _renderedSections.Add(sectionName);
                await renderDelegate(Context.Writer);

                // Return a token value that allows the Write call that wraps the RenderSection \ RenderSectionAsync
                // to succeed.
                return HtmlString.Empty;
            }
            if (required)
            {
                var message = string.Format("The section with name '{0}' was not defined.", sectionName);
                throw new InvalidOperationException(message);
            }
            // If the section is optional and not found, then don't do anything.
            return null;
        }

        private void EnsureMethodCanBeInvoked(string methodName)
        {
            var explicitTemplate = (ITemplate) this;
            if (explicitTemplate.PreviousSectionWriters == null)
            {
                var message = string.Format("Can't call {0} becase previous section writers are not defined.",
                    methodName);
                throw new InvalidOperationException(message);
            }
        }

        private void EnsureTemplateContextIsSet()
        {
            if (Context == null)
            {
                throw new InvalidOperationException("Template execution context hasn't been set.");
            }
        }
    }
}