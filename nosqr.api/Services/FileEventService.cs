using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace nosqr.api.Services
{
    public class FileEventService: EventService, IDisposable
    {
        readonly string _path;
        readonly FileSystemWatcher _fw;

        public FileEventService(string path)
        {
            _path = path;
            _fw = new FileSystemWatcher(path, "*.xml");
            _fw.Created += fw_Created;
            _fw.Changed += _fw_Changed;
        }

        void _fw_Changed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine(e.FullPath);
        }

        void fw_Created(object sender, FileSystemEventArgs e)
        {
            var filePath = e.FullPath;
            var xml = new XmlDocument();
            xml.Load(filePath);
            var type = (from t in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                       let fname = string.Format("{0}.{1}", xml.NamespaceURI, xml.DocumentElement.Name)
                       where t.FullName == fname
                       select t).FirstOrDefault();


            var ser = new XmlSerializer(type, xml.NamespaceURI);
            using(var xr = new XmlNodeReader(xml))
            {
                var obj = ser.Deserialize(xr);
            }

        }

        public override void Publish<TEvent>(TEvent @event)
        {
            var ser = new XmlSerializer(typeof(TEvent), typeof(TEvent).Namespace);
            var filePath = Path.Combine(_path, string.Format("{0}.xml", Guid.NewGuid()));
            using(var fs = File.CreateText(filePath))
            {
                ser.Serialize(fs, @event);
            }
            base.Publish<TEvent>(@event);
        }

        public void Dispose()
        {
            if(_fw!=null)
            {
                _fw.Dispose();
            }
        }
    }
}
