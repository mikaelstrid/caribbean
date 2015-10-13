using System;
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
        public void ParseObjectStatus_TestCases(string kind, string kommandeForsaljning, ObjectStatus expected)
        {
            // ARRANGE

            // ACT
            var result = VitecObjectFactory.ParseObjectStatus(kind, kommandeForsaljning);

            // ASSERT
            result.Should().Be(expected);
        }


        [TestCase("den 3 januari 2012 11:06", 2012, 1, 3, 11, 06)]
        [TestCase("den 19 november 2013 10:51", 2013, 11, 19, 10, 51)]
        [TestCase("den 11 maj 2011 14:26", 2011, 5, 11, 14, 26)]
        [TestCase("den 9 februari 2012 13:24", 2012, 2, 9, 13, 24)]
        [TestCase("den 26 augusti 2013 11:59", 2013, 8, 26, 11, 59)]
        [TestCase("den 8 mars 2013 09:39", 2013, 3, 8, 9, 39)]
        public void ParseNyTid_TestCases(string input, int year, int month, int day, int hour, int minute)
        {
            // ARRANGE

            // ACT
            var result = VitecObjectFactory.ParseNyTid(input);

            // ASSERT
            result.Should().Be(new DateTime(year, month, day, hour, minute, 0));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("blablaba")]
        public void ParseNyTid_ShouldReturnDateTimeMin_IfInputIsIllegal(string input)
        {
            // ARRANGE

            // ACT
            var result = VitecObjectFactory.ParseNyTid(input);

            // ASSERT
            result.Should().Be(DateTime.MinValue);
        }

    }
}
