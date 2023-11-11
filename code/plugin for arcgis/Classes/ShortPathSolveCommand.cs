using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.NetworkAnalyst;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;

namespace WHU2019302050008.Classes
{
    /// <summary>
    /// Summary description for ShortPathSolveCommand.
    /// </summary>
    [Guid("6de6c4c4-6e4b-4b15-bc1a-8e57d526595b")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("WHU2019302050008.Classes.ShortPathSolveCommand")]
    public sealed class ShortPathSolveCommand : BaseCommand
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
        public static INAContext m_NAContext;//����������������Ķ���INAContext
        private INetworkDataset networkDataset;
        private IFeatureClass inputFClass;
        private IFeatureClass barriesFClass;
        string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

        public ShortPathSolveCommand()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "NetWorkAnalyst";
            base.m_caption = "·�����";
            base.m_message = "�õ����·��";
            base.m_toolTip = "·�����";
            base.m_name = "ShortPathSolver";

            try
            {
                //
                // TODO: change bitmap name if necessary
                //
                string bitmapResourceName = GetType().Name + ".bmp";
                base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overridden Class Methods

        /// <summary>
        /// Occurs when this command is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            if (hook == null)
                return;

            try
            {
                m_hookHelper = new HookHelperClass();
                m_hookHelper.Hook = hook;
                if (m_hookHelper.ActiveView == null)
                    m_hookHelper = null;
            }
            catch
            {
                m_hookHelper = null;
            }

            if (m_hookHelper == null)
                base.m_enabled = false;
            else
                base.m_enabled = true;
            // TODO:  Add other initialization code
        }

        /// <summary>
        /// Occurs when this command is clicked
        /// </summary>
        public override void OnClick()
        {
            // TODO: Add ShortPathSolveCommand.OnClick implementation
            string name = NetWorkAnalysClass.getPath(path) + "\\data\\wuhanuniversity.gdb";
            IFeatureWorkspace pFWorkspace = NetWorkAnalysClass.OpenWorkspace(name) as IFeatureWorkspace;
            //"RouteNetwork", "BaseData"�������ɸ���
            networkDataset = NetWorkAnalysClass.OpenPathNetworkDataset(pFWorkspace as IWorkspace, "RouteNetwork", "BaseData");
            m_NAContext = NetWorkAnalysClass.CreatePathSolverContext(networkDataset);
            //ͨ���������ݼ������������������
            //��Ҫ�����ݼ�
            inputFClass = pFWorkspace.OpenFeatureClass("Stops");
            barriesFClass = pFWorkspace.OpenFeatureClass("Barries");
            if (IfLayerExist("NetworkDataset") == false)
            {
                ILayer layer;
                INetworkLayer networkLayer;
                networkLayer = new NetworkLayerClass();
                networkLayer.NetworkDataset = networkDataset;
                layer = networkLayer as ILayer;
                layer.Name = "NetworkDataset";
                m_hookHelper.ActiveView.FocusMap.AddLayer(layer);
                layer.Visible = false;
            }
            if (IfLayerExist(m_NAContext.Solver.DisplayName) == true)
            {
                for (int i = 0; i < m_hookHelper.FocusMap.LayerCount; i++)
                {
                    if (m_hookHelper.FocusMap.get_Layer(i).Name == m_NAContext.Solver.DisplayName)
                    {
                        m_hookHelper.FocusMap.DeleteLayer(m_hookHelper.FocusMap.get_Layer(i));
                    }
                }
            }
            INALayer naLayer = m_NAContext.Solver.CreateLayer(m_NAContext);
            ILayer pLayer = naLayer as ILayer;
            pLayer.Name = m_NAContext.Solver.DisplayName;
            m_hookHelper.ActiveView.FocusMap.AddLayer(pLayer);
            if (inputFClass.FeatureCount(null) < 2)
            {
                MessageBox.Show("ֻ��һ��վ�㣬���ܽ���·��������");
                return;
            }
            IGPMessages gpMessages = new GPMessagesClass();
            //����վ��Ҫ�أ��������ݲ�
            NetWorkAnalysClass.LoadNANetworkLocations("Stops", inputFClass, m_NAContext, 80);
            //�����ϰ���Ҫ�أ��������ݲ�
            NetWorkAnalysClass.LoadNANetworkLocations("Barriers", barriesFClass, m_NAContext, 5);
            INASolver naSolver = m_NAContext.Solver;//���������������
            try
            {
                naSolver.Solve(m_NAContext, gpMessages, null);

            }
            catch (Exception ex)
            {
                MessageBox.Show("δ���ҵ���Ч·��" + ex.Message, "��ʾ", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                return;
            }
            for (int i = 0; i < m_hookHelper.FocusMap.LayerCount; i++)
            {
                if (m_hookHelper.FocusMap.get_Layer(i).Name == m_NAContext.Solver.DisplayName)
                {
                    ICompositeLayer pCompositeLayer = m_hookHelper.FocusMap.get_Layer(i) as ICompositeLayer;
                    {
                        for (int t = 0; t < pCompositeLayer.Count; t++)
                        {
                            ILayer pResultLayer = pCompositeLayer.get_Layer(t);
                            if (pResultLayer.Name == "Stops" || pResultLayer.Name == "Barriers")
                            {
                                pResultLayer.Visible = false;
                                continue;
                            }
                        }
                    }
                }
            }
            IGeoDataset geoDataset;
            IEnvelope envelope;
            geoDataset = m_NAContext.NAClasses.get_ItemByName("Routes") as IGeoDataset;
            envelope = geoDataset.Extent;
            if (!envelope.IsEmpty)
                envelope.Expand(1.1, 1.1, true);
            m_hookHelper.ActiveView.Extent = envelope;
            m_hookHelper.ActiveView.Refresh();
        }
        private bool IfLayerExist(string layerName)
        {
            for (int i = 0; i < m_hookHelper.FocusMap.LayerCount; i++)
            {
                if (m_hookHelper.FocusMap.get_Layer(i).Name == layerName)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}