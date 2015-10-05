using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Caribbean.DataAccessLayer.PrintTemplates.Tests
{
    public class TemplateContentRepositoryTests
    {
        private const string TEST_FILES_BASE_PATH = "..\\..\\TemplateContentRepositoryTestFiles\\";

        [Test]
        public void FindAllRelativePaths_Case1()
        {
            // ARRANGE
            var html = File.ReadAllText(TEST_FILES_BASE_PATH + "FindAllRelativePaths-case1-input.html");

            // ACT
            var result = TemplateContentRepository.FindAllRelativePaths(html, new[] { "css", "images", "scripts" });

            // ASSERT
            result.ShouldBeEquivalentTo(
                new[]
                {
                    "css/site_global.css?418650203",
                    "css/master_a5---st_ende.css?64275528",
                    "css/fram-efterutskick.css?187324935",
                    "images/bild-2_3.jpg",
                    "images/logo.jpg",
                    "scripts/jquery-1.8.3.min.js",
                    "scripts/museutils.js?3793461109",
                    "scripts/jquery.watch.js?3766403489",
                });
        }

        [Test]
        public void FindAllRelativePaths_Case2()
        {
            // ARRANGE
            var html = File.ReadAllText(TEST_FILES_BASE_PATH + "FindAllRelativePaths-case2-input.html");

            // ACT
            var result = TemplateContentRepository.FindAllRelativePaths(html, new[] { "css", "images", "scripts" });

            // ASSERT
            result.ShouldBeEquivalentTo(
                new[]
                {
                    "scripts/jquery.watch.js?3766403489",
                    "scripts/museutils.js?3793461109",
                    "scripts/jquery-1.8.3.min.js",
                    "images/logo.jpg",
                    "images/extra_kontaktperson.jpg",
                    "images/maklaren.jpg",
                    "images/bild-2_3.jpg",
                    "images/bild-3_2.jpg",
                    "css/bak-efterutskick.css?355101299",
                    "css/master_a5---st_ende.css?64275528",
                    "css/site_global.css?418650203",
                    "css/iefonts_bak-efterutskick.css?54909579"
                });
        }

        [Test]
        public void FindAllRelativePaths_ShoudNotIncludeDuplicatePaths()
        {
            // ARRANGE

            // ACT
            var result = TemplateContentRepository.FindAllRelativePaths(
                    File.ReadAllText(TEST_FILES_BASE_PATH + "bak-efterutskick.html"), new[] {"css", "images"});

            // ASSERT
            result.ShouldBeEquivalentTo(new []
            {
                "css/site_global.css?4127533176",
                "css/master_a5---st_ende-utfall.css?3975356102",
                "css/bak-efterutskick.css?170404491",
                "css/iefonts_bak-efterutskick.css?526698638",
                "images/objektbild-bild2.jpg",
                "images/objektbild-bild3.jpg",
                "images/fb_storangsgatan_logo.svg",
                "images/fb_storangsgatan_logo_poster_.png",
                "images/personalbild-maklaren.jpg",
                "images/doljbar-beskrivning.jpg",
                "images/fri_vardering.png",
                "images/personalbild-extra_kontaktperson.jpg"
            });
        }
    }
}
