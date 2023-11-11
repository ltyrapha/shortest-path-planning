using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;

namespace WHU2019302050008.Classes
{
    /// <summary>
    /// Summary description for AddNetStopsTool.
    /// </summary>
    [Guid("86171a39-be3b-4fef-aaa8-a13f60f59e4b")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("WHU2019302050008.Classes.AddNetStopsTool")]
    public sealed class AddNetStopsTool : BaseTool
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryRegistration(registerType);

            //
            // TODO: Add any COM registration code here
            //
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryUnregistration(registerType);

            //
            // TODO: Add any COM unregistration code here
            //
        }

        #region ArcGIS Component Category Registrar generated code
        /// <summary>
        /// Required method for ArcGIS Component Category registration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            ControlsCommands.Register(regKey);

        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            ControlsCommands.Unregister(regKey);

        }

        #endregion
        #endregion

        private IHookHelper m_hookHelper;
        private IFeatureWorkspace pFWorkspace;
        private IFeatureClass inputFClass;
        string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

        public AddNetStopsTool()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "NetWorkAnalyst";
            base.m_caption = "���վ��";
            base.m_message = "����ڵ�ͼ�ϵ�������";
            base.m_toolTip = "���վ��";
            base.m_name = "AddStop";  
            try
            {
                //
                // TODO: change resource name if necessary
                //
                string bitmapResourceName = GetType().Name + ".bmp";
                base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
                base.m_cursor = new System.Windows.Forms.Cursor(GetType(), GetType().Name + ".cur");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overridden Class Methods

        /// <summary>
        /// Occurs when this tool is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            try
            {
                m_hookHelper = new HookHelperClass();
                m_hookHelper.Hook = hook;
                if (m_hookHelper.ActiveView == null)
                {
                    m_hookHelper = null;
                }
            }
            catch
            {
                m_hookHelper = null;
            }

            if (m_hookHelper == null)
                base.m_enabled = false;
            else
                base.m_enabled = true;

            // TODO:  Add AddNetStopsTool.OnCreate implementation
        }

        /// <summary>
        /// Occurs when this tool is clicked
        /// </summary>
        public override void OnClick()
        {
            // TODO: Add AddNetStopsTool.OnClick implementation
            string name = NetWorkAnalysClass.getPath(path) + "\\data\\wuhanuniversity.gdb";
            pFWorkspace = NetWorkAnalysClass.OpenWorkspace(name) as IFeatureWorkspace;
            inputFClass = pFWorkspace.OpenFeatureClass("Stops");
            if (inputFClass.FeatureCount(null) > 0)
            {
                ITable pTable = inputFClass as ITable;
                pTable.DeleteSearchedRows(null);
            }
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add AddNetStopsTool.OnMouseDown implementation
            try
            {
                IPoint pStopsPoint = new PointClass();
                pStopsPoint = m_hookHelper.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                IFeature newPointFeature = inputFClass.CreateFeature();
                try
                {
                    pStopsPoint.Z = 0;
                    newPointFeature.Shape = pStopsPoint;
                    IZAware pZAware = pStopsPoint as IZAware;
                    IMAware pMAware = pStopsPoint as IMAware;
                    pZAware.ZAware = true;
                    pMAware.MAware = true;
                }
                catch
                {
                    IGeometry pGeo = pStopsPoint;
                    IZAware pZAware = pGeo as IZAware;
                    pZAware.ZAware = false;

                    newPointFeature.Shape = pGeo;
                }
                newPointFeature.Store();
                IGraphicsContainer pGrap = m_hookHelper.ActiveView as IGraphicsContainer;
                IColor pColor;
                IRgbColor pRgbColor = new RgbColorClass();
                pRgbColor.Red = 255;
                pRgbColor.Green = 255;
                pRgbColor.Blue = 255;
                pColor = pRgbColor as IColor;
                IPictureMarkerSymbol pms = new PictureMarkerSymbolClass();
                pms.BitmapTransparencyColor = pColor;
                string picturePath = NetWorkAnalysClass.getPath(path) + "\\data\\Img\\stops.bmp";
                //����Զ���վ��ͼƬ
                pms.CreateMarkerSymbolFromFile(esriIPictureType.esriIPictureBitmap, picturePath);
                pms.Size = 18;
                IMarkerElement pMarkerEle = new MarkerElementClass();
                pMarkerEle.Symbol = pms as IMarkerSymbol;
                pStopsPoint.SpatialReference = m_hookHelper.ActiveView.FocusMap.SpatialReference;
                IElement pEle = pMarkerEle as IElement;
                pEle.Geometry = pStopsPoint;
                pGrap.AddElement(pEle, 1);
                m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            }
            catch
            {
                MessageBox.Show("���վ��ʧ�ܣ�", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add AddNetStopsTool.OnMouseMove implementation
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add AddNetStopsTool.OnMouseUp implementation
        }
        #endregion
    }
}
