using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Caribbean.Models.RealEstateObjects
{
    public class VitecObjectDetails : VitecObjectBase
    {
        public XDocument XDocument { get; set; }

        public IEnumerable<VitecObjectImage> ObjectImages { get; set; }
        public IEnumerable<VitecStaffImage> StaffImages { get; set; }

        public string GetElementValue(string xpath)
        {
            if (string.IsNullOrWhiteSpace(xpath)) return null;
            var element = XDocument.XPathSelectElement(xpath);
            return element?.Value;
        }
        
        public XElement GetElement(string xpath)
        {
            return !string.IsNullOrWhiteSpace(xpath) ? XDocument.XPathSelectElement(xpath) : null;
        }
    }
}