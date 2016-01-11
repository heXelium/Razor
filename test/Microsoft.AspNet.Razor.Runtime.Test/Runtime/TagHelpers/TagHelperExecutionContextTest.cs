// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.Extensions.WebEncoders.Testing;
using Xunit;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    public class TagHelperExecutionContextTest
    {

        [Theory]
        [InlineData(TagMode.SelfClosing)]
        [InlineData(TagMode.StartTagAndEndTag)]
        [InlineData(TagMode.StartTagOnly)]
        public void TagMode_ReturnsExpectedValue(TagMode tagMode)
        {
            // Arrange & Act
            var executionContext = new TagHelperExecutionContext("p", tagMode);

            // Assert
            Assert.Equal(tagMode, executionContext.TagMode);
        }

        [Fact]
        public void ParentItems_SetsItemsProperty()
        {
            // Arrange
            var expectedItems = new Dictionary<object, object>
            {
                { "test-entry", 1234 }
            };

            // Act
            var executionContext = new TagHelperExecutionContext(
                "p",
                tagMode: TagMode.StartTagAndEndTag,
                items: expectedItems,
                uniqueId: string.Empty,
                executeChildContentAsync: async () => await Task.FromResult(result: true),
                startTagHelperWritingScope: _ => { },
                endTagHelperWritingScope: () => new DefaultTagHelperContent());

            // Assert
            Assert.NotNull(executionContext.Items);
            Assert.Same(expectedItems, executionContext.Items);
        }

        public static TheoryData<HtmlEncoder> HtmlEncoderData
        {
            get
            {
                return new TheoryData<HtmlEncoder>
                {
                    HtmlEncoder.Default,
                    NullHtmlEncoder.Default,
                    new HtmlTestEncoder(),
                };
            }
        }

        [Theory]
        [MemberData(nameof(HtmlEncoderData))]
        public async Task GetChildContentAsync_ReturnsExpectedContent(HtmlEncoder encoder)
        {
            // Arrange
            var tagHelperContent = new DefaultTagHelperContent();
            var executionCount = 0;
            var content = "Hello from child content";
            var expectedContent = $"HtmlEncode[[{content}]]";
            var executionContext = new TagHelperExecutionContext(
                "p",
                tagMode: TagMode.StartTagAndEndTag,
                items: new Dictionary<object, object>(),
                uniqueId: string.Empty,
                executeChildContentAsync: () =>
                {
                    executionCount++;
                    tagHelperContent.SetContent(content);

                    return Task.FromResult(result: true);
                },
                startTagHelperWritingScope: _ => { },
                endTagHelperWritingScope: () => tagHelperContent);

            // Act
            var actualContent = await executionContext.GetChildContentAsync(useCachedResult: true, encoder: encoder);

            // Assert
            Assert.Equal(expectedContent, actualContent.GetContent(new HtmlTestEncoder()));
        }

        [Theory]
        [MemberData(nameof(HtmlEncoderData))]
        public async Task GetChildContentAsync_StartsWritingScopeWithGivenEncoder(HtmlEncoder encoder)
        {
            // Arrange
            HtmlEncoder passedEncoder = null;
            var executionContext = new TagHelperExecutionContext(
                "p",
                tagMode: TagMode.StartTagAndEndTag,
                items: new Dictionary<object, object>(),
                uniqueId: string.Empty,
                executeChildContentAsync: () => Task.FromResult(result: true),
                startTagHelperWritingScope: encoderArgument => passedEncoder = encoderArgument,
                endTagHelperWritingScope: () => new DefaultTagHelperContent());

            // Act
            await executionContext.GetChildContentAsync(useCachedResult: true, encoder: encoder);

            // Assert
            Assert.Same(encoder, passedEncoder);
        }

        [Theory]
        [MemberData(nameof(HtmlEncoderData))]
        public async Task GetChildContentAsync_CachesValue(HtmlEncoder encoder)
        {
            // Arrange
            var executionCount = 0;
            var executionContext = new TagHelperExecutionContext(
                "p",
                tagMode: TagMode.StartTagAndEndTag,
                items: new Dictionary<object, object>(),
                uniqueId: string.Empty,
                executeChildContentAsync: () =>
                {
                    executionCount++;
                    return Task.FromResult(result: true);
                },
                startTagHelperWritingScope: _ => { },
                endTagHelperWritingScope: () => new DefaultTagHelperContent());

            // Act
            var content1 = await executionContext.GetChildContentAsync(useCachedResult: true, encoder: encoder);
            var content2 = await executionContext.GetChildContentAsync(useCachedResult: true, encoder: encoder);

            // Assert
            Assert.Equal(1, executionCount);
        }

        [Theory]
        [MemberData(nameof(HtmlEncoderData))]
        public async Task GetChildContentAsync_CachesValuePerEncoder(HtmlEncoder encoder)
        {
            // Arrange
            var executionCount = 0;
            var executionContext = new TagHelperExecutionContext(
                "p",
                tagMode: TagMode.StartTagAndEndTag,
                items: new Dictionary<object, object>(),
                uniqueId: string.Empty,
                executeChildContentAsync: () =>
                {
                    executionCount++;
                    return Task.FromResult(result: true);
                },
                startTagHelperWritingScope: _ => { },
                endTagHelperWritingScope: () => new DefaultTagHelperContent());

            // Method compares HtmlEncoder instances. The same as Type comparisons when using Default properties.
            var firstEncoder = new HtmlTestEncoder();

            // Act
            var content1 = await executionContext.GetChildContentAsync(useCachedResult: true, encoder: firstEncoder);
            var content2 = await executionContext.GetChildContentAsync(useCachedResult: true, encoder: encoder);

            // Assert
            Assert.Equal(2, executionCount);
        }

        [Theory]
        [MemberData(nameof(HtmlEncoderData))]
        public async Task GetChildContentAsync_CanExecuteChildrenMoreThanOnce(HtmlEncoder encoder)
        {
            // Arrange
            var executionCount = 0;
            var executionContext = new TagHelperExecutionContext(
                "p",
                tagMode: TagMode.StartTagAndEndTag,
                items: new Dictionary<object, object>(),
                uniqueId: string.Empty,
                executeChildContentAsync: () =>
                {
                    executionCount++;
                    return Task.FromResult(result: true);
                },
                startTagHelperWritingScope: _ => { },
                endTagHelperWritingScope: () => new DefaultTagHelperContent());

            // Act
            await executionContext.GetChildContentAsync(useCachedResult: false, encoder: encoder);
            await executionContext.GetChildContentAsync(useCachedResult: false, encoder: encoder);

            // Assert
            Assert.Equal(2, executionCount);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetChildContentAsync_ReturnsNewObjectEveryTimeItIsCalled(bool useCachedResult)
        {
            // Arrange
            var executionContext = new TagHelperExecutionContext(
                "p",
                tagMode: TagMode.StartTagAndEndTag,
                items: new Dictionary<object, object>(),
                uniqueId: string.Empty,
                executeChildContentAsync: () => Task.FromResult(result: true),
                startTagHelperWritingScope: _ => { },
                endTagHelperWritingScope: () => new DefaultTagHelperContent());

            // Act
            var content1 = await executionContext.GetChildContentAsync(useCachedResult, encoder: null);
            var content2 = await executionContext.GetChildContentAsync(useCachedResult, encoder: null);

            // Assert
            Assert.NotSame(content1, content2);
        }

        public static TheoryData<string, string> DictionaryCaseTestingData
        {
            get
            {
                return new TheoryData<string, string>
                {
                    { "class", "CLaSS" },
                    { "Class", "class" },
                    { "Class", "claSS" }
                };
            }
        }

        [Theory]
        [MemberData(nameof(DictionaryCaseTestingData))]
        public void HtmlAttributes_IgnoresCase(string originalName, string updatedName)
        {
            // Arrange
            var executionContext = new TagHelperExecutionContext("p", TagMode.StartTagAndEndTag);
            executionContext.HTMLAttributes[originalName] = "hello";

            // Act
            executionContext.HTMLAttributes[updatedName] = "something else";

            // Assert
            var attribute = Assert.Single(executionContext.HTMLAttributes);
            Assert.Equal(new TagHelperAttribute(originalName, "something else"), attribute);
        }

        [Theory]
        [MemberData(nameof(DictionaryCaseTestingData))]
        public void AllAttributes_IgnoresCase(string originalName, string updatedName)
        {
            // Arrange
            var executionContext = new TagHelperExecutionContext("p", tagMode: TagMode.StartTagAndEndTag);
            executionContext.AllAttributes.Add(originalName, value: false);

            // Act
            executionContext.AllAttributes[updatedName].Value = true;

            // Assert
            var attribute = Assert.Single(executionContext.AllAttributes);
            Assert.Equal(new TagHelperAttribute(originalName, true), attribute);
        }

        [Fact]
        public void AddHtmlAttribute_MaintainsHTMLAttributes()
        {
            // Arrange
            var executionContext = new TagHelperExecutionContext("p", TagMode.StartTagAndEndTag);
            var expectedAttributes = new TagHelperAttributeList
            {
                { "class", "btn" },
                { "foo", "bar" }
            };

            // Act
            executionContext.AddHtmlAttribute("class", "btn");
            executionContext.AddHtmlAttribute("foo", "bar");

            // Assert
            Assert.Equal(
                expectedAttributes,
                executionContext.HTMLAttributes,
                CaseSensitiveTagHelperAttributeComparer.Default);
        }

        [Fact]
        public void AddMinimizedHtmlAttribute_MaintainsHTMLAttributes()
        {
            // Arrange
            var executionContext = new TagHelperExecutionContext("input", tagMode: TagMode.StartTagOnly);
            var expectedAttributes = new TagHelperAttributeList
            {
                ["checked"] = new TagHelperAttribute { Name = "checked", Minimized = true },
                ["visible"] = new TagHelperAttribute { Name = "visible", Minimized = true }
            };

            // Act
            executionContext.AddMinimizedHtmlAttribute("checked");
            executionContext.AddMinimizedHtmlAttribute("visible");

            // Assert
            Assert.Equal(
                expectedAttributes,
                executionContext.HTMLAttributes,
                CaseSensitiveTagHelperAttributeComparer.Default);
        }

        [Fact]
        public void AddMinimizedHtmlAttribute_MaintainsHTMLAttributes_SomeMinimized()
        {
            // Arrange
            var executionContext = new TagHelperExecutionContext("input", tagMode: TagMode.SelfClosing);
            var expectedAttributes = new TagHelperAttributeList
            {
                { "class", "btn" },
                { "foo", "bar" }
            };
            expectedAttributes.Add(new TagHelperAttribute { Name = "checked", Minimized = true });
            expectedAttributes.Add(new TagHelperAttribute { Name = "visible", Minimized = true });

            // Act
            executionContext.AddHtmlAttribute("class", "btn");
            executionContext.AddHtmlAttribute("foo", "bar");
            executionContext.AddMinimizedHtmlAttribute("checked");
            executionContext.AddMinimizedHtmlAttribute("visible");

            // Assert
            Assert.Equal(
                expectedAttributes,
                executionContext.HTMLAttributes,
                CaseSensitiveTagHelperAttributeComparer.Default);
        }

        [Fact]
        public void TagHelperExecutionContext_MaintainsAllAttributes()
        {
            // Arrange
            var executionContext = new TagHelperExecutionContext("p", TagMode.StartTagAndEndTag);
            var expectedAttributes = new TagHelperAttributeList
            {
                { "class", "btn" },
                { "something", true },
                { "foo", "bar" }
            };

            // Act
            executionContext.AddHtmlAttribute("class", "btn");
            executionContext.AddTagHelperAttribute("something", true);
            executionContext.AddHtmlAttribute("foo", "bar");

            // Assert
            Assert.Equal(
                expectedAttributes,
                executionContext.AllAttributes,
                CaseSensitiveTagHelperAttributeComparer.Default);
        }

        [Fact]
        public void Add_MaintainsTagHelpers()
        {
            // Arrange
            var executionContext = new TagHelperExecutionContext("p", TagMode.StartTagAndEndTag);
            var tagHelper = new PTagHelper();

            // Act
            executionContext.Add(tagHelper);

            // Assert
            var singleTagHelper = Assert.Single(executionContext.TagHelpers);
            Assert.Same(tagHelper, singleTagHelper);
        }

        [Fact]
        public void Add_MaintainsMultipleTagHelpers()
        {
            // Arrange
            var executionContext = new TagHelperExecutionContext("p", TagMode.StartTagAndEndTag);
            var tagHelper1 = new PTagHelper();
            var tagHelper2 = new PTagHelper();

            // Act
            executionContext.Add(tagHelper1);
            executionContext.Add(tagHelper2);

            // Assert
            var tagHelpers = executionContext.TagHelpers.ToArray();
            Assert.Equal(2, tagHelpers.Length);
            Assert.Same(tagHelper1, tagHelpers[0]);
            Assert.Same(tagHelper2, tagHelpers[1]);
        }

        private class PTagHelper : TagHelper
        {
        }
    }
}