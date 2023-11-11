using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace WHU2019302050008.Forms
{
    public partial class FormQueryAttr : Form
    {
        private AxMapControl mMapControl;
        private IFeatureLayer mFeatureLayer;
        private IFeatureClass pFeatureClass=null;
        public FormQueryAttr(AxMapControl mapcontrol)
        {
            InitializeComponent();
            this.mMapControl = mapcontrol;
        }
        private void FormQueryAttr_Load(object sender, EventArgs e) 
        {
            if (this.mMapControl.LayerCount <= 0)
                return;
            ILayer pLayer;
            string strLayerName;
            for (int i = 0; i < this.mMapControl.LayerCount; i++) {
                pLayer = this.mMapControl.get_Layer(i);
                strLayerName = pLayer.Name;
                this.comboBox1.Items.Add(strLayerName);
            }
            this.comboBox1.SelectedIndex = 0;
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) 
        {
            this.listBoxField.Items.Clear();
            mFeatureLayer = mMapControl.get_Layer(comboBox1.SelectedIndex) as IFeatureLayer;
            pFeatureClass = mFeatureLayer.FeatureClass;
            string strFIdName;
            for (int i = 0; i < pFeatureClass.Fields.FieldCount; i++) 
            {
                strFIdName = pFeatureClass.Fields.get_Field(i).Name;
                this.listBoxField.Items.Add(strFIdName);
            }
            this.listBoxField.SelectedIndex = 0;
        }
        private void listBoxField_SelectedIndexChanged(object sender,EventArgs e)
        {
            string sFieldName = listBoxField.Text;
            listBoxValue.Items.Clear();
            int iFieldIndex = 0;
            IField pField = null;
            IFeatureCursor pFeatCursor = pFeatureClass.Search(null, true);
            IFeature pFeat = pFeatCursor.NextFeature();//初始定位
            iFieldIndex = pFeatureClass.FindField(sFieldName);//得到要素类的字段索引
            pField = pFeatureClass.Fields.get_Field(iFieldIndex);
            while (pFeat != null) 
            {
                if (pField.Type == esriFieldType.esriFieldTypeString)
                {
                    listBoxValue.Items.Add("'" + pFeat.get_Value(iFieldIndex) + "'");
                }
                else
                {
                    listBoxValue.Items.Add(pFeat.get_Value(iFieldIndex));
                }
                pFeat = pFeatCursor.NextFeature();
            }
        }

        private void listBoxField_DoubleClick(object sender, EventArgs e)
        {
            textBox1.SelectedText = listBoxField.SelectedItem.ToString() + " ";
        }

        private void listBoxValue_DoubleClick(object sender, EventArgs e)
        {
            textBox1.SelectedText = listBoxValue.SelectedItem.ToString() + " ";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.SelectedText = "=";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.SelectedText = "!=";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.SelectedText = "is";
        }

        private void button17_Click(object sender, EventArgs e)
        {
            try
            {
                mMapControl.Map.ClearSelection();
                IActiveView pActiveView = mMapControl.Map as IActiveView;
                IQueryFilter pQueryFilter = new QueryFilterClass();
                pQueryFilter.WhereClause = textBox1.Text;
                IFeatureCursor pFeatureCursor = mFeatureLayer.Search(pQueryFilter, false);
                IFeature pFeature = pFeatureCursor.NextFeature();
                while (pFeature != null)
                {
                    mMapControl.Map.SelectFeature(mFeatureLayer, pFeature);
                    pFeature = pFeatureCursor.NextFeature();
                }
                pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                pActiveView.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button21_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            textBox1.SelectedText = ">=";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.SelectedText = "like";
        }


        
    }   
}
