using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ShimmerAPI
{
    class Logging
    {
        private StreamWriter PCsvFile = null;
        private String FileName;
        private String Delimeter = ",";
        private Boolean FirstWrite = true;

        public Logging(String fileName, String delimeter){
            Delimeter = delimeter;
            FileName = fileName;
            try
            {
                PCsvFile = new StreamWriter(FileName, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save to CSV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
	    }

        volatile bool bRowInProgress = false;
        volatile int bWriteMarkerNext = 0;
        public long mrkReceivedTime = 0;

        public void WriteMarker(int inp)
        {
            bWriteMarkerNext = inp;

        }

        

        public void WriteData(ObjectCluster obj)
        {
            bRowInProgress = true;
            if (FirstWrite)
            {
                WriteHeader(obj);
                FirstWrite = false;
            }
            Double[] data = obj.GetData().ToArray();

            PCsvFile.Write(System.DateTime.Now.Hour + ":"+ System.DateTime.Now.Minute+ "." + System.DateTime.Now.Second+"."+System.DateTime.Now.Millisecond + Delimeter);

            PCsvFile.Write(obj.elapsedTimer.ToString()+ Delimeter);

            for (int i = 0; i < data.Length; i++)
            {
                PCsvFile.Write(data[i].ToString() + Delimeter);
            }
            
            if (bWriteMarkerNext > 0)
            {
                PCsvFile.Write("MRK" + bWriteMarkerNext + Delimeter);
                bWriteMarkerNext = 0;
            }
            else
            {
                PCsvFile.Write(Delimeter);
            }

            PCsvFile.WriteLine();


            bRowInProgress = false;
        }

        private void WriteHeader(ObjectCluster obj)
        {
            ObjectCluster objectCluster = new ObjectCluster(obj);
            List<String> names = objectCluster.GetNames();
            List<String> formats = objectCluster.GetFormats();
            List<String> units = objectCluster.GetUnits();
            List<Double> data = objectCluster.GetData();
            String deviceId = objectCluster.GetShimmerID();

            for (int i = 0; i < data.Count; i++)
            {
                PCsvFile.Write(deviceId + Delimeter);
            }
            PCsvFile.WriteLine();
            for (int i = 0; i < data.Count; i++)
            {
                PCsvFile.Write(names[i] + Delimeter);
            }
            PCsvFile.WriteLine();
            for (int i = 0; i < data.Count; i++)
            {
                PCsvFile.Write(formats[i] + Delimeter);
            }
            PCsvFile.WriteLine();
            for (int i = 0; i < data.Count; i++)
            {
                PCsvFile.Write(units[i] + Delimeter);
            }
            PCsvFile.WriteLine();
        }

        public void CloseFile()
        {
            PCsvFile.Close();
        }
    }
}
