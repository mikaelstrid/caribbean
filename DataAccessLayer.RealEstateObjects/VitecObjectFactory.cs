using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Caribbean.Models.RealEstateObjects;
using NLog;

namespace Caribbean.DataAccessLayer.RealEstateObjects
{
    public interface IVitecObjectFactory
    {
        VitecObjectSummary CreateSummary(XElement objectElement);
        VitecObjectDetails CreateDetails(string xml);
        VitecObjectImage CreateObjectImage(XElement pictureElement);
        RealEstateImageBase CreateImage(XElement element);
    }

    public class VitecObjectFactory : IVitecObjectFactory
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly int _thumbnailWidthInPx;

        public VitecObjectFactory()
        {
            if (!int.TryParse(ConfigurationManager.AppSettings["Caribbean.RealEstateObjects.ThumbnailWidthInPx"], out _thumbnailWidthInPx))
            {
                _thumbnailWidthInPx = 508;
            }
        }

        public VitecObjectSummary CreateSummary(XElement objectElement)
        {
            return new VitecObjectSummary
            {
                Id = objectElement.Element("GID").Value,
                Address = objectElement.Element("Adress").Value,
                CityArea = objectElement.Element("Omrade").Value,
                NumberOfRooms = ConvertToDoubleSwedish(objectElement.Element("Rum").Value),
                SquareMeters = ConvertToInt(objectElement.Element("BoArea").Value),
                ThumbnailUrl = CreateSummaryThumbnailUrl(objectElement.Element("BildUrl").Value, width: _thumbnailWidthInPx),
                Status = ParseObjectStatus(objectElement.Element("Kind").Value, objectElement.Element("KommandeForsaljning").Value),
                Price = ConvertToInt(objectElement.Element("Pris").Value),
            };
        }

        internal static int? ConvertToInt(string s)
        {
            int value;
            return int.TryParse(s.Replace(".", "").Replace(" ", ""), out value) ? (int?) value : null;
        }

        internal static double? ConvertToDoubleSwedish(string input)
        {
            if (input == null) return null;
            double value;
            return double.TryParse(input.Replace(".", "").Replace(" ", ""), NumberStyles.Any, new CultureInfo("sv-SE"), out value) ? (double?)value : null;
        }

        private string CreateSummaryThumbnailUrl(string originalUrl, int width)
        {
            if (string.IsNullOrWhiteSpace(originalUrl)) return string.Empty;
            var urlWithoutSizeXAndSizeY = Utilities.RemoveQueryStringParamsByKey(originalUrl, new[] { "sizex", "sizey" });
            return urlWithoutSizeXAndSizeY + "&sizex=" + width;
        }


        public VitecObjectDetails CreateDetails(string xml)
        {
            return CreateDetails(XDocument.Parse(xml));
        }

        private VitecObjectDetails CreateDetails(XDocument xmlDocument)
        {
            if (xmlDocument == null) throw new ArgumentNullException(nameof(xmlDocument));
            var objectElement = xmlDocument.Element("OBJEKT");

            var modifiedTime = ParseNyTid(objectElement.Element("nytid").Value);

            return new VitecObjectDetails
            {
                XDocument = xmlDocument,
                Id = objectElement.Attribute("gid").Value,
                Address = objectElement.Element("msadress").Value,
                ObjectImages = objectElement.Descendants("picture").Select(CreateObjectImage),
                StaffImages = new [] { CreateStaffImage(objectElement.Element("Maklare"), "ma"), CreateStaffImage(objectElement.Element("Extrakontaktperson"), "ek") }.Where(i => i != null),
                CreatedTime = modifiedTime,
            };
        }

        public RealEstateImageBase CreateImage(XElement element)
        {
            if (element.Name == "picture") return CreateObjectImage(element);
            return new VitecStaffImage {ImageUrl = element.Value};
        }

        public VitecObjectImage CreateObjectImage(XElement pictureElement)
        {
            try
            {
                return new VitecObjectImage
                {
                    Pid = pictureElement.Attribute("pid").Value,
                    Name = pictureElement.Element("picnamn").Value,
                    ImageUrlWithoutSizeParameters = Utilities.RemoveQueryStringParamsByKey(pictureElement.Element("picurl").Value, new[] { "sizex", "sizey" }),
                    ThumbnailBaseUrlWithoutSizeParameters = Utilities.RemoveQueryStringParamsByKey(pictureElement.Element("thumburl").Value, new[] { "sizex", "sizey" }),
                    Group = pictureElement.Element("picgrupp").Value,
                    Category = pictureElement.Element("pickategori").Value,
                };

            }
            catch (Exception e)
            {
                Logger.Warn(e, "Error when parsing object image element.");
                return null;
            }
        }

        private static VitecStaffImage CreateStaffImage(XElement staffElement, string elementPrefix)
        {
            try
            {
                if (staffElement == null)
                {
                    Logger.Trace($"Staff element ({elementPrefix}) == null");
                    return null;
                }

                var imageUrl = staffElement.Element(elementPrefix + "BildUrl") ?.Value;
                if (string.IsNullOrWhiteSpace(imageUrl)) return null;

                return new VitecStaffImage { ImageUrl = imageUrl };
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Error when parsing staff element.");
                return null;
            }
        }


        // HELPER METHODS
        internal static ObjectStatus ParseObjectStatus(string kind, string kommandeForsaljning)
        {
            if (kommandeForsaljning == "1" || kommandeForsaljning == "True" || kind.StartsWith("1")) return ObjectStatus.Coming;
            if (kind.StartsWith("2") || kind.StartsWith("3")) return ObjectStatus.ForSale;
            if (kind.StartsWith("5")) return ObjectStatus.Reference;
            return ObjectStatus.Unknown;
        }

        internal static DateTime ParseNyTid(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return DateTime.MinValue;

            DateTime time;
            if (!DateTime.TryParseExact(input, "\\d\\e\\n d MMMM yyyy HH:mm", new CultureInfo("sv-SE"), DateTimeStyles.None, out time)) time = DateTime.MinValue;
            return time;
        }
    }
}
