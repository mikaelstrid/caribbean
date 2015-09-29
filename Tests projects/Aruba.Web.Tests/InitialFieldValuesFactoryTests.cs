using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Caribbean.Aruba.Web.Business;
using Caribbean.DataAccessLayer.RealEstateObjects;
using Caribbean.DataContexts.Application.Migrations;
using Caribbean.Models.RealEstateObjects;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Caribbean.Aruba.Web.Tests
{
    public class InitialFieldValuesFactoryTests
    {
        private readonly Mock<IVitecObjectFactory> _mockVitecObjectFactory = new Mock<IVitecObjectFactory>();
        private readonly IReadOnlyDictionary<string, string> _valueMappings;
        private readonly VitecObjectDetails _vitecObjectDetails;
        
        public InitialFieldValuesFactoryTests()
        {
            var valueMappings = ConfigurationDataGenerator.GetPlaceholderMappings().ToDictionary(m => m.Name, m => m.Path);
            valueMappings.Add("ExistingDummy", "/NONEXISTING/XPATH");
            _valueMappings = valueMappings;

            _vitecObjectDetails = new VitecObjectDetails { XDocument = XDocument.Load("..\\..\\TestFiles\\OBJ22998_1431132504.xml") };
        }


        // === CreateInitialTextFieldValue ===
        [TestCase("obj_gata", "Långenäs 141")]
        [TestCase("obj_omrade", "Mölnlycke, Råda, Härryda")]
        [TestCase("obj_rum", "10")]
        public void CreateInitialTextFieldValue_ShouldReturnCorrectValue(string fieldName, string expectedValueFromXml)
        {
            // ARRANGE

            // ACT
            var result = InitialFieldValuesFactory.CreateInitialTextFieldValue(
                new TextFieldInfo { FieldName = fieldName }, 
                _vitecObjectDetails, 
                _valueMappings);

            // ASSERT
            result.Value.Should().Be(JsonConvert.SerializeObject(new { html = expectedValueFromXml }));
        }



        // === CreateInitialImageFieldValue ===
        [Test]
        public void CreateInitialImageFieldValue_ShouldReturnNull_IfFieldNameIsUnknown()
        {
            // ARRANGE
            var imageFieldInfo = new ImageFieldInfo { FieldName = "UnknownFieldName" };

            // ACT
            var result = InitialFieldValuesFactory.CreateInitialImageFieldValue(_mockVitecObjectFactory.Object, imageFieldInfo, _vitecObjectDetails, _valueMappings);
            
            // ASSERT
            result.Should().BeNull();
        }

        [Test]
        public void CreateInitialImageFieldValue_ShouldReturnNull_IfXpathHasNoHits()
        {
            // ARRANGE
            var imageFieldInfo = new ImageFieldInfo { FieldName = "ExistingDummy" };

            // ACT
            var result = InitialFieldValuesFactory.CreateInitialImageFieldValue(_mockVitecObjectFactory.Object, imageFieldInfo, _vitecObjectDetails, _valueMappings);

            // ASSERT
            result.Should().BeNull();
        }

        [Test]
        public void CreateInitialImageFieldValue_ShouldReturnNull_IfFactoryReturnsNull()
        {
            // ARRANGE
            var imageFieldInfo = new ImageFieldInfo { FieldName = "obj_gata" };
            _mockVitecObjectFactory.Setup(m => m.CreateImage(It.IsAny<XElement>())).Returns((VitecObjectImage) null);

            // ACT
            var result = InitialFieldValuesFactory.CreateInitialImageFieldValue(_mockVitecObjectFactory.Object, imageFieldInfo, _vitecObjectDetails, _valueMappings);

            // ASSERT
            result.Should().BeNull();
        }

        [Test]
        public void CreateInitialImageFieldValue_ShouldReturnAFieldValue_IfFactoryReturnsImage()
        {
            // ARRANGE
            var imageFieldInfo = new ImageFieldInfo { FieldName = "obj_gata" };
            _mockVitecObjectFactory.Setup(m => m.CreateImage(It.IsAny<XElement>())).Returns(new VitecObjectImage { ImageUrlWithoutSizeParameters = "http://www.test.com"});

            // ACT
            var result = InitialFieldValuesFactory.CreateInitialImageFieldValue(_mockVitecObjectFactory.Object, imageFieldInfo, _vitecObjectDetails, _valueMappings);

            // ASSERT
            result.Should().NotBeNull();
        }
    }
}
