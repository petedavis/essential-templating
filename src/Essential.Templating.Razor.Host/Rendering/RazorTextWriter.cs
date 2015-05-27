// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Essential.Templating.Razor.Host.Rendering
{
    public class RazorTextWriter : TextWriter, IBufferedTextWriter
    {
        public RazorTextWriter(TextWriter unbufferedWriter, Encoding encoding)
        {
            UnbufferedWriter = unbufferedWriter;
            BufferedWriter = new StringCollectionTextWriter(encoding);
            TargetWriter = BufferedWriter;
            IsBuffering = true;
        }

        public override Encoding Encoding
        {
            get { return BufferedWriter.Encoding; }
        }

        public bool IsBuffering { get; private set; }

        // Internal for unit testing
        private StringCollectionTextWriter BufferedWriter { get; set; }

        private TextWriter UnbufferedWriter { get; set; }

        private TextWriter TargetWriter { get; set; }

        /// <inheritdoc />
        public override void Write(char value)
        {
            TargetWriter.Write(value);
        }

        /// <inheritdoc />
        public override void Write(object value)
        {
            var htmlString = value as HtmlString;
            if (htmlString != null)
            {
                htmlString.WriteTo(TargetWriter);
                return;
            }

            base.Write(value);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            Contract.Requires<ArgumentNullException>(buffer != null);
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (count < 0 || (buffer.Length - index < count))
            {
                throw new ArgumentOutOfRangeException("count");
            }

            TargetWriter.Write(buffer, index, count);
        }

        /// <inheritdoc />
        public override void Write(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                TargetWriter.Write(value);
            }
        }

        /// <inheritdoc />
        public override Task WriteAsync(char value)
        {
            return TargetWriter.WriteAsync(value);
        }

        /// <inheritdoc />
        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (count < 0 || (buffer.Length - index < count))
            {
                throw new ArgumentOutOfRangeException("count");
            }

            return TargetWriter.WriteAsync(buffer, index, count);
        }

        /// <inheritdoc />
        public override Task WriteAsync(string value)
        {
            return TargetWriter.WriteAsync(value);
        }

        /// <inheritdoc />
        public override void WriteLine()
        {
            TargetWriter.WriteLine();
        }

        /// <inheritdoc />
        public override void WriteLine(string value)
        {
            TargetWriter.WriteLine(value);
        }

        /// <inheritdoc />
        public override Task WriteLineAsync(char value)
        {
            return TargetWriter.WriteLineAsync(value);
        }

        /// <inheritdoc />
        public override Task WriteLineAsync(char[] value, int start, int offset)
        {
            return TargetWriter.WriteLineAsync(value, start, offset);
        }

        /// <inheritdoc />
        public override Task WriteLineAsync(string value)
        {
            return TargetWriter.WriteLineAsync(value);
        }

        /// <inheritdoc />
        public override Task WriteLineAsync()
        {
            return TargetWriter.WriteLineAsync();
        }

        /// <summary>
        /// Copies the buffered content to the unbuffered writer and invokes flush on it.
        /// Additionally causes this instance to no longer buffer and direct all write operations
        /// to the unbuffered writer.
        /// </summary>
        public override void Flush()
        {
            if (IsBuffering)
            {
                IsBuffering = false;
                TargetWriter = UnbufferedWriter;
                CopyTo(UnbufferedWriter);
            }

            UnbufferedWriter.Flush();
        }

        /// <summary>
        /// Copies the buffered content to the unbuffered writer and invokes flush on it.
        /// Additionally causes this instance to no longer buffer and direct all write operations
        /// to the unbuffered writer.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous copy and flush operations.</returns>
        public override async Task FlushAsync()
        {
            if (IsBuffering)
            {
                IsBuffering = false;
                TargetWriter = UnbufferedWriter;
                await CopyToAsync(UnbufferedWriter);
            }

            await UnbufferedWriter.FlushAsync();
        }

        /// <inheritdoc />
        public void CopyTo(TextWriter writer)
        {
            writer = UnWrapRazorTextWriter(writer);
            BufferedWriter.CopyTo(writer);
        }

        /// <inheritdoc />
        public Task CopyToAsync(TextWriter writer)
        {
            writer = UnWrapRazorTextWriter(writer);
            return BufferedWriter.CopyToAsync(writer);
        }

        private static TextWriter UnWrapRazorTextWriter(TextWriter writer)
        {
            var targetRazorTextWriter = writer as RazorTextWriter;
            if (targetRazorTextWriter != null)
            {
                writer = targetRazorTextWriter.IsBuffering ? targetRazorTextWriter.BufferedWriter :
                                                             targetRazorTextWriter.UnbufferedWriter;
            }

            return writer;
        }
    }
}