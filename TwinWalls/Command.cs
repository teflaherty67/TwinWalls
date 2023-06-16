#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

#endregion

namespace TwinWalls
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document curDoc = uidoc.Document;

            // prompt the user to select the walls to "twin"

            // verify all selected elements are walls

            // set the Location Line for all selected walls to Core Face: Exterior

            // if wall name contains 2x4, new interior wall to be 2x4,GWB

            // if wall name contains 2x6, new interior wall to be 2x6,GWB



            // Get the current Revit application and document
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Prompt the user to select walls
            ReferenceList referenceList = uiDoc.Selection.PickObjects(ObjectType.Element, new WallSelectionFilter(), "Select walls");

            // Create a selection set and add the selected walls
            Selection sel = uiDoc.Selection;
            sel.Elements.Clear();
            foreach (Reference reference in referenceList)
            {
                Element element = doc.GetElement(reference);
                sel.Elements.Add(element);
            }

            return Result.Succeeded;

        }

        // Custom selection filter to select only walls
        public class WallSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                return elem is Wall;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }


        public static String GetMethod()
        {
            var method = MethodBase.GetCurrentMethod().DeclaringType?.FullName;
            return method;
        }

    }
}
