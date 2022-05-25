using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Prism.Commands;
using System;
using System.Collections.Generic;
using RevitAPITrainingLibrary;

namespace RevitAPITR6._1
{
    internal class MainViewViewModel
    {
        private ExternalCommandData commandData;
        public List<DuctType> DuctTypes { get; } = new List<DuctType>();
        public List<Level> Levels { get; } = new List<Level>();
        public DelegateCommand SaveCommand { get; }
        public double DuctWidth { get; set; }
        public List<XYZ> Points { get; } = new List<XYZ>();
        public DuctType SelectedDuctType { get; set; }
        public Level SelectedLevel { get; set; }

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            DuctTypes = DuctUtils.GetDuctTypes(commandData);
            Levels = LevelUtils.GetLevels(commandData);
            SaveCommand = new DelegateCommand(OnSaveCommand);
            DuctWidth = 100;
            Points = SelectionUtils.GetPionts(commandData, "Выберите точки", ObjectTypes.Endpoints);
        }

        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (Points.Count < 2 || 
                SelectedDuctType = null ||
                SelectedLevel == null)
                return;

            var curves = new List<Curve>();
            for (int i=0;i<Points.Count;i++)
            {
                if (i == 0)
                    continue;

                var prevPoint = Points[i - 1];
                var currentPoint = Points[i];

                Curve curve = Line.CreateBound(prevPoint, currentPoint);
                curve.Add(curve);
            }

            using (var ts = new Transaction(doc, "Create duct"))
            {
                ts.Start();

                foreach (var curve in curves)
                {
                    Duct.Create(doc, curve, SelectedDuctType.Id, SelectedLevel.Id,
                        UnitUtils.ConvertToInternalUnits(DuctWidth, UnitTypeId.Millimeters), 0, false, false);
                }

                ts.Commit();
            }

            RaiseCloseRequest();
        }


        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
