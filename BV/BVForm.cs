using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using AR_Zhuk_DataModel;



namespace BeetlyVisualisation
{
    public partial class BVForm : Form
    {
        public BVForm()
        {
            InitializeComponent();
            this.Height = 136;
        }

        private void BVForm_Load(object sender, EventArgs e)
        {

        }

        private void buttonSetXMLFile_Click(object sender, EventArgs e)
        {
            this.textBoxXMLInfoFile.Text = GetXMLFileName(this.textBoxXMLInfoFile.Text);

        }

        private string GetXMLFileName(string definedPath)
        {
            string XMLPath = string.Empty;

            OpenFileDialog openXMLFileDialog = new OpenFileDialog();
            openXMLFileDialog.Filter = "XML Files (*.xml)|*.xml";
            openXMLFileDialog.RestoreDirectory = true;

            if (openXMLFileDialog.ShowDialog() == DialogResult.OK)
            {
                return openXMLFileDialog.FileName;
            }


            return definedPath;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if(System.IO.File.Exists(this.textBoxXMLInfoFile.Text))
            {
                string XmlInfoFile = this.textBoxXMLInfoFile.Text;

                // string imagePath = @"C:\Users\fazleevaa\Desktop\Новая папка\";

                // string imageOutputPath = imagePath + @"Тесты\";

                

                string imageOutputPath = Path.GetDirectoryName(XmlInfoFile) + "\\" + Path.GetFileNameWithoutExtension(XmlInfoFile) + "\\";

                string imagePath = @"\\dsk2.picompany.ru\project\CAD_Settings\Revit_server\13. Settings\02_RoomManager\00_PNG_ПИК1";

                
                
                string ExcelDataPath = @"C:\Users\fazleevaa\Desktop\БД_Параметрические данные квартир ПИК1.xlsx";

                ImageCombiner ic = new ImageCombiner(XmlInfoFile, ExcelDataPath, imagePath, 72);
                //ic.CombineImages(imageOutputPath);
                ic.generateGeneralObject(imageOutputPath);
                MessageBox.Show("Генерация изображений завершена");
            }
            else
            {
                MessageBox.Show("Проверьте задание пути файла XML", "Ошибка");
            }

            
        }

        private void buttonPreview_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(this.textBoxXMLInfoFile.Text))
            {
                

                string XmlInfoFile = textBoxXMLInfoFile.Text;

                Serializer ser = new Serializer();
                GeneralObject  genObj = Utils.getHouseInfo(XmlInfoFile);                

                string imagePath = @"\\dsk2.picompany.ru\project\CAD_Settings\Revit_server\13. Settings\02_RoomManager\00_PNG_ПИК1";
                                                        
                string ExcelDataPath = @"C:\Users\fazleevaa\Desktop\БД_Параметрические данные квартир ПИК1.xlsx";

                ImageCombiner ic = new ImageCombiner(genObj, ExcelDataPath, imagePath, 72);

                Bitmap previewImage = ic.generateGeneralObject();

                pictureBoxPreview.Image = previewImage;

                this.Height = 500;



            }
            else
            {
                MessageBox.Show("Проверьте задание пути файла XML", "Ошибка");
            }
        }
    }
}
