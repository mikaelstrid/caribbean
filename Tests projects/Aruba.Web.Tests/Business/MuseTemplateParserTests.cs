using System.IO;
using System.Linq;
using Caribbean.Aruba.Web.Business;
using Caribbean.Models.Database;
using FluentAssertions;
using NUnit.Framework;

namespace Caribbean.Aruba.Web.Tests.Business
{
    public class MuseTemplateParserTests
    {
        private const string TEST_FILES_BASE_PATH = "..\\..\\Business\\MuseTemplateParserTestFiles\\";

        [TestCase(1, new[] { "objekt_rum", "objekt_storlek" })]
        [TestCase(2, new[] { "objekt_rum", "objekt_storlek" })]
        [TestCase(3, new string[0])]
        [TestCase(4, new[] { "objekt_gata", "objekt_rum", "objekt_storlek", "kontorets_adress", "kontorets_tel", "kontorets_mail", "kontorets_www", "ansvarig_maklare", "ansvarig_maklare_tel", "ansvarig_maklare_mobil", "ansvarig_maklare_mail", "extra_kontaktperson", "extra_kontaktperson_tel", "extra_kontaktperson_mobil", "kontorets_mail",  })]
        public void FindAllTextFields(int number, string[] expectedResult)
        {
            // ARRANGE
            var html = File.ReadAllText(TEST_FILES_BASE_PATH + $"FindAllTextFields-case{number}-input.html");

            // ACT
            var result = MuseTemplateParser.FindAllTextFields(html);

            // ASSERT
            var simplifiedResult = result.Select(r => r.FieldName);
            simplifiedResult.ShouldAllBeEquivalentTo(expectedResult);
        }

        [TestCase(1, new[] { "fritext" }, new[] { "Brodtext" })]
        [TestCase(2, new[] { "fritext" }, new[] { "Brodtext" })]
        [TestCase(3, new[] { "fritext" }, new[] { "Brodtext" })]
        [TestCase(4, new[] { "beskrivning" }, new[] { "Brodtext" })]
        [TestCase(5, new[] { "fritext", "fritext2" }, new[] { "Brodtext", "Brodtext2" })]
        [TestCase(6, new[] { "objekt_kort_saljbeskrivning" }, new[] { "" })]
        [TestCase(7, new[] { "objekt_kort_saljbeskrivning", "fritext" }, new[] { "", "Brodtext" })]
        public void FindAllHtmlFields(int number, string[] expectedFieldNames, string[] expectedFirstParagraphClasses)
        {
            // ARRANGE
            var html = File.ReadAllText(TEST_FILES_BASE_PATH + $"FindAllHtmlFields-case{number}-input.html");

            // ACT
            var result = MuseTemplateParser.FindAllHtmlFields(html);

            // ASSERT
            var simplifiedExpected = expectedFieldNames.Zip(expectedFirstParagraphClasses, (f, s) => new HtmlFieldInfo { FieldName = f, FirstParagraphClass = s});
            result.ShouldAllBeEquivalentTo(simplifiedExpected);
        }

        [TestCase("1", new[] { "2_3" })]
        [TestCase("2", new[] { "2_3", "3_2" })]
        [TestCase("MC", new[] { "linhai_300b_4x4_eec_2014_r%c3%b6d-3", "linhai_300b_4x4_eec_2014_r%c3%b6d-4" })]
        [TestCase("CLOWN", new[] { "arton11015-d39d5-2", "arton11015-d39d5-3" })]
        public void FindAllImageType1Fields(string number, string[] expectedResult)
        {
            // ARRANGE
            var html = File.ReadAllText(TEST_FILES_BASE_PATH + $"FindAllImageType1Fields-case{number}-input.html");

            // ACT
            var result = MuseTemplateParser.FindAllImageType1Fields(html);

            // ASSERT
            var simplifiedResult = result.Select(r => r.FieldName);
            simplifiedResult.ShouldAllBeEquivalentTo(expectedResult);
        }

        [TestCase("1", new[] { "linhai_300b_4x4_eec_2014_r%c3%b6d" })]
        [TestCase("2", new[] { "kymco500_red-510x398-crop-u124" })]
        [TestCase("MC", new[] { "linhai_300b_4x4_eec_2014_r%c3%b6d-1", "5910_cc0d17a8", "kymco500_red-510x398", "5910_cc0d17a8187x198", "kymco500_red-510x398255x199", "linhai_300b_4x4_eec_2014_r%c3%b6d202x198-2", "5910_cc0d17a8-crop-u121", "kymco500_red-510x398-crop-u124", "5910_cc0d17a8281x139", "kymco500_red-510x398163x198", "linhai_300b_4x4_eec_2014_r%c3%b6d207x148", "5910_cc0d17a8-crop-u140", "kymco500_red-510x398-crop-u143" })]
        [TestCase("CLOWN", new[] { "arton11015-d39d5-1", "arton11015-d39d5-4", "arton11015-d39d5-5" })]
        public void FindAllImageType2Fields(string number, string[] expectedResult)
        {
            // ARRANGE
            var html = File.ReadAllText(TEST_FILES_BASE_PATH + $"FindAllImageType2Fields-case{number}-input.html");

            // ACT
            var result = MuseTemplateParser.FindAllImageType2Fields(html);

            // ASSERT
            var simplifiedResult = result.Select(r => r.FieldName);
            simplifiedResult.ShouldAllBeEquivalentTo(expectedResult);
        }


        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void MarkEditableTextFields(int number)
        {
            // ARRANGE
            var html = File.ReadAllText(TEST_FILES_BASE_PATH + $"MarkEditableTextFields-case{number}-input.html");
            var fieldValues = new[]
            {
                new FieldValue {FieldName = "objekt_rum", Id = 17, Value = "{\"html\":\"4rok\"}"},
            };

            // ACT
            var result = MuseTemplateParser.MarkEditableTextFields(html, fieldValues);

            // ASSERT
            result.Should().Be(File.ReadAllText(
                TEST_FILES_BASE_PATH + $"MarkEditableTextFields-case{number}-output.html"));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void MarkEditableHtmlFields(int number)
        {
            // ARRANGE
            var html = File.ReadAllText(TEST_FILES_BASE_PATH + $"MarkEditableHtmlFields-case{number}-input.html");
            var fieldValues = new[]
            {
                new FieldValue {FieldName = "fritext", Id = 17, Value = "{\"html\":\"<p class=\\\"Brodtext\\\">Test</p>\"}"},
            };

            // ACT
            var result = MuseTemplateParser.MarkEditableHtmlFields(html, fieldValues);

            // ASSERT
            result.Should().Be(File.ReadAllText(
                TEST_FILES_BASE_PATH + $"MarkEditableHtmlFields-case{number}-output.html"));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void MarkEditableImageType1Fields(int number)
        {
            // ARRANGE
            var html = File.ReadAllText(
                TEST_FILES_BASE_PATH + $"MarkEditableImageType1Fields-case{number}-input.html");
            var fieldValues = new[]
            {
                new FieldValue {FieldName = "2_3", Id = 17, Value = "{\"url\":\"http://farm2.pics.objektdata.se/pic/pic.dll/image?url=26301%2fSFDFE4CF213B4EB464CBB361D0CEC725171.jpg&sizex=10000\",\"scale\":1.44296116504854,\"angle\":0,\"x\":0,\"y\":839,\"w\":1189,\"h\":793}"},
            };

            // ACT
            var result = MuseTemplateParser.MarkEditableImageType1Fields(html, fieldValues);

            // ASSERT
            result.Should().Be(File.ReadAllText(
                TEST_FILES_BASE_PATH + $"MarkEditableImageType1Fields-case{number}-output.html"));
        }
    
        [TestCase("1")]
        [TestCase("2")]
        [TestCase("3")]
        [TestCase("CLOWN")]
        [TestCase("MC")]
        public void MarkEditableImageType2Fields(string number)
        {
            // ARRANGE
            var html = File.ReadAllText(
                TEST_FILES_BASE_PATH + $"MarkEditableImageType2Fields-case{number}-input.html");
            var fieldValues = new[]
            {
                new FieldValue {FieldName = "linhai_300b_4x4_eec_2014_r%c3%b6d", Id = 17, Value = "{\"url\":\"http://farm2.pics.objektdata.se/pic/pic.dll/image?url=26301%2fSFDFE4CF213B4EB464CBB361D0CEC725171.jpg&sizex=10000\",\"scale\":1.44296116504854,\"angle\":0,\"x\":0,\"y\":839,\"w\":1189,\"h\":793}"},
                new FieldValue {FieldName = "kymco500_red-510x398-crop-u124", Id = 17, Value = "{\"url\":\"http://farm2.pics.objektdata.se/pic/pic.dll/image?url=26301%2fSFDFE4CF213B4EB464CBB361D0CEC725171.jpg&sizex=10000\",\"scale\":1.44296116504854,\"angle\":0,\"x\":0,\"y\":839,\"w\":1189,\"h\":793}"},
                new FieldValue {FieldName = "arton11015-d39d5-1", Id = 17, Value = "{\"url\":\"http://farm2.pics.objektdata.se/pic/pic.dll/image?url=26301%2fSFDFE4CF213B4EB464CBB361D0CEC725171.jpg&sizex=10000\",\"scale\":1.44296116504854,\"angle\":0,\"x\":0,\"y\":839,\"w\":1189,\"h\":793}"},
                new FieldValue {FieldName = "arton11015-d39d5-4", Id = 17, Value = "{\"url\":\"http://farm2.pics.objektdata.se/pic/pic.dll/image?url=26301%2fSFDFE4CF213B4EB464CBB361D0CEC725171.jpg&sizex=10000\",\"scale\":1.44296116504854,\"angle\":0,\"x\":0,\"y\":839,\"w\":1189,\"h\":793}"},
                new FieldValue {FieldName = "5910_cc0d17a8", Id = 17, Value = "{\"url\":\"http://farm2.pics.objektdata.se/pic/pic.dll/image?url=26301%2fSFDFE4CF213B4EB464CBB361D0CEC725171.jpg&sizex=10000\",\"scale\":1.44296116504854,\"angle\":0,\"x\":0,\"y\":839,\"w\":1189,\"h\":793}"},
                new FieldValue {FieldName = "kymco500_red-510x398", Id = 19, Value = "{\"url\":\"http://farm2.pics.objektdata.se/pic/pic.dll/image?url=26301%2fSFDFE4CF213B4EB464CBB361D0CEC725171.jpg&sizex=10000\",\"scale\":0.44296116504854,\"angle\":0,\"x\":0,\"y\":839,\"w\":1189,\"h\":793}"},
                new FieldValue {FieldName = "linhai_300b_4x4_eec_2014_r%c3%b6d202x198-2", Id = 17, Value = "{\"url\":\"http://farm2.pics.objektdata.se/pic/pic.dll/image?url=26301%2fSFDFE4CF213B4EB464CBB361D0CEC725171.jpg&sizex=10000\",\"scale\":1.44296116504854,\"angle\":0,\"x\":0,\"y\":839,\"w\":1189,\"h\":793}"},
                new FieldValue {FieldName = "5910_cc0d17a8-crop-u121", Id = 17, Value = "{\"url\":\"http://farm2.pics.objektdata.se/pic/pic.dll/image?url=26301%2fSFDFE4CF213B4EB464CBB361D0CEC725171.jpg&sizex=10000\",\"scale\":1.44296116504854,\"angle\":0,\"x\":0,\"y\":839,\"w\":1189,\"h\":793}"},
                new FieldValue {FieldName = "kymco500_red-510x398-crop-u124", Id = 17, Value = "{\"url\":\"http://farm2.pics.objektdata.se/pic/pic.dll/image?url=26301%2fSFDFE4CF213B4EB464CBB361D0CEC725171.jpg&sizex=10000\",\"scale\":1.44296116504854,\"angle\":0,\"x\":0,\"y\":839,\"w\":1189,\"h\":793}"},
                new FieldValue {FieldName = "linhai_300b_4x4_eec_2014_r%c3%b6d207x148", Id = 17, Value = "{\"url\":\"http://farm2.pics.objektdata.se/pic/pic.dll/image?url=26301%2fSFDFE4CF213B4EB464CBB361D0CEC725171.jpg&sizex=10000\",\"scale\":1.44296116504854,\"angle\":0,\"x\":0,\"y\":839,\"w\":1189,\"h\":793}"},
                new FieldValue {FieldName = "5910_cc0d17a8-crop-u140", Id = 17, Value = "{\"url\":\"http://farm2.pics.objektdata.se/pic/pic.dll/image?url=26301%2fSFDFE4CF213B4EB464CBB361D0CEC725171.jpg&sizex=10000\",\"scale\":1.44296116504854,\"angle\":0,\"x\":0,\"y\":839,\"w\":1189,\"h\":793}"},
                new FieldValue {FieldName = "kymco500_red-510x398-crop-u143", Id = 17, Value = "{\"url\":\"http://farm2.pics.objektdata.se/pic/pic.dll/image?url=26301%2fSFDFE4CF213B4EB464CBB361D0CEC725171.jpg&sizex=10000\",\"scale\":1.44296116504854,\"angle\":0,\"x\":0,\"y\":839,\"w\":1189,\"h\":793}"},
                new FieldValue {FieldName = "", Id = 17, Value = "{\"url\":\"http://farm2.pics.objektdata.se/pic/pic.dll/image?url=26301%2fSFDFE4CF213B4EB464CBB361D0CEC725171.jpg&sizex=10000\",\"scale\":1.44296116504854,\"angle\":0,\"x\":0,\"y\":839,\"w\":1189,\"h\":793}"},
            };

            // ACT
            var result = MuseTemplateParser.MarkEditableImageType2Fields(html, fieldValues);

            // ASSERT
            result.Should().Be(File.ReadAllText(
                TEST_FILES_BASE_PATH + $"MarkEditableImageType2Fields-case{number}-output.html"));
        }
    }
}
