﻿using System;
using System.Linq;
using System.Xml.Linq;
using Caribbean.Models.RealEstateObjects;

namespace Caribbean.DataAccessLayer.RealEstateObjects
{
    public interface IVitecObjectFactory
    {
        VitecObjectSummary CreateSummary(XElement objectElement);
        VitecObjectDetails CreateDetails(string xml);
        VitecObjectImage CreateImage(XElement pictureElement);
    }

    public class VitecObjectFactory : IVitecObjectFactory
    {
        public VitecObjectSummary CreateSummary(XElement objectElement)
        {
            return new VitecObjectSummary
            {
                Id = objectElement.Element("GID").Value,
                Address = objectElement.Element("Adress").Value,
                ThumbnailUrl = CreateSummaryThumbnailUrl(objectElement.Element("BildUrl").Value, width: 150),
                Status = objectElement.Element("Kind").Value,
                Price = ConvertToInt(objectElement.Element("Pris").Value),
            };
        }

        private static int? ConvertToInt(string s)
        {
            int value;
            return int.TryParse(s.Replace(".", "").Replace(" ", ""), out value) ? (int?) value : null;
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
            return new VitecObjectDetails
            {
                XDocument = xmlDocument,
                Id = objectElement.Attribute("gid").Value,
                Address = objectElement.Element("msadress").Value,
                Images = objectElement.Descendants("picture").Select(CreateImage)
            };
        }


        public VitecObjectImage CreateImage(XElement pictureElement)
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
    }
}
