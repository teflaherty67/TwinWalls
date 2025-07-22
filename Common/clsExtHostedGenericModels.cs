using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwinWalls.Common
{
    internal class clsExtHostedGenericModels
    {
        public string FamilyName { get; set; }
        public FamilySymbol FamilyType { get; set; }
        public XYZ Location { get; set; }
        public ElementId HostWallId { get; set; }
        public ElementId LevelId { get; set; }
        public double ElevFromLevel { get; set; }
        public double Length { get; set; }
        public double Extension { get; set; }
        public double ExtensionTop { get; set; }
        public double ExtensionBottom { get; set; }
        public double Projection { get; set; }
        public XYZ StartPoint { get; set; }
        public XYZ EndPoint { get; set; }


        // Constructor to initialize the properties

        public clsExtHostedGenericModels(string familyName, FamilySymbol familyType)
        {
            FamilyName = familyName;
            FamilyType = familyType;
        }

        // constructor for Masonry-Horizontal family
        public clsExtHostedGenericModels(string familyName, FamilySymbol familyType, XYZ location, ElementId hostWallId,
            ElementId levelId, double elevFromLevel, double length, double extension, double projection)
        {
            FamilyName = familyName;
            FamilyType = familyType;
            Location = location;
            HostWallId = hostWallId;
            LevelId = levelId;
            ElevFromLevel = elevFromLevel;
            Length = length;
            Extension = extension;
            Projection = projection;
        }

        // constructor for Masonry-Vertical family
        public clsExtHostedGenericModels(string familyName, FamilySymbol familyType, XYZ location, ElementId hostWallId,
            ElementId levelId, double elevFromLevel, double length, double extensionTop, double extensionBottom, double projection)
        {
            FamilyName = familyName;
            FamilyType = familyType;
            Location = location;
            HostWallId = hostWallId;
            LevelId = levelId;
            ElevFromLevel = elevFromLevel;
            Length = length;
            ExtensionTop = extensionTop;
            ExtensionBottom = extensionBottom;
            Projection = projection;
        }

        // constructor for brick sills
        public clsExtHostedGenericModels(string familyName, FamilySymbol familyType, XYZ location, ElementId hostWallId,
            ElementId levelId, double elevFromLevel, double length, double extension)
        {
            FamilyName = familyName;
            FamilyType = familyType;
            Location = location;
            HostWallId = hostWallId;
            LevelId = levelId;
            ElevFromLevel = elevFromLevel;
            Length = length;
            Extension = extension;
        }
    }
}
