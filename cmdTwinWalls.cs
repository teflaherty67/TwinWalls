using TwinWalls.Common;

namespace TwinWalls
{
    [Transaction(TransactionMode.Manual)]
    public class cmdTwinWalls : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            try
            {                
                // prompt the user to select the walls
                IList<Reference> selectedWalls = uidoc.Selection.PickObjects(ObjectType.Element, new WallSelectionFilter(), "Select walls to twin.");

                // notify the user if no walls were selected
                if (selectedWalls == null || selectedWalls.Count == 0)
                {
                    Utils.TaskDialogError("Error", "Twin Walls", "No Walls Selected");
                    return Result.Cancelled;
                }

                // filter for exterior walls
                IList<Wall> exteriorWalls = new List<Wall>();          

                foreach (Reference wallRef in selectedWalls)
                {
                    Wall curWall = curDoc.GetElement(wallRef) as Wall;

                    if (curWall != null && IsExteriorWall(curWall))
                    {
                        exteriorWalls.Add(curWall);
                    }
                }

                // notify the user if no exterior walls were found
                if (exteriorWalls.Count == 0)
                {
                    Utils.TaskDialogWarning("Warning", "Twin Walls", "No exterior walls found");
                    return Result.Cancelled;
                }

                // create & start transaction
                using(Transaction t = new Transaction(curDoc, "Twin Walls"))
                {
                    t.Start();

                    // loop through the walls and perform the twin operation
                    foreach (Wall originalWall in exteriorWalls)
                    {
                        ConvertToTwinWalls(curDoc, originalWall);
                    }

                    // commit the transaction
                    t.Commit();
                }

                // notify the user
                Utils.TaskDialogInformation("Information", "Twin Walls", $"Revit found {exteriorWalls.Count} exterior walls.");            
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Utils.TaskDialogError("Error", "TwinWalls", $"An error occurred while creating the selection filter: {ex.Message}");
                return Result.Failed;
            }
        }        

        private class WallSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                return elem is Wall; // Allows only Wall elements to be selected
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                // For selecting walls, we are primarily interested in the element itself.
                // If you need to allow selection based on faces or edges of a wall,
                // you might adjust this method and potentially the PickObject/PickObjects call.
                return false;
            }
        }

        private bool IsExteriorWall(Wall wall)
        {
            WallType wallType = wall.WallType;
            if (wallType.Function == WallFunction.Exterior)
            {
                return true;
            }

            // If the wall is not of type Exterior, return false   
            return false;
        }

        private void ConvertToTwinWalls(Document curDoc, Wall originalWall)
        {
            // get wall type of the original wall
            WallType originalWallType = originalWall.WallType;

            // get corresponding twin wall types
            WallType structuralWallType = GetStructuralWallType(curDoc, originalWallType);
            WallType architecturalWallType = GetArchitecturalWallType(curDoc, originalWallType);

            // null check for wall types
            if (structuralWallType == null || architecturalWallType == null)
            {
                Utils.TaskDialogWarning("Warning", "Twin Walls", $"Could not find corresponding twin wall types for: {originalWallType.Name}.");
                return;
            }

            // save original wall offset and extension parameter values
            double originalBaseOffset = originalWall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble();
            double originalTopOffset = originalWall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).AsDouble();
            double originalBaseExtension = originalWall.get_Parameter(BuiltInParameter.WALL_BOTTOM_EXTENSION_DIST_PARAM).AsDouble();
            double originalTopExtension = originalWall.get_Parameter(BuiltInParameter.WALL_TOP_EXTENSION_DIST_PARAM).AsDouble();

            // set/verify original wall location line
            Parameter paramLocationLine = originalWall.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM);
            paramLocationLine.Set((int)WallLocationLine.CoreExterior);

            // replace original wall with structural wall
            originalWall.WallType = structuralWallType;

            // set Base Offset to 0
            Parameter baseOffsetParam = originalWall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET);
            baseOffsetParam.Set(0.0);

            // set Base Extension Distance to 0
            Parameter baseExtensionParam = originalWall.get_Parameter(BuiltInParameter.WALL_BOTTOM_EXTENSION_DIST_PARAM);
            baseExtensionParam.Set(0.0);

            // Set Top Offset to 0
            Parameter topOffsetParam = originalWall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET);
            topOffsetParam.Set(0.0);

            // Set Top Extension Distance to 0
            Parameter topExtensionParam = originalWall.get_Parameter(BuiltInParameter.WALL_TOP_EXTENSION_DIST_PARAM);
            topExtensionParam.Set(0.0);
        }

        private WallType GetStructuralWallType(Document doc, WallType originalType)
        {
            string structuralName = ParseStructuralWallName(originalType.Name);
            if (!string.IsNullOrEmpty(structuralName))
            {
                return GetWallTypeByName(doc, structuralName);
            }
            return null;
        }

        private WallType GetArchitecturalWallType(Document doc, WallType originalType)
        {
            string architecturalName = ParseArchitecturalWallName(originalType.Name);
            if (!string.IsNullOrEmpty(architecturalName))
            {
                return GetWallTypeByName(doc, architecturalName);
            }
            return null;
        }

        private string ParseStructuralWallName(string originalWallName)
        {
            // Pattern to match stud sizes like 2x4, 2x6, 2x8, etc.
            var studPattern = @"\b(\d+x\d+)\b";
            var match = System.Text.RegularExpressions.Regex.Match(originalWallName, studPattern);

            if (match.Success)
            {
                string studSize = match.Groups[1].Value;
                return $"{studSize},GWB";
            }

            return null;
        }

        private string ParseArchitecturalWallName(string originalWallName)
        {
            // Extract everything before the stud size and replace with "Shthng"
            var studPattern = @",\d+x\d+,GWB";
            string architecturalPortion = System.Text.RegularExpressions.Regex.Replace(originalWallName, studPattern, ",Shthng");

            // Only return if we actually made a replacement (meaning it was an exterior wall)
            if (architecturalPortion != originalWallName)
            {
                return architecturalPortion;
            }

            return null;
        }

        private WallType GetWallTypeByName(Document doc, string name)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            return collector.OfClass(typeof(WallType))
                           .Cast<WallType>()
                           .FirstOrDefault(wt => wt.Name == name);
        }

        private void CopyWallParameters(Wall source, Wall target)
        {
            // Copy common parameters
            Parameter sourceHeight = source.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
            Parameter targetHeight = target.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
            if (sourceHeight != null && targetHeight != null && !targetHeight.IsReadOnly)
            {
                targetHeight.Set(sourceHeight.AsDouble());
            }

            Parameter sourceOffset = source.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET);
            Parameter targetOffset = target.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET);
            if (sourceOffset != null && targetOffset != null && !targetOffset.IsReadOnly)
            {
                targetOffset.Set(sourceOffset.AsDouble());
            }

            // Add more parameter copying as needed
        }

        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            Common.ButtonDataClass myButtonData = new Common.ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 1");

            return myButtonData.Data;
        }
    }
}
