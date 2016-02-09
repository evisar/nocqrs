using Autofac;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using System.Xml.Serialization;

namespace nosqr.api.Services
{
    public class FileEventService: EventService, IDisposable
    {
        readonly string _inPath, _outPath, _tmpPath, _errPath;
        readonly FileSystemWatcher _fw;
        readonly IContainer _container;
        readonly bool _write;
        readonly Stopwatch _sw;
        int _count;
        

        static string CreateFolder(string x, string y)
        {
            var z = Path.Combine(x, y);
            if (!Directory.Exists(z))
            {
                Directory.CreateDirectory(z);
            }
            return z;
        }

        public FileEventService(string path, IContainer container, bool read = false, bool write = false)
        {
            _sw = Stopwatch.StartNew();
            _count = 0;

            _inPath = CreateFolder(path, "in");
            _outPath = CreateFolder(path, "out");
            _errPath = CreateFolder(path, "err");
            _tmpPath = CreateFolder(path, "tmp");
            _container = container;
            _write = write;
            if (read)
            {
                var timer = new System.Timers.Timer();
                timer.Interval = 1000;
                timer.Elapsed += timer_Elapsed;
                timer.Start();
            }
        }

        static readonly object _sync = new object();

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_sync)
            {
                Console.WriteLine("Messages per second: {0}", (decimal)_count / (decimal)_sw.Elapsed.TotalSeconds);
                var files = Directory.GetFiles(_inPath);               

                if (files.Length > 0)
                {
                    var tmp = Guid.NewGuid();
                    var tmpPath = CreateFolder(_tmpPath, tmp.ToString());

                    files.ToList().ForEach(x =>
                        {
                            var fi = new FileInfo(x);
                            var destPath = Path.Combine(tmpPath, fi.Name);
                            File.Move(x, destPath);
                        });


                    Task.Run(() =>
                        {
                            var tmpfiles = Directory.GetFiles(tmpPath);
                            foreach (var filePath in tmpfiles)
                            {
                                var fi = new FileInfo(filePath);

                                try
                                {
                                    


                                    var xml = new XmlDocument();
                                    xml.Load(filePath);
                                    var type = (from t in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                                                let elem = xml.DocumentElement
                                                let fname = string.Format("{0}.{1}", elem.NamespaceURI, elem.Name)
                                                let tname = string.Format("{0}.{1}", t.Namespace, t.Name)
                                                where t.FullName == fname
                                                select t).FirstOrDefault();


                                    var ser = new XmlSerializer(type, type.Namespace);
                                    using (var xr = new XmlNodeReader(xml))
                                    {
                                        var obj = ser.Deserialize(xr);

                                        var handlerType = typeof(DomainServicesExtensions).GetMethod("CreateHandler").MakeGenericMethod(type);
                                        var handler = handlerType.Invoke(null, new[] { _container }) as Delegate;
                                        handler.DynamicInvoke(obj);
                                    }

                                    var outPath = Path.Combine(_outPath, fi.Name);
                                    File.Move(filePath, outPath);
                                }
                                catch
                                {
                                    var errPath = Path.Combine(_errPath, fi.Name);
                                    File.Move(filePath, errPath);
                                }
                                finally
                                {
                                    Interlocked.Increment(ref _count);
                                }
                            }
                        });
                }
            }
        }
        public override void Publish<TEvent>(TEvent @event)
        {
            if (_write)
            {
                var ser = new XmlSerializer(typeof(TEvent), typeof(TEvent).Namespace);
                var filePath = Path.Combine(_inPath, string.Format("{0}.xml", Guid.NewGuid()));
                using (var fs = File.CreateText(filePath))
                {
                    ser.Serialize(fs, @event);
                }
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
