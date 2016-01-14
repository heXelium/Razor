// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using Microsoft.AspNet.Razor.Tokenizer.Symbols;

namespace Microsoft.AspNet.Razor.Test.Framework
{
    internal class RawTextSymbol : ISymbol
    {
        public string Content { get; }

        public RawTextSymbol(string content)
        {
            Content = content;
        }

        public override bool Equals(object obj)
        {
            var other = obj as RawTextSymbol;
            return other != null && Equals(Content, other.Content);
        }

        internal bool EquivalentTo(ISymbol sym)
        {
            return Equals(Content, sym.Content);
        }

        public override int GetHashCode()
        {
            // Hash code should include only immutable properties.
            return Content == null ? 0 : Content.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "RAW - [{1}]", Content);
        }
    }
}
