using System;
using System.Threading;
using System.Text;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.IO;
using GHI.Premium.IO;
using System.IO;

namespace OakhillLandroverController
{
    //http://www.tinyclr.com/forum/1/1858/
  
    public class Storage
    {
        private PersistentStorage ps;
        private Microsoft.SPOT.Hardware.InputPort cardDetectPin; 

        public Storage(string storageType)
        {
            try
            {
                if (PersistentStorage.DetectSDCard())
                {
                    ps = new PersistentStorage(storageType);
                    ps.MountFileSystem();
                    Debug.Print("SD Card found and mounted.");
                }

                if (!VolumeInfo.GetVolumes()[0].IsFormatted)
                {
                    Debug.Print("SD Card not formatted.");
                    throw new HomeMonitorException("SD Card not formatted.");
                }

            }
            catch (Exception e)
            {
                Debug.Print("Error: No SD Card Found!");
                //throw new HomeMonitorException("Error: No SD Card Found!", e);
            }
        }

        public Storage(string storageType, Microsoft.SPOT.Hardware.Cpu.Pin _detectPin)
        {
            cardDetectPin = new InputPort(_detectPin, false, Port.ResistorMode.PullUp);
            try
            {
                if (PersistentStorage.DetectSDCard())
                {
                    ps = new PersistentStorage(storageType);
                    ps.MountFileSystem();
                    Debug.Print("SD Card found and mounted.");
                }

                if (!VolumeInfo.GetVolumes()[0].IsFormatted)
                {
                    Debug.Print("SD Card not formatted.");
                    throw new HomeMonitorException("SD Card not formatted.");
                }

            }
            catch (Exception e)
            {
                Debug.Print("Error: No SD Card Found!");
                //throw new HomeMonitorException("Error: No SD Card Found!", e);
            }
        }

        /// <summary>
        /// Write bytes to a file.
        /// </summary>
        /// <param name="fileName">File to read.</param>
        /// <param name="data">Bytes to write to the file.</param>
        public void WriteFileBytes(string fileName, byte[] data)
        {
            try
            {
                string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;

                Debug.Print("Looking for file " + fileName + " and deleting it if exists.");
                if (File.Exists(rootDirectory + "\\" + fileName))
                    File.Delete(rootDirectory + "\\" + fileName);

                Debug.Print("Writing file..." + fileName);
                FileStream FileHandle = new FileStream(rootDirectory + "\\" + fileName, FileMode.Create, FileAccess.Write);

                //write file
                FileHandle.Write(data, 0, data.Length);

                FileHandle.Close();
            }
            catch(Exception)
            {
                Debug.Print("Error: No File Handle.");
                throw new HomeMonitorException("No File Handle to write.");
            }
            // Flush everything to make sure we are starting fresh.
            //ps.UnmountFileSystem();
            Thread.Sleep(500);

            //ps.Dispose();
        }

        /// <summary>
        /// Write a line to a file.
        /// </summary>
        /// <param name="fileName">File to write to.</param>
        /// <param name="data">Lines to write to the file.</param>
        public void WriteFileLine(string fileName, string line)
        {
            try
            {
                string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;

                Debug.Print("Writing file..." + fileName);
                StreamWriter FileHandle = new StreamWriter(rootDirectory + "\\" + fileName, true); //creates a new file, or appends to file if it exists

                FileHandle.WriteLine(line);
                FileHandle.Close();
            }
            catch (Exception)
            {
                Debug.Print("WriteFileLine Error: No File Handle.");
                //throw new HomeMonitorException("No File Handle to write.");
            }

            // Flush everything to make sure we are starting fresh.
            //ps.UnmountFileSystem();
            Thread.Sleep(500);

            //ps.Dispose();
        }

        /// <summary>
        /// Write lines to a file.
        /// </summary>
        /// <param name="fileName">File to write to.</param>
        /// <param name="data">Lines to write to the file.</param>
        public void WriteFileLines(string fileName, string[] lines)
        {
            try
            {
                string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;

                Debug.Print("Looking for file " + fileName + " and deleting it if exists.");
                if (File.Exists(rootDirectory + "\\" + fileName))
                    File.Delete(rootDirectory + "\\" + fileName);
                

                Debug.Print("Writing file..." + fileName);
                StreamWriter FileHandle = new StreamWriter(rootDirectory + "\\" + fileName); //creates a new file, or appends to file if it exists

                //write file
                foreach (string line in lines)
                {
                    if(line != null  && line != string.Empty)
                    FileHandle.WriteLine(line);
                }
                FileHandle.Close();
            }
            catch (Exception)
            {
                Debug.Print("Error: No File Handle.");
                throw new HomeMonitorException("No File Handle to write.");
            }

            // Flush everything to make sure we are starting fresh.
            //ps.UnmountFileSystem();
            Thread.Sleep(500);

            //ps.Dispose();
        }

        /// <summary>
        /// Read a file.
        /// </summary>
        /// <param name="fileName">File to read.</param>
        /// <param name="numBytes">Number of bytes to read from the file.</param>
        public byte[] ReadFile(string fileName, int numBytes)
        {
            string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;

            Debug.Print("Looking if " + fileName + " exists.");

            if (!File.Exists(rootDirectory + "\\" + fileName))
                return null;

            byte[] buffer = new byte[numBytes];

            //ps.MountFileSystem();

            Debug.Print("Reading File..." + fileName);
            FileStream FileHandle = new FileStream(rootDirectory + "\\" + fileName, FileMode.Open, FileAccess.Read);

            FileHandle.Read(buffer, 0, buffer.Length);

            FileHandle.Close();

            Thread.Sleep(500);
            //ps.UnmountFileSystem();
            //ps.Dispose();

            return buffer;
        }

        /// <summary>
        /// Reads a given number of lines from a file.
        /// </summary>
        /// <param name="fileName">File to read.</param>
        /// <param name="numLines">Number of lines to read from the file.</param>
        public string[] ReadFileLines(string fileName, int numLines)
        {
            string rootDirectory = VolumeInfo.GetVolumes()[0].RootDirectory;

            Debug.Print("Looking if " + fileName + " exists.");

            if (!File.Exists(rootDirectory + "\\" + fileName))
                return null;

            string[] buffer = new string[numLines];

            //ps.MountFileSystem();

            Debug.Print("Reading File Lines..." + fileName);
            StreamReader FileHandle = new StreamReader(rootDirectory + "\\" + fileName);

            for (int i = 0; i < buffer.Length; i++ )
            {
                buffer[i] = FileHandle.ReadLine();
                if (buffer[i] == "")
                    break;
            }

            FileHandle.Close();

            Thread.Sleep(500);
            //ps.UnmountFileSystem();
            //ps.Dispose();

            return buffer;
        }

        public bool sdCardDetect
        {
            get
            {
                //active low detect input
                if (cardDetectPin != null)
                    return !(cardDetectPin.Read());
                else
                    return false;
            }
        
        }
    }
}
