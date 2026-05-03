using KerbalInstructionsKit.Config;
using KerbalInstructionsKit.Core;
using KerbalInstructionsKit.Tests.TestHelpers;
using Xunit;

namespace KerbalInstructionsKit.Tests
{
    public class PageLoaderTests
    {
        [Fact]
        public void Load_ParsesTextPage()
        {
            var n = new FakeSceneNode()
                .Set("title", "Introduction")
                .Set("text", "Welcome to KSP!");
            var page = PageLoader.Load(n);
            Assert.NotNull(page);
            Assert.Equal("Introduction", page.Title);
            Assert.Equal("Welcome to KSP!", page.Text);
            Assert.Null(page.Image);
        }

        [Fact]
        public void Load_ParsesImagePage()
        {
            var n = new FakeSceneNode()
                .Set("image", "KIK/images/rocket")
                .Set("caption", "A rocket");
            var page = PageLoader.Load(n);
            Assert.NotNull(page);
            Assert.Equal("KIK/images/rocket", page.Image);
            Assert.Equal("A rocket", page.Caption);
            Assert.Null(page.Text);
        }

        [Fact]
        public void Load_ParsesPageWithTextAndImage()
        {
            var n = new FakeSceneNode()
                .Set("text", "Some text")
                .Set("image", "KIK/images/orbit");
            var page = PageLoader.Load(n);
            Assert.NotNull(page);
            Assert.Equal("Some text", page.Text);
            Assert.Equal("KIK/images/orbit", page.Image);
        }

        [Fact]
        public void Load_RejectsPageWithNeitherTextNorImage()
        {
            var n = new FakeSceneNode().Set("title", "Empty");
            Assert.Null(PageLoader.Load(n));
        }

        [Fact]
        public void Load_RejectsNullNode()
        {
            Assert.Null(PageLoader.Load(null));
        }

        [Fact]
        public void Load_ParsesLinksOnPage()
        {
            var linkNode = new FakeSceneNode()
                .Set("type", "lesson")
                .Set("target", "LSN_Next")
                .Set("label", "Next");
            var n = new FakeSceneNode()
                .Set("text", "Read more:")
                .AddNode("LINK", linkNode);
            var page = PageLoader.Load(n);
            Assert.NotNull(page);
            Assert.Single(page.Links);
            Assert.Equal("LSN_Next", page.Links[0].Target);
        }

        [Fact]
        public void Load_SkipsInvalidLinksButKeepsValid()
        {
            var validLink = new FakeSceneNode()
                .Set("type", "lesson")
                .Set("target", "LSN_A")
                .Set("label", "A");
            var invalidLink = new FakeSceneNode()
                .Set("type", "unknown")
                .Set("target", "X")
                .Set("label", "X");
            var n = new FakeSceneNode()
                .Set("text", "Links")
                .AddNode("LINK", validLink)
                .AddNode("LINK", invalidLink);
            var page = PageLoader.Load(n);
            Assert.NotNull(page);
            Assert.Single(page.Links);
            Assert.Equal("LSN_A", page.Links[0].Target);
        }
    }
}
