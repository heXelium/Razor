// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.Razor.Tokenizer.Symbols
{
    public class HtmlSymbol : SymbolBase<HtmlSymbolType>
    {
        // Helper constructor
        public HtmlSymbol(string content, HtmlSymbolType type)
            : this(content, type, Enumerable.Empty<RazorError>())
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
        }

        public HtmlSymbol(
            string content,
            HtmlSymbolType type,
            IEnumerable<RazorError> errors)
            : base(content, type, errors)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
        }
    }
}
