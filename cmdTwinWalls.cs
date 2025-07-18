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

                // notify the user
                Utils.TaskDialogInformation("Information", "Twin Walls", $"Revit found {exteriorWalls.Count} exterior walls.");            
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"An error occurred while creating the selection filter: {ex.Message}");
                return Result.Failed;
            }

            return Result.Succeeded;
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
