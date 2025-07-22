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

        public clsExtHostedGenericModels(string familyName, FamilySymbol familyType)
        {
            FamilyName = familyName;
            FamilyType = familyType;
        }
    }
}
