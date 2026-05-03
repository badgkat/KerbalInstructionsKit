using KerbalInstructionsKit.Config;
using KerbalInstructionsKit.Core;
using KerbalInstructionsKit.Tests.TestHelpers;
using Xunit;

namespace KerbalInstructionsKit.Tests
{
    public class LinkLoaderTests
    {
        [Fact]
        public void Load_ParsesLessonLink()
        {
            var n = new FakeSceneNode().Set("type", "lesson").Set("target", "LSN_X").Set("label", "Go to X");
            var link = LinkLoader.Load(n);
            Assert.NotNull(link);
            Assert.Equal(LinkType.Lesson, link.Type);
            Assert.Equal("LSN_X", link.Target);
            Assert.Equal("Go to X", link.Label);
        }

        [Fact]
        public void Load_ParsesKspediaLink()
        {
            var n = new FakeSceneNode().Set("type", "kspedia").Set("target", "ManeuverNodes").Set("label", "KSPedia");
            var link = LinkLoader.Load(n);
            Assert.Equal(LinkType.Kspedia, link.Type);
            Assert.Equal("ManeuverNodes", link.Target);
        }

        [Fact]
        public void Load_ParsesUrlLink_Http()
        {
            var n = new FakeSceneNode().Set("type", "url").Set("target", "https://example.com").Set("label", "Wiki");
            var link = LinkLoader.Load(n);
            Assert.Equal(LinkType.Url, link.Type);
            Assert.Equal("https://example.com", link.Target);
        }

        [Fact]
        public void Load_NormalizesBackslashesInUrl()
        {
            var n = new FakeSceneNode().Set("type", "url").Set("target", @"https:\wiki.example.com\page").Set("label", "Wiki");
            var link = LinkLoader.Load(n);
            Assert.NotNull(link);
            Assert.Equal(LinkType.Url, link.Type);
            Assert.Equal("https://wiki.example.com/page", link.Target);
        }

        [Fact]
        public void Load_RejectsUrlLink_NonHttp()
        {
            var n = new FakeSceneNode().Set("type", "url").Set("target", "file:///etc/passwd").Set("label", "X");
            Assert.Null(LinkLoader.Load(n));
        }

        [Fact]
        public void Load_RejectsLink_MissingTarget()
        {
            var n = new FakeSceneNode().Set("type", "lesson").Set("label", "X");
            Assert.Null(LinkLoader.Load(n));
        }

        [Fact]
        public void Load_RejectsLink_UnknownType()
        {
            var n = new FakeSceneNode().Set("type", "weird").Set("target", "X").Set("label", "X");
            Assert.Null(LinkLoader.Load(n));
        }

        [Fact]
        public void Load_RejectsLink_MissingType()
        {
            var n = new FakeSceneNode().Set("target", "X").Set("label", "X");
            Assert.Null(LinkLoader.Load(n));
        }

        [Fact]
        public void Load_RejectsNullNode()
        {
            Assert.Null(LinkLoader.Load(null));
        }
    }
}
