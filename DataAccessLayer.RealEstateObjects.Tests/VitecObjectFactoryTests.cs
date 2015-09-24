using Caribbean.DataAccessLayer.RealEstateObjects;
using Caribbean.Models.RealEstateObjects;
using FluentAssertions;
using NUnit.Framework;

namespace DataAccessLayer.RealEstateObjects.Tests
{
    public class VitecObjectFactoryTests
    {
        [TestCase("190", ObjectStatus.Coming)]
        [TestCase("200", ObjectStatus.ForSale)]
        [TestCase("220", ObjectStatus.ForSale)]
        [TestCase("240", ObjectStatus.ForSale)]
        [TestCase("300", ObjectStatus.ForSale)]
        [TestCase("320", ObjectStatus.ForSale)]
        [TestCase("340", ObjectStatus.ForSale)]
        [TestCase("350", ObjectStatus.ForSale)]
        [TestCase("510", ObjectStatus.Reference)]
        public void ParseObjectStatus_ShouldReturnComing_IfInputIs190(string kind, ObjectStatus expected)
        {
            // ARRANGE

            // ACT
            var result = VitecObjectFactory.ParseObjectStatus(kind);

            // ASSERT
            result.Should().Be(expected);
        }

    }
}
