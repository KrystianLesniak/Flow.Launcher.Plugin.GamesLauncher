using System.Xml.Serialization;

namespace GamesLauncher.Platforms.SyncEngines.EaApp.Models
{
    [XmlRoot("DiPManifest")]
    public class InstallerDataXml
    {
        [XmlElement("runtime")]
        public Runtime? Runtime { get; set; }
    }

    public class Runtime
    {
        [XmlElement("launcher")]
        public List<Launcher> Launchers { get; set; } = new List<Launcher>();
    }

    public class Launcher
    {
        [XmlElement("filePath")]
        public string FilePath { get; set; } = string.Empty;

        [XmlElement("trial")]
        public bool Trial { get; set; }
    }
}
