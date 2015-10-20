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

        [TestCase(1, 0)]
        [TestCase(2, 1)]
        [TestCase(3, 2)]
        [TestCase(4, 3)]
        public void FindAllTextFields(int number, int expectedResultIndex)
        {
            var expectedResults = new[]
            {
                new [] { new TextFieldInfo {FieldName = "field1", FieldTemplate = "{objekt_rum}" }, new TextFieldInfo {FieldName = "field2", FieldTemplate = "{objekt_storlek}" } },
                new [] { new TextFieldInfo {FieldName = "field1", FieldTemplate = "{objekt_rum}" }, new TextFieldInfo {FieldName = "field2", FieldTemplate = "{objekt_storlek}" } },
                new TextFieldInfo[0],
                new []
                {
                    new TextFieldInfo {FieldName = "field0", FieldTemplate = "{objekt_gata}" },
                    new TextFieldInfo {FieldName = "field1", FieldTemplate = "{objekt_rum}" },
                    new TextFieldInfo {FieldName = "field2", FieldTemplate = "{objekt_storlek}" },
                    new TextFieldInfo {FieldName = "field3", FieldTemplate = "{kontorets_adress}" },
                    new TextFieldInfo {FieldName = "field4", FieldTemplate = "{kontorets_tel}" },
                    new TextFieldInfo {FieldName = "field5", FieldTemplate = "{kontorets_mail}" },
                    new TextFieldInfo {FieldName = "field6", FieldTemplate = "{kontorets_www}" },
                    new TextFieldInfo {FieldName = "field7", FieldTemplate = "{ansvarig_maklare}" },
                    new TextFieldInfo {FieldName = "field8", FieldTemplate = "{ansvarig_maklare_tel}" },
                    new TextFieldInfo {FieldName = "field9", FieldTemplate = "{ansvarig_maklare_mobil}" },
                    new TextFieldInfo {FieldName = "field10", FieldTemplate = "{ansvarig_maklare_mail}" },
                    new TextFieldInfo {FieldName = "field11", FieldTemplate = "{extra_kontaktperson}" },
                    new TextFieldInfo {FieldName = "field12", FieldTemplate = "{extra_kontaktperson_tel}" },
                    new TextFieldInfo {FieldName = "field13", FieldTemplate = "{extra_kontaktperson_mobil}" },
                    new TextFieldInfo {FieldName = "field14", FieldTemplate = "{kontorets_mail}" }
                },
            };

            // ARRANGE
            var html = File.ReadAllText(TEST_FILES_BASE_PATH + $"FindAllTextFields-case{number}-input.html");

            // ACT
            var result = MuseTemplateParser.FindAllTextFields(html);

            // ASSERT
            result.ShouldAllBeEquivalentTo(expectedResults[expectedResultIndex]);
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
            var simplifiedExpected = expectedFieldNames.Zip(expectedFirstParagraphClasses, (f, s) => new HtmlFieldInfo { FieldName = f, FirstParagraphClass = s });
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
            var result = MuseTemplateParser.FindAllImageType1Fields(html, MuseTemplateParser.REGEX_REAL_ESTATE_OBJECT_IMAGE_FIELDS_TYPE1);

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
            var result = MuseTemplateParser.FindAllImageType2Fields(html, MuseTemplateParser.REGEX_REAL_ESTATE_OBJECT_IMAGE_FIELDS_TYPE2);

            // ASSERT
            var simplifiedResult = result.Select(r => r.FieldName);
            simplifiedResult.ShouldAllBeEquivalentTo(expectedResult);
        }


        [TestCase("1", new[] { "2_3" })]
        public void FindAllStaffImageType1Fields(string number, string[] expectedResult)
        {
            // ARRANGE
            var html = File.ReadAllText(TEST_FILES_BASE_PATH + $"FindAllStaffImageType1Fields-case{number}-input.html");

            // ACT
            var result = MuseTemplateParser.FindAllImageType1Fields(html, MuseTemplateParser.REGEX_STAFF_IMAGE_FIELDS_TYPE1);

            // ASSERT
            var simplifiedResult = result.Select(r => r.FieldName);
            simplifiedResult.ShouldAllBeEquivalentTo(expectedResult);
        }


        [TestCase("1", new[] { "linhai_300b_4x4_eec_2014_r%c3%b6d" })]
        public void FindAllStaffImageType2Fields(string number, string[] expectedResult)
        {
            // ARRANGE
            var html = File.ReadAllText(TEST_FILES_BASE_PATH + $"FindAllStaffImageType2Fields-case{number}-input.html");

            // ACT
            var result = MuseTemplateParser.FindAllImageType2Fields(html, MuseTemplateParser.REGEX_STAFF_IMAGE_FIELDS_TYPE2);

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
                new FieldValue {FieldName = "field1", Id = 17, Value = "{\"html\":\"4rok\"}"},
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
            var result = MuseTemplateParser.MarkEditableImageType1Fields(html, fieldValues, MuseTemplateParser.REGEX_REAL_ESTATE_OBJECT_IMAGE_FIELDS_TYPE1, "realestateobject");

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
            var result = MuseTemplateParser.MarkEditableImageType2Fields(html, fieldValues, MuseTemplateParser.REGEX_REAL_ESTATE_OBJECT_IMAGE_FIELDS_TYPE2, "realestateobject");

            // ASSERT
            result.Should().Be(File.ReadAllText(
                TEST_FILES_BASE_PATH + $"MarkEditableImageType2Fields-case{number}-output.html"));
        }


        [TestCase(1)]
        [TestCase(2)]
        public void MarkEditableStaffImageType1Fields(int number)
        {
            // ARRANGE
            var html = File.ReadAllText(TEST_FILES_BASE_PATH + $"MarkEditableStaffImageType1Fields-case{number}-input.html");
            var fieldValues = new[]
            {
                new FieldValue {FieldName = "2_3", Id = 17, Value = "{\"url\":\"http://farm2.pics.objektdata.se/pic/pic.dll/image?url=26301%2fSFDFE4CF213B4EB464CBB361D0CEC725171.jpg&sizex=10000\",\"scale\":1.44296116504854,\"angle\":0,\"x\":0,\"y\":839,\"w\":1189,\"h\":793}"},
            };

            // ACT
            var result = MuseTemplateParser.MarkEditableImageType1Fields(html, fieldValues, MuseTemplateParser.REGEX_STAFF_IMAGE_FIELDS_TYPE1, "staff");

            // ASSERT
            result.Should().Be(File.ReadAllText(TEST_FILES_BASE_PATH + $"MarkEditableStaffImageType1Fields-case{number}-output.html"));
        }

        [TestCase("1")]
        public void MarkEditableStaffImageType2Fields(string number)
        {
            // ARRANGE
            var html = File.ReadAllText(TEST_FILES_BASE_PATH + $"MarkEditableStaffImageType2Fields-case{number}-input.html");
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
            var result = MuseTemplateParser.MarkEditableImageType2Fields(html, fieldValues, MuseTemplateParser.REGEX_REAL_ESTATE_OBJECT_IMAGE_FIELDS_TYPE2, "staff");

            // ASSERT
            result.Should().Be(File.ReadAllText(
                TEST_FILES_BASE_PATH + $"MarkEditableStaffImageType2Fields-case{number}-output.html"));
        }


        [TestCase(1)]
        public void MarkAllFields(int number)
        {
            // ARRANGE
            var html = File.ReadAllText(TEST_FILES_BASE_PATH + $"MarkAllFields-case{number}-input.html");
            var fieldValues = new[]
            {
                new FieldValue {FieldName = "2_3", Id = 17, Value = "{\"url\":\"http://farm2.pics.objektdata.se/pic/pic.dll/image?url=26301%2fSFDFE4CF213B4EB464CBB361D0CEC725171.jpg&sizex=10000\",\"scale\":1.44296116504854,\"angle\":0,\"x\":0,\"y\":839,\"w\":1189,\"h\":793}"},
                new FieldValue {FieldName = "3_3", Id = 18, Value = "{\"url\":\"http://farm2.pics.objektdata.se/pic/pic.dll/image?url=26301%2fSFDFE4CF213B4EB464CBB361D0CEC725171.jpg&sizex=10000\",\"scale\":1.44296116504854,\"angle\":0,\"x\":0,\"y\":839,\"w\":1189,\"h\":793}"},
            };
            var sut = new MuseTemplateParser();

            // ACT
            var result = sut.MarkAllFields(html, fieldValues);

            // ASSERT
            result.Should().Be(File.ReadAllText(TEST_FILES_BASE_PATH + $"MarkAllFields-case{number}-output.html"));

        }
    }
}
