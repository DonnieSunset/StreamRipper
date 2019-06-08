//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.ComponentModel.DataAnnotations;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml;
//using System.Xml.Serialization;

//namespace StreamRipper.Domain {
//    [XmlRoot(Namespace = "StreamRipper", ElementName = nameof(AppSettings))]
//    public class AppSettings {
//        private const string DEFAULT_FILE_NAME = "Settings.xml";
//        public static string DefaultFileName { get { return DEFAULT_FILE_NAME; } }

//        private static readonly AppSettings _current;
//        public static AppSettings Current { get { return _current; } }

//        [XmlAttribute]
//        public int Version { get; set; } = 1;

//        [XmlElement(IsNullable = false)]
//        public int RecordBacktrackSeconds { get; set; } = 0;

//        [XmlElement(IsNullable = true)]
//        public string RecordingOutputDirectory { get; set; } = null;

//        [XmlArray(IsNullable = true)]
//        public ObservableCollection<StreamSource> StreamSources { get; set; } = new ObservableCollection<StreamSource>();

//        public AppSettings() {
//        }

//        static AppSettings() {
//            _current = LoadOrCreate();
//        }

//        public static AppSettings LoadOrCreate(string fileName = DEFAULT_FILE_NAME) {
//            if(string.IsNullOrWhiteSpace(fileName)) {
//                throw new ArgumentNullException(nameof(fileName));
//            }

//            var appSettings = new AppSettings();

//            if(!File.Exists(fileName)) {
//                appSettings.StreamSources.Add(new StreamSource {
//                    DisplayName = "Radio Energy Nuremberg",
//                    FilePrefix = "NRJ_Nuremberg",
//                    StreamUrl = "http://energyradio.de/nuernberg"
//                });

//                appSettings.StreamSources.Add(new StreamSource {
//                    DisplayName = "Metal-only",
//                    FilePrefix = "MetalOnly",
//                    StreamUrl = "http://server1.blitz-stream.de:4400"
//                });

//                appSettings.Write(DefaultFileName);
//            } else {
//                appSettings.Read(DefaultFileName);
//            }

//            return appSettings;
//        }

//        public void Write(string fileName) {
//            if(string.IsNullOrWhiteSpace(fileName)) {
//                throw new ArgumentNullException(nameof(fileName));
//            }

//            using(var f = File.Open(fileName, FileMode.Create)) {
//                var w = new XmlSerializer(GetType());
//                w.Serialize(f, this);
//            }
//        }

//        public void Read(string fileName) {
//            if(string.IsNullOrWhiteSpace(fileName)) {
//                throw new ArgumentNullException(nameof(fileName));
//            }

//            using(var f = File.Open(fileName, FileMode.Open)) {
//                var w = new XmlSerializer(GetType());
//                var loadedSettings = (AppSettings)w.Deserialize(f);

//                // As for why we are copying these values (especially the StreamSources
//                // collection item-by-item) and not just replacing the object:
//                // We don't want possible WPF bidings to break and rather have them
//                // recognize the changes. Replacing references is a no-go there.
//                // TODO: This could be done much nicer. Expressions FTW!
//                RecordBacktrackSeconds = loadedSettings.RecordBacktrackSeconds;
//                RecordingOutputDirectory = loadedSettings.RecordingOutputDirectory;

//                StreamSources.Clear();
//                foreach(var src in loadedSettings.StreamSources) {
//                    StreamSources.Add(src);
//                }
//            }
//        }
//    }
//}
