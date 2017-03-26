using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SynthesisOfDM
{
    class TFFile
    {
        FileStream fs;

        BinaryReader br;
        BinaryWriter bw;

        public void OpenStream(string filename, FileMode fm)
        {
            fs = new FileStream(filename, fm);
        }

        public void CloseStream()
        {
            fs.Close();
        }

        public void StartBR()
        {
            br = new BinaryReader(fs);
        }

        public void StartBW()
        {
            bw = new BinaryWriter(fs);
        }

        public void StopBR()
        {
            br.Close();
        }

        public void StopBW()
        {
            bw.Flush();
            bw.Close();
        }

        public FileStream GetFs() { return fs; }
        public BinaryReader GetBr() { return br; }
        public BinaryWriter GetBw() { return bw; }

        /*
                Console.WriteLine(br.ReadDecimal());
                Console.WriteLine(br.ReadString());
                Console.WriteLine(br.ReadString());
                Console.WriteLine(br.ReadChar());

                FileStream fs = new FileStream(filename, FileMode.Append);
                FileStream fs = new FileStream(filename, FileMode.Open);

                w.Write(1.2M);
                w.Write("string");
                w.Write("string 2");
                w.Write('!');
        */
    }
}
