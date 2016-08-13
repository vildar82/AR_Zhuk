using AR_Zhuk_DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace AR_AreaZhuk
{

    //public class GeneralObject
    //{
    //    public string GUID { get; set; }
    //    public List<HouseInfo> Houses = new List<HouseInfo>();
    //    public SpotInfo SpotInf { get; set; }

    //}
    public class Serializer
    {

        public void SerializeSpoinfo(SpotInfo sp)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Zhuk_Config.cfg";
           
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                XmlSerializer ser = new XmlSerializer(sp.GetType());
                ser.Serialize(fs, sp);
            }
        }

        public SpotInfo GetLastSpot()
        {

            SpotInfo sp = new SpotInfo();
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Zhuk_Config.cfg";
            if (!File.Exists(path))
                return new SpotInfo();
            try
            {
                XmlSerializer ser = new XmlSerializer(sp.GetType());
                using (XmlReader reader = XmlReader.Create(path))
                {
                    try
                    {
                        sp = (SpotInfo)ser.Deserialize(reader);
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {

                //  MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            return sp;
        }
        public void SerializeList(GeneralObject go,string mark)
        {


            //SaveFileDialog sfd = new SaveFileDialog();

            //sfd.Filter = "xml files |*.xml";
            //sfd.FilterIndex = 2;
            //sfd.RestoreDirectory = true;

            //if (sfd.ShowDialog() == DialogResult.OK)
            //{
            if (File.Exists(@"E:\Экспорты\" + mark + ".xml"))
                mark += "1";
            Thread.Sleep(100);
            bool isContinue = true;
            //while (isContinue)
            //{
                //try
                //{
                    using (FileStream fs = new FileStream(@"E:\Экспорты\" + mark + ".xml", FileMode.Create, FileAccess.Write))
                    {
                        //try
                        //{
                        XmlSerializer ser = new XmlSerializer(go.GetType());
                        ser.Serialize(fs, go);
                           // isContinue = false;
                        //}
                        //catch
                        //{
                        //}

                    }
                //}
                //catch { }
            //}
           
               
            //}
           
        }

        public List<HouseInfo> DeserializeXmlFile(string filePath)
        {

            List<HouseInfo> houseInfo = new List<HouseInfo>();

            try
            {
                XmlSerializer ser = new XmlSerializer(houseInfo.GetType());
                using (XmlReader reader = XmlReader.Create(filePath))
                {
                    try
                    {
                        houseInfo = (List<HouseInfo>)ser.Deserialize(reader);
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {

                //  MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            return houseInfo;
        }
    }
}
