// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Text;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Test.Text
{
    public class LineTrackingStringBufferTest
    {
        [Fact]
        public void CtorInitializesProperties()
        {
            var buffer = new LineTrackingStringBuffer();
            Assert.Equal(0, buffer.Length);
        }

        [Fact]
        public void CharAtCorrectlyReturnsLocation()
        {
            var buffer = new LineTrackingStringBuffer();
            buffer.Append("foo\rbar\nbaz\r\nbiz");
            LineTrackingStringBuffer.CharacterReference chr = buffer.CharAt(14);
            Assert.Equal('i', chr.Character);
            Assert.Equal(new SourceLocation(14, 3, 1), chr.Location);
        }
    }
}
