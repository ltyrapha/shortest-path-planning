using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Controls;
using WHU2019302050008.Classes;
using WHU2019302050008.Forms;
using ESRI.ArcGIS.Geodatabase;



namespace WHU2019302050008
{
    
    public partial class Form1 : Form
    {
        private string pMapUnits;
        public Form1()
        {
            InitializeComponent();
        }

        private void Openmxd_Click(object sender, EventArgs e)
        {
            OpenFileDialog OpenMXD=new OpenFileDialog();
            OpenMXD.Title="打开地图";
            OpenMXD.InitialDirectory="D:";
            OpenMXD.Filter="Map Documents(*.mxd)|*.mxd";
            if (OpenMXD.ShowDialog()==DialogResult.OK)
            {
                string MxdPath=OpenMXD.FileName;
                axMapControl1.LoadMxFile(MxdPath);
            }
        }
        IEnvelope pEnvelope;

        private void SynchronizeEagleEye()
        {
            if (axMapControl2.LayerCount > 0)
            {
                axMapControl2.ClearLayers();
            }

            axMapControl2.SpatialReference = axMapControl1.SpatialReference;
            //保持鸟瞰图和数据视图图层顺序一致
            for (int i = axMapControl1.LayerCount - 1; i >= 0; i--)
            {
                axMapControl2.AddLayer(axMapControl1.get_Layer(i));
            }
            axMapControl2.Extent = axMapControl1.Extent;
            pEnvelope = axMapControl1.Extent;
            DrawRectangle(pEnvelope);
            axMapControl2.Refresh();
        }
        private void DrawRectangle(IEnvelope pEnvelope)
        {
            IGraphicsContainer pGraphicsContainer = axMapControl2.Map as IGraphicsContainer;
            IActiveView pActiveView = pGraphicsContainer as IActiveView;

            pGraphicsContainer.DeleteAllElements();
            IRectangleElement pRectangleElement = new RectangleElementClass();
            IElement pElement = pRectangleElement as IElement;
            pElement.Geometry = pEnvelope;

            IRgbColor pRgbColor = new RgbColorClass();
            pRgbColor.Red = 255;
            pRgbColor.Blue = 0;
            pRgbColor.Green = 0;
            pRgbColor.Transparency = 255;

            ILineSymbol pLineSymbol = new SimpleLineSymbolClass();
            pLineSymbol.Width = 3;
            pLineSymbol.Color = pRgbColor;

            pRgbColor.Red = 255;
            pRgbColor.Blue = 0;
            pRgbColor.Green = 0;
            pRgbColor.Transparency = 0;

            IFillSymbol pFillSymbol = new SimpleFillSymbolClass();
            pFillSymbol.Outline = pLineSymbol;
            pFillSymbol.Color = pRgbColor;

            IFillShapeElement pFillShapeElement = pElement as IFillShapeElement;
            pFillShapeElement.Symbol = pFillSymbol;

            pGraphicsContainer.AddElement((IElement)pFillShapeElement, 0);
            pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        private void axMapControl1_OnMapReplaced(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMapReplacedEvent e)
        {
            SynchronizeEagleEye();

            CopyMapFromMapControlToPageLayoutControl();//调用地图复制函数

            esriUnits mapUnits = axMapControl1.MapUnits;
            switch (mapUnits)
            {
                case esriUnits.esriCentimeters:
                    pMapUnits = "Centimeters";
                    break;
                case esriUnits.esriDecimalDegrees:
                    pMapUnits = "Decimal Degrees";
                    break;
                case esriUnits.esriDecimeters:
                    pMapUnits = "Decimeters";
                    break;
                case esriUnits.esriFeet:
                    pMapUnits = "Feet";
                    break;
                case esriUnits.esriInches:
                    pMapUnits = "Inches";
                    break;
                case esriUnits.esriKilometers:
                    pMapUnits = "Kilometers";
                    break;
                case esriUnits.esriMeters:
                    pMapUnits = "Meters";
                    break;
                case esriUnits.esriMiles:
                    pMapUnits = "Miles";
                    break;
                case esriUnits.esriMillimeters:
                    pMapUnits = "Millimeters";
                    break;
                case esriUnits.esriNauticalMiles:
                    pMapUnits = "NauticalMiles";
                    break;
                case esriUnits.esriPoints:
                    pMapUnits = "Points";
                    break;
                case esriUnits.esriUnknownUnits:
                    pMapUnits = "Unknown";
                    break;
                case esriUnits.esriYards:
                    pMapUnits = "Yards";
                    break;
            }


        }

        private void axMapControl1_OnExtentUpdated(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnExtentUpdatedEvent e)
        {
            pEnvelope = (IEnvelope)e.newEnvelope;
            DrawRectangle(pEnvelope);
        }

        private void axMapControl2_OnMouseMove(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseMoveEvent e)
        {
            if (e.button == 1)
            {
                IPoint pPoint = new PointClass();
                pPoint.PutCoords(e.mapX, e.mapY);
                axMapControl1.CenterAt(pPoint);
                axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
            }
        }

        private void CopyMapFromMapControlToPageLayoutControl()
        {
            try
            {
                //获得IObjectCopy接口
                IObjectCopy pObjectCopy = new ObjectCopyClass();
                //获得要拷贝的图层 
                System.Object pSourceMap = axMapControl1.Map;
                //获得拷贝图层
                System.Object pCopiedMap = pObjectCopy.Copy(pSourceMap);
                //获得要重绘的地图 
                System.Object pOverwritedMap = axPageLayoutControl1.ActiveView.FocusMap;
                //重绘pagelayout地图
                pObjectCopy.Overwrite(pCopiedMap, ref pOverwritedMap);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            // 取得鼠标所在工具的索引号  
            int index = axToolbarControl1.HitTest(e.x, e.y, false);
            if (index != -1)
            {
                // 取得鼠标所在工具的 ToolbarItem  
                IToolbarItem toolbarItem = axToolbarControl1.GetItem(index);
                // 设置状态栏信息  
                StatusLabel.Text = toolbarItem.Command.Message;
            }
            else
            {
                StatusLabel.Text = " 就绪 ";
            }

            // 显示当前比例尺
            ScaleLabel.Text = " 比例尺 1:" + ((long)this.axMapControl1.MapScale).ToString();

            // 显示当前坐标
            CoordinateLabel.Text = " 当前坐标 X = " + e.mapX.ToString() + " Y = " + e.mapY.ToString() + " " + pMapUnits.ToString();

            //漫游（BaseTool方法）
            if (pan != null)
                pan.OnMouseMove(e.button, e.shift, e.x, e.y);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pMapUnits = "Unknown";
            axTOCControl1.SetBuddyControl(axMapControl1);
            axTOCControl1.ActiveView.Clear();
        }

        private Pan pan = null;
        private void 漫游ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //声明并初始化
            pan = new Pan();
            //关联MapControl
            pan.OnCreate(this.axMapControl1.Object);
            //设置鼠标形状 
            this.axMapControl1.MousePointer = esriControlsMousePointer.esriPointerPan;
            this.axMapControl1.CurrentTool = pan;

        }

        private void axMapControl1_OnMouseUp(object sender, IMapControlEvents2_OnMouseUpEvent e)
        {
            //漫游（BaseTool方法）
            if (pan != null)
                pan.OnMouseUp(e.button, e.shift, e.x, e.y);
        }

        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if (pan != null)
                pan.OnMouseDown(e.button, e.shift, e.x, e.y);

        }

        private void axMapControl2_OnMouseDown_1(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if (e.button == 1)
            {
                IPoint pPoint = new PointClass();
                pPoint.PutCoords(e.mapX, e.mapY);
                axMapControl1.CenterAt(pPoint);
                axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
            }
            else if (e.button == 2)
            {
                IEnvelope pEnv = axMapControl2.TrackRectangle();
                axMapControl1.Extent = pEnv;
                axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
            }
        }
        private void 中心放大ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //声明与初始化 
            FixedZoomIn fixedZoomin = new FixedZoomIn();
            //与MapControl关联
            fixedZoomin.OnCreate(this.axMapControl1.Object);
            fixedZoomin.OnClick();

        }

        private void 中心缩小ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ICommand command = new ControlsMapZoomOutFixedCommandClass();
            command.OnCreate(this.axMapControl1.Object);
            command.OnClick();
        }
        private void 放大ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Tool的定义和初始化 
            ITool tool = new ControlsMapZoomInToolClass();
            //查询接口获取ICommand 
            ICommand command = tool as ICommand;
            //Tool通过ICommand与MapControl的关联 
            command.OnCreate(this.axMapControl1.Object);
            command.OnClick();
            //MapControl的当前工具设定为tool 
            this.axMapControl1.CurrentTool = tool;

        }
        private void 缩小ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Tool的定义和初始化 
            ITool ptool = new ControlsMapZoomOutToolClass();
            //查询接口获取ICommand 
            ICommand command = ptool as ICommand;
            //Tool通过ICommand与MapControl的关联 
            command.OnCreate(this.axMapControl1.Object);
            command.OnClick();
            //MapControl的当前工具设定为tool 
            this.axMapControl1.CurrentTool = ptool;
        }

        private void 全图显示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ICommand command = new ControlsMapFullExtentCommandClass();
            command.OnCreate(this.axMapControl1.Object);
            command.OnClick();
        }

        private void 属性查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormQueryAttr formqueryattr = new FormQueryAttr(this.axMapControl1);
            formqueryattr.Show();
        }

        private void 要素选择ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.axMapControl1.CurrentTool = null;
            //Tool的定义和初始化 
            ControlsSelectFeaturesToolClass pTool = new ESRI.ArcGIS.Controls.ControlsSelectFeaturesToolClass();
            //Tool通过ICommand与MapControl的关联 
            pTool.OnCreate(this.axMapControl1.Object);
            //MapControl的当前工具设定为tool 
            this.axMapControl1.CurrentTool = pTool as ITool;
        }

        private ILayer pLayer;
        private void axTOCControl1_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            if (axMapControl1.LayerCount > 0)
            {
                esriTOCControlItem pItem = new esriTOCControlItem();
                pLayer = new FeatureLayerClass();
                IBasicMap pBasicMap = new MapClass();
                object pOther = new object();
                object pIndex = new object();
                // Returns the item in the TOCControl at the specified coordinates.
                axTOCControl1.HitTest(e.x, e.y, ref pItem, ref pBasicMap, ref pLayer, ref pOther, ref pIndex);
            }//TOCControl类的ITOCControl接口的HitTest方法
            if (e.button == 2)
            {
                contextMenuStrip1.Show(axTOCControl1, e.x, e.y);
            }
        }
        private void 打开属性表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //传入图层，在右击事件里返回的图层
            FormAttr frm1 = new FormAttr(pLayer as IFeatureLayer);
            frm1.Show();
        }


        //双击开始显示要素name属性
        private void axMapControl1_OnDoubleClick(object sender, IMapControlEvents2_OnDoubleClickEvent e)
        {
            ILayer pLayer = new FeatureLayerClass();
            IFeatureLayer pFeatureLayer;
            for (int i = 1; i <= axMapControl1.LayerCount - 1; i++)
            {
                pLayer = axMapControl1.get_Layer(i);// pFeatLyr是要显示的图层
                pFeatureLayer = pLayer as IFeatureLayer;
                pLayer.ShowTips = true;
                ILayerFields pLayerFields = pLayer as ILayerFields;
                for (int j = 0; j <= pLayerFields.FieldCount - 1; j++)
                {
                    IField field = pLayerFields.get_Field(j);
                    if (field.Name == "name")// sFieldName是要显示的字段
                    {
                        pFeatureLayer.DisplayField = field.Name;
                        break;
                    }
                }
                axMapControl1.ShowMapTips = true;
            }
            
        }

        //最短路径分析
        IMapDocument mapDocument;
        public IFeatureWorkspace pFWorkspace;
        string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

        private void 添加站点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
                ICommand pCommand;
                pCommand = new AddNetStopsTool();
                pCommand.OnCreate(axMapControl1.Object);
                axMapControl1.CurrentTool = pCommand as ITool;
                pCommand = null;
        }

        private void 添加障碍ToolStripMenuItem_Click(object sender, EventArgs e)
        {
                ICommand pCommand;
                pCommand = new AddNetBarriesTool();
                pCommand.OnCreate(axMapControl1.Object);
                axMapControl1.CurrentTool = pCommand as ITool;
                pCommand = null;
        }

        private void 路径解决ToolStripMenuItem_Click(object sender, EventArgs e)
        {
                ICommand pCommand;
                pCommand = new ShortPathSolveCommand();
                pCommand.OnCreate(axMapControl1.Object);
                pCommand.OnClick();
                pCommand = null;
        
        }

        private void 清除分析ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            axMapControl1.CurrentTool = null;
                try
                {
                    string name = NetWorkAnalysClass.getPath(path) + "\\data\\wuhanuniversity.gdb";
                    //打开工作空间
                    pFWorkspace = NetWorkAnalysClass.OpenWorkspace(name) as IFeatureWorkspace;
                    IGraphicsContainer pGrap = this.axMapControl1.ActiveView as IGraphicsContainer;
                    pGrap.DeleteAllElements();//删除所添加的图片要素
                    IFeatureClass inputFClass = pFWorkspace.OpenFeatureClass("Stops");
                    //删除站点要素
                    if (inputFClass.FeatureCount(null) > 0)
                    {
                        ITable pTable = inputFClass as ITable;
                        pTable.DeleteSearchedRows(null);
                    }
                    IFeatureClass barriesFClass = pFWorkspace.OpenFeatureClass("Barries");//删除障碍点要素
                    if (barriesFClass.FeatureCount(null) > 0)
                    {
                        ITable pTable = barriesFClass as ITable;
                        pTable.DeleteSearchedRows(null);
                    }
                    for (int i = 0; i < axMapControl1.LayerCount; i++)//删除分析结果
                    {
                        ILayer pLayer = axMapControl1.get_Layer(i);
                        if (pLayer.Name == ShortPathSolveCommand.m_NAContext.Solver.DisplayName)
                        {
                            axMapControl1.DeleteLayer(i);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                this.axMapControl1.Refresh();
        
        }

        private void frmShortPathSolver_Load(object sender, EventArgs e)
        {

        }






    }

}

