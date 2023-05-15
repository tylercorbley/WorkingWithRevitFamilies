#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion

namespace WorkingWithRevitFamilies
{
    [Transaction(TransactionMode.Manual)]
    public class PlaceFurnitureInRoom : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // Your code goes here
            //get all rooms
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Rooms);


            List<string[]> furlist = GetFurnitureTypes();
            List<string[]> furSet = GetFurnitureSets();
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Instert Family into rooms");

                //loop through rooms
                foreach (SpatialElement room in collector)
                {
                    int furnCount = 0; //create a counter that will keep track of each furniture placed in each room, it will reset every time we loop to the next room
                    LocationPoint loc = room.Location as LocationPoint;
                    XYZ roomPoint = loc.Point as XYZ;
                    //get the furniture set parameter from the current room
                    string furnSet = GetPArameterValueAsString(room, "Furniture Set");

                    foreach (string[] rmType in furSet) //loop through the furniture set data
                    {
                        if (furnSet == rmType[0]) //if the furniture set parameter is equal to the current furniture set from the list loop, continue
                        {
                            string furn = rmType[2]; //assign the furniture of the current furniture set to a string
                            string[] splitFurn = furn.Split(','); //split that string into an array that i can loop through VVVV
                            foreach (string furnus in splitFurn)
                            {
                                foreach (string[] furbis in furlist) //loop through the furniture list now
                                {
                                    if (furnus.Trim() == furbis[0]) //if the furniture name from the split array we are looping through is equal to the furniture name from the furniture list we are looping through, continue
                                    {
                                        furnCount++;
                                        FamilySymbol curFS = GetFamilySymbolByName(doc, furbis[1], furbis[2]); //use the other items in the funriture list array as strings to get a revit family, active it, then plug it into the the room
                                        curFS.Activate();
                                        FamilyInstance curFI = doc.Create.NewFamilyInstance(roomPoint, curFS, StructuralType.NonStructural);
                                        SetParameterValue(room, "Furniture Count", furnCount);
                                    }
                                }
                            }
                        }
                    }


                }
                t.Commit();
            }

            return Result.Succeeded;
        }
        internal string GetPArameterValueAsString(Element element, string paramName)
        {
            IList<Parameter> paramList = element.GetParameters(paramName);
            Parameter occParam = paramList.First();

            return occParam.AsString();
        }

        internal void SetParameterValue(Element element, string paramName, string value)
        {
            IList<Parameter> paramList = element.GetParameters(paramName);
            Parameter param = paramList.First();

            param.Set(value);
        }

        internal void SetParameterValue(Element element, string paramName, double value)
        {
            IList<Parameter> paramList = element.GetParameters(paramName);
            Parameter param = paramList.First();

            param.Set(value);
        }

        internal FamilySymbol GetFamilySymbolByName(Document doc, string famName, string fsName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(FamilySymbol));

            foreach (FamilySymbol fs in collector)
            {
                if (fs.Name == fsName && fs.FamilyName == famName)
                    return fs;
            }
            return null;
        }
        public static String GetMethod()
        {
            var method = MethodBase.GetCurrentMethod().DeclaringType?.FullName;
            return method;
        }
        private List<string[]> GetFurnitureTypes()
        {
            List<string[]> returnList = new List<string[]>();
            returnList.Add(new string[] { "Furniture Name", "Revit Family Name", "Revit Family Type" });
            returnList.Add(new string[] { "desk", "Desk", "60in x 30in" });
            returnList.Add(new string[] { "task chair", "Chair-Task", "Chair-Task" });
            returnList.Add(new string[] { "side chair", "Chair-Breuer", "Chair-Breuer" });
            returnList.Add(new string[] { "bookcase", "Shelving", "96in x 12in x 84in" });
            returnList.Add(new string[] { "loveseat", "Sofa", "54in" });
            returnList.Add(new string[] { "teacher desk", "Table-Rectangular", "48in x 30in" });
            returnList.Add(new string[] { "student desk", "Desk", "60in x 30in Student" });
            returnList.Add(new string[] { "computer desk", "Table-Rectangular", "48in x 30in" });
            returnList.Add(new string[] { "lab desk", "Table-Rectangular", "72in x 30in" });
            returnList.Add(new string[] { "lounge chair", "Chair-Corbu", "Chair-Corbu" });
            returnList.Add(new string[] { "coffee table", "Table-Coffee", "30in x 60in x 18in" });
            returnList.Add(new string[] { "sofa", "Sofa-Corbu", "Sofa-Corbu" });
            returnList.Add(new string[] { "dining table", "Table-Dining", "30in x 84in x 22in" });
            returnList.Add(new string[] { "dining chair", "Chair-Breuer", "Chair-Breuer" });
            returnList.Add(new string[] { "stool", "Chair-Task", "Chair-Task" });

            return returnList;
        }

        private List<string[]> GetFurnitureSets()
        {
            List<string[]> returnList = new List<string[]>();
            returnList.Add(new string[] { "Furniture Set", "Room Type", "Included Furniture" });
            returnList.Add(new string[] { "A", "Office", "desk, task chair, side chair, bookcase" });
            returnList.Add(new string[] { "A2", "Office", "desk, task chair, side chair, bookcase, loveseat" });
            returnList.Add(new string[] { "B", "Classroom - Large", "teacher desk, task chair, student desk, student desk, student desk, student desk, student desk, student desk, student desk, student desk, student desk, student desk, student desk, student desk" });
            returnList.Add(new string[] { "B2", "Classroom - Medium", "teacher desk, task chair, student desk, student desk, student desk, student desk, student desk, student desk, student desk, student desk" });
            returnList.Add(new string[] { "C", "Computer Lab", "computer desk, computer desk, computer desk, computer desk, computer desk, computer desk, task chair, task chair, task chair, task chair, task chair, task chair" });
            returnList.Add(new string[] { "D", "Lab", "teacher desk, task chair, lab desk, lab desk, lab desk, lab desk, lab desk, lab desk, lab desk, stool, stool, stool, stool, stool, stool, stool" });
            returnList.Add(new string[] { "E", "Student Lounge", "lounge chair, lounge chair, lounge chair, sofa, coffee table, bookcase" });
            returnList.Add(new string[] { "F", "Teacher's Lounge", "lounge chair, lounge chair, sofa, coffee table, dining table, dining chair, dining chair, dining chair, dining chair, bookcase" });
            returnList.Add(new string[] { "G", "Waiting Room", "lounge chair, lounge chair, sofa, coffee table" });

            return returnList;
        }
    }

}
