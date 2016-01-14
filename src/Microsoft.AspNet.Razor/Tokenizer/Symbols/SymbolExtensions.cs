// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.Text;

namespace Microsoft.AspNet.Razor.Tokenizer.Symbols
{
    public static class SymbolExtensions
    {
        public static LocationTagged<string> GetContent(this SpanBuilder span)
        {
            return GetContent(span.Symbols, span.Start);
        }

        public static LocationTagged<string> GetContent(this IEnumerable<ISymbol> symbols, SourceLocation spanStart)
        {
            if (symbols.Any())
            {
                return new LocationTagged<string>(GetContent(symbols), spanStart);
            }
            else
            {
                return new LocationTagged<string>(string.Empty, spanStart);
            }
        }

        public static string GetContent(this IEnumerable<ISymbol> symbols) =>
            string.Concat(symbols.Select(s => s.Content));

        /// <summary>
        /// Converts the generic <see cref="IEnumerable{ISymbol}"/> to a <see cref="IEnumerable{HtmlSymbol}"/> and
        /// finds the first <see cref="HtmlSymbol"/> with type <paramref name="type"/>.
        /// </summary>
        /// <param name="symbols">The <see cref="IEnumerable{ISymbol}"/> instance this method extends.</param>
        /// <param name="type">The <see cref="HtmlSymbolType"/> to search for.</param>
        /// <returns>The first <see cref="HtmlSymbol"/> of type <paramref name="type"/>.</returns>
        public static HtmlSymbol FirstHtmlSymbolAs(this IEnumerable<ISymbol> symbols, HtmlSymbolType type)
        {
            return symbols.OfType<HtmlSymbol>().FirstOrDefault(sym => (type & sym.Type) == sym.Type);
        }
    }
}
