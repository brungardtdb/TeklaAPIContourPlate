using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
// Tekla Structures Namespaces
using Tekla.Structures.Model;
using T3D = Tekla.Structures.Geometry3d;
using TSMUI = Tekla.Structures.Model.UI;
// Many Tekla classes and methods return these collections
using System.Collections;

namespace TeklaAPIContourPlate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Model currentModel;

        public MainWindow()
        {
            InitializeComponent();

            // try connecting to model
            try
            {
                currentModel = new Model();
            }
            catch
            {
                MessageBox.Show("Model may not be connected.");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Reset workplane to global
            currentModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());

            ArrayList PickedPoints = null;
            TSMUI.Picker myPicker = new TSMUI.Picker();

            try
            {
                PickedPoints = myPicker.PickPoints(Tekla.Structures.Model.UI.Picker.PickPointEnum.PICK_POLYGON);
            }
            catch
            {

                PickedPoints = null;
            }

            if (PickedPoints != null)
            {
                ContourPlate myPlate = new ContourPlate();
                myPlate.AssemblyNumber.Prefix = "P";
                myPlate.AssemblyNumber.StartNumber = 1;
                myPlate.PartNumber.Prefix = "p";
                myPlate.PartNumber.StartNumber = 1;
                myPlate.Name = "Plate";
                myPlate.Profile.ProfileString = "PL25.4";
                myPlate.Material.MaterialString = "A36";
                myPlate.Finish = "GP";
                myPlate.Class = "9";
                myPlate.Position.Depth = Position.DepthEnum.FRONT;

                foreach (T3D.Point ThisPoint in PickedPoints)
                {
                    myPlate.AddContourPoint(new ContourPoint(ThisPoint, new Chamfer(12.7, 12.7, Chamfer.ChamferTypeEnum.CHAMFER_LINE)));
                }

                if(!myPlate.Insert())
                {
                    Tekla.Structures.Model.Operations.Operation.DisplayPrompt("No plate was created.");
                }
                else
                {
                    // Change the workplane to match coordinate system of plate
                    currentModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane(myPlate.GetCoordinateSystem()));

                    // Show the plate in the model and show the workplane change
                    currentModel.CommitChanges();

                    // This gets the plates coordinates and information in the current workplane.
                    myPlate.Select();

                    // Draw the coordinate of the plate in the model in hte local coordinate system. 
                    TSMUI.GraphicsDrawer myDrawer = new TSMUI.GraphicsDrawer();

                    foreach (ContourPoint ContourPoint in myPlate.Contour.ContourPoints)
                    {
                        T3D.Point CornerPoint = new T3D.Point(ContourPoint.X, ContourPoint.Y, ContourPoint.Z);

                        const double  IMPERIALUNIT = 25.4;
                        double XValue = Math.Round(CornerPoint.X / IMPERIALUNIT, 4);
                        double YValue = Math.Round(CornerPoint.Y / IMPERIALUNIT, 4);
                        double ZValue = Math.Round(CornerPoint.Z / IMPERIALUNIT, 4);

                        myDrawer.DrawText(CornerPoint, "(" + XValue + "," + YValue + "," + ZValue + ")", new TSMUI.Color(1, 0, 0));
                        myDrawer.DrawLineSegment(new T3D.LineSegment(new T3D.Point(0, 0, 0), new T3D.Point(0, 0, 500)), new TSMUI.Color(1, 0, 0));


                    }
                }

            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // declaration for object user will select
            ModelObject pickedObject = null;

            // New picker for user interface
            TSMUI.Picker picker = new TSMUI.Picker();

            try
            {
                // if user picks an object
                pickedObject = picker.PickObject(TSMUI.Picker.PickObjectEnum.PICK_ONE_OBJECT);
            }
            catch
            {
                // if user interrupts
                pickedObject = null;
                MessageBox.Show("No object was selected.");
            }
            finally
            {
                if (pickedObject != null)
                {
                    // get coordinate system for object
                    T3D.CoordinateSystem objectSystem = pickedObject.GetCoordinateSystem();

                    // set transformation plane to picked object coordinate system
                    currentModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane(objectSystem));

                    currentModel.CommitChanges();
                }
            }
        }
    }
}
