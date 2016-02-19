using Autofac;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using System.Xml.Serialization;

namespace nosqr.api.Services.Channels
{
    public class FileEventSerializer: IEventSerializer
    {
        readonly string _inPath, _outPath, _tmpPath, _errPath;
        int _read = 0, _write = 0;
        Stopwatch _sw = Stopwatch.StartNew();

        public FileEventSerializer()
        {
            var path = ConfigurationManager.AppSettings["file.path"];
            _inPath = CreateFolder(path, "in");
            _outPath = CreateFolder(path, "out");
            _errPath = CreateFolder(path, "err");
            _tmpPath = CreateFolder(path, "tmp");

            Task.Run(() =>
            {
                var fw = new FileSystemWatcher(_inPath);
                fw.EnableRaisingEvents = true;
                fw.Changed += fw_Changed;
            });
        }

        void fw_Changed(object sender, FileSystemEventArgs e)
        {
            var filePath = e.FullPath;
            if (!File.Exists(filePath))
                return;

            var fi = new FileInfo(filePath);

            try
            {
                var xml = new XmlDocument();
                xml.Load(filePath);

                var fname = string.Format("{0}.{1}", xml.DocumentElement.NamespaceURI, xml.DocumentElement.Name);

                XmlSerializer ser;
                Delegate handler;
                SetSerializer(fname, out ser, out handler);

                using (var xr = new XmlNodeReader(xml))
                {
                    var obj = ser.Deserialize(xr);
                    if(OnMessage!=null) OnMessage(obj);
                    _read++;
                }

                var outPath = Path.Combine(_outPath, fi.Name);
                File.Move(filePath, outPath);
            }
            catch
            {
                var errPath = Path.Combine(_errPath, fi.Name);
                if (File.Exists(filePath))
                {
                    File.Move(filePath, errPath);
                }
            }
        }

        static Dictionary<string, Tuple<Type, XmlSerializer>> _types
            = new Dictionary<string, Tuple<Type, XmlSerializer>>();

        private void SetSerializer(string fname, out XmlSerializer ser, out Delegate handler)
        {
            Type type = null;
            ser = null;
            handler = null;

            if (!_types.ContainsKey(fname))
            {
                type = (from t in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())

                        let tname = string.Format("{0}.{1}", t.Namespace, t.Name)
                        where t.FullName == fname
                        select t).FirstOrDefault();

                ser = new XmlSerializer(type, type.Namespace);
                _types.Add(fname, new Tuple<Type, XmlSerializer>(type, ser));
            }

            type = _types[fname].Item1;
            ser = _types[fname].Item2;
        }

        static string CreateFolder(string x, string y)
        {
            var z = Path.Combine(x, y);
            if (!Directory.Exists(z))
            {
                Directory.CreateDirectory(z);
            }
            return z;
        }

        public void Serialize<TEvent>(TEvent @event) where TEvent : IEvent
        {
            XmlSerializer ser;
            Delegate handler;
            SetSerializer(typeof(TEvent).FullName, out ser, out handler);

            var filePath = Path.Combine(_inPath, string.Format("{0}.xml", Guid.NewGuid()));
            using (var fs = File.CreateText(filePath))
            {
                ser.Serialize(fs, @event);
            }

            _write++;
        }

        public event Action<object> OnMessage;
    }
}
