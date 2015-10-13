using Caribbean.DataAccessLayer.RealEstateObjects;
using Caribbean.Models.RealEstateObjects;
using FluentAssertions;
using NUnit.Framework;

namespace DataAccessLayer.RealEstateObjects.Tests
{
    public class VitecObjectFactoryTests
    {
        [TestCase("190", "0", ObjectStatus.Coming)]
        [TestCase("200", "0", ObjectStatus.ForSale)]
        [TestCase("220", "0", ObjectStatus.ForSale)]
        [TestCase("240", "0", ObjectStatus.ForSale)]
        [TestCase("300", "0", ObjectStatus.ForSale)]
        [TestCase("320", "0", ObjectStatus.ForSale)]
        [TestCase("340", "0", ObjectStatus.ForSale)]
        [TestCase("350", "0", ObjectStatus.ForSale)]
        [TestCase("510", "0", ObjectStatus.Reference)]
        [TestCase("190", "1", ObjectStatus.Coming)]
        [TestCase("200", "1", ObjectStatus.Coming)]
        [TestCase("220", "1", ObjectStatus.Coming)]
        [TestCase("240", "1", ObjectStatus.Coming)]
        [TestCase("300", "1", ObjectStatus.Coming)]
        [TestCase("320", "1", ObjectStatus.Coming)]
        [TestCase("340", "1", ObjectStatus.Coming)]
        [TestCase("350", "1", ObjectStatus.Coming)]
        [TestCase("510", "1", ObjectStatus.Coming)]
        [TestCase("190", "False", ObjectStatus.Coming)]
        [TestCase("200", "False", ObjectStatus.ForSale)]
        [TestCase("220", "False", ObjectStatus.ForSale)]
        [TestCase("240", "False", ObjectStatus.ForSale)]
        [TestCase("300", "False", ObjectStatus.ForSale)]
        [TestCase("320", "False", ObjectStatus.ForSale)]
        [TestCase("340", "False", ObjectStatus.ForSale)]
        [TestCase("350", "False", ObjectStatus.ForSale)]
        [TestCase("510", "False", ObjectStatus.Reference)]
        [TestCase("190", "True", ObjectStatus.Coming)]
        [TestCase("200", "True", ObjectStatus.Coming)]
        [TestCase("220", "True", ObjectStatus.Coming)]
        [TestCase("240", "True", ObjectStatus.Coming)]
        [TestCase("300", "True", ObjectStatus.Coming)]
        [TestCase("320", "True", ObjectStatus.Coming)]
        [TestCase("340", "True", ObjectStatus.Coming)]
        [TestCase("350", "True", ObjectStatus.Coming)]
        [TestCase("510", "True", ObjectStatus.Coming)]
        public void ParseObjectStatus_ShouldReturnComing_IfInputIs190(string kind, string kommandeForsaljning, ObjectStatus expected)
        {
            // ARRANGE

            // ACT
            var result = VitecObjectFactory.ParseObjectStatus(kind, kommandeForsaljning);

            // ASSERT
            result.Should().Be(expected);
        }

    }
}
