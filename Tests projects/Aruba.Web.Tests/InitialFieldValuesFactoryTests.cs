using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Caribbean.Aruba.Web.Business;
using Caribbean.DataAccessLayer.RealEstateObjects;
using Caribbean.Models.RealEstateObjects;
using FluentAssertions;
using Moq;
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
            _valueMappings = new Dictionary<string, string>
            {
                { "FieldName1", "/XPATH/WITHOUT/HITS" },
                { "FieldName2", "/objects/object" },
            };
            var xDocument = XDocument.Parse("<objects><object></object></objects>");
            _vitecObjectDetails = new VitecObjectDetails { XDocument = xDocument };
        }

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
            var imageFieldInfo = new ImageFieldInfo { FieldName = "FieldName1" };

            // ACT
            var result = InitialFieldValuesFactory.CreateInitialImageFieldValue(_mockVitecObjectFactory.Object, imageFieldInfo, _vitecObjectDetails, _valueMappings);

            // ASSERT
            result.Should().BeNull();
        }

        [Test]
        public void CreateInitialImageFieldValue_ShouldReturnNull_IfFactoryReturnsNull()
        {
            // ARRANGE
            var imageFieldInfo = new ImageFieldInfo { FieldName = "FieldName2" };
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
            var imageFieldInfo = new ImageFieldInfo { FieldName = "FieldName2" };
            _mockVitecObjectFactory.Setup(m => m.CreateImage(It.IsAny<XElement>())).Returns(new VitecObjectImage { ImageUrlWithoutSizeParameters = "http://www.test.com"});

            // ACT
            var result = InitialFieldValuesFactory.CreateInitialImageFieldValue(_mockVitecObjectFactory.Object, imageFieldInfo, _vitecObjectDetails, _valueMappings);

            // ASSERT
            result.Should().NotBeNull();
        }
    }
}
