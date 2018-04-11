using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MICE.Nintendo.Databases.NstDatabase
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public partial class database
    {
        private DatabaseGame[] gameField;

        private decimal versionField;

        private string conformanceField;

        /// <remarks/>
        [XmlElement("game")]
        public DatabaseGame[] game
        {
            get
            {
                return this.gameField;
            }
            set
            {
                this.gameField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public decimal version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public string conformance
        {
            get
            {
                return this.conformanceField;
            }
            set
            {
                this.conformanceField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class DatabaseGame
    {

        private DatabaseGameDevice[] peripheralsField;

        private DatabaseGameArcade arcadeField;

        private DatabaseGameCartridge[] cartridgeField;

        /// <remarks/>
        [XmlArrayItem("device", IsNullable = false)]
        public DatabaseGameDevice[] peripherals
        {
            get
            {
                return this.peripheralsField;
            }
            set
            {
                this.peripheralsField = value;
            }
        }

        /// <remarks/>
        public DatabaseGameArcade arcade
        {
            get
            {
                return this.arcadeField;
            }
            set
            {
                this.arcadeField = value;
            }
        }

        /// <remarks/>
        [XmlElement("cartridge")]
        public DatabaseGameCartridge[] cartridge
        {
            get
            {
                return this.cartridgeField;
            }
            set
            {
                this.cartridgeField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class DatabaseGameDevice
    {

        private string typeField;

        /// <remarks/>
        [XmlAttribute]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class DatabaseGameArcade
    {

        private DatabaseGameArcadeBoard boardField;

        private string systemField;

        private string dumpField;

        private string crcField;

        private string sha1Field;

        /// <remarks/>
        public DatabaseGameArcadeBoard board
        {
            get
            {
                return this.boardField;
            }
            set
            {
                this.boardField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public string system
        {
            get
            {
                return this.systemField;
            }
            set
            {
                this.systemField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public string dump
        {
            get
            {
                return this.dumpField;
            }
            set
            {
                this.dumpField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public string crc
        {
            get
            {
                return this.crcField;
            }
            set
            {
                this.crcField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public string sha1
        {
            get
            {
                return this.sha1Field;
            }
            set
            {
                this.sha1Field = value;
            }
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class DatabaseGameArcadeBoard
    {

        private object[] itemsField;

        private byte mapperField;

        private string typeField;

        /// <remarks/>
        [XmlElement("chr", typeof(DatabaseGameArcadeBoardChr))]
        [XmlElement("pad", typeof(DatabaseGameArcadeBoardPad))]
        [XmlElement("prg", typeof(DatabaseGameArcadeBoardPrg))]
        [XmlElement("vram", typeof(DatabaseGameArcadeBoardVram))]
        [XmlElement("wram", typeof(DatabaseGameArcadeBoardWram))]
        public object[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public byte mapper
        {
            get
            {
                return this.mapperField;
            }
            set
            {
                this.mapperField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class DatabaseGameArcadeBoardChr
    {

        private string sizeField;

        /// <remarks/>
        [XmlAttribute]
        public string size
        {
            get
            {
                return this.sizeField;
            }
            set
            {
                this.sizeField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class DatabaseGameArcadeBoardPad
    {

        private byte hField;

        private byte vField;

        /// <remarks/>
        [XmlAttribute]
        public byte h
        {
            get
            {
                return this.hField;
            }
            set
            {
                this.hField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public byte v
        {
            get
            {
                return this.vField;
            }
            set
            {
                this.vField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class DatabaseGameArcadeBoardPrg
    {

        private string sizeField;

        /// <remarks/>
        [XmlAttribute]
        public string size
        {
            get
            {
                return this.sizeField;
            }
            set
            {
                this.sizeField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class DatabaseGameArcadeBoardVram
    {

        private string sizeField;

        /// <remarks/>
        [XmlAttribute]
        public string size
        {
            get
            {
                return this.sizeField;
            }
            set
            {
                this.sizeField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class DatabaseGameArcadeBoardWram
    {

        private string sizeField;

        private byte batteryField;

        private bool batteryFieldSpecified;

        /// <remarks/>
        [XmlAttribute]
        public string size
        {
            get
            {
                return this.sizeField;
            }
            set
            {
                this.sizeField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public byte battery
        {
            get
            {
                return this.batteryField;
            }
            set
            {
                this.batteryField = value;
            }
        }

        /// <remarks/>
        [XmlIgnoreAttribute()]
        public bool batterySpecified
        {
            get
            {
                return this.batteryFieldSpecified;
            }
            set
            {
                this.batteryFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class DatabaseGameCartridge
    {

        private DatabaseGameCartridgeBoard boardField;

        private string systemField;

        private string dumpField;

        private string crcField;

        private string sha1Field;

        /// <remarks/>
        public DatabaseGameCartridgeBoard board
        {
            get
            {
                return this.boardField;
            }
            set
            {
                this.boardField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public string system
        {
            get
            {
                return this.systemField;
            }
            set
            {
                this.systemField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public string dump
        {
            get
            {
                return this.dumpField;
            }
            set
            {
                this.dumpField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public string crc
        {
            get
            {
                return this.crcField;
            }
            set
            {
                this.crcField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public string sha1
        {
            get
            {
                return this.sha1Field;
            }
            set
            {
                this.sha1Field = value;
            }
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class DatabaseGameCartridgeBoard
    {

        private object[] itemsField;

        private string typeField;

        private byte mapperField;

        private bool mapperFieldSpecified;

        /// <remarks/>
        [XmlElement("chip", typeof(DatabaseGameCartridgeBoardChip))]
        [XmlElement("chr", typeof(DatabaseGameCartridgeBoardChr))]
        [XmlElement("pad", typeof(DatabaseGameCartridgeBoardPad))]
        [XmlElement("prg", typeof(DatabaseGameCartridgeBoardPrg))]
        [XmlElement("vram", typeof(DatabaseGameCartridgeBoardVram))]
        [XmlElement("wram", typeof(DatabaseGameCartridgeBoardWram))]
        public object[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public byte mapper
        {
            get
            {
                return this.mapperField;
            }
            set
            {
                this.mapperField = value;
            }
        }

        /// <remarks/>
        [XmlIgnoreAttribute()]
        public bool mapperSpecified
        {
            get
            {
                return this.mapperFieldSpecified;
            }
            set
            {
                this.mapperFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class DatabaseGameCartridgeBoardChip
    {

        private DatabaseGameCartridgeBoardChipPin[] pinField;

        private string typeField;

        private byte batteryField;

        private bool batteryFieldSpecified;

        /// <remarks/>
        [XmlElement("pin")]
        public DatabaseGameCartridgeBoardChipPin[] pin
        {
            get
            {
                return this.pinField;
            }
            set
            {
                this.pinField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public byte battery
        {
            get
            {
                return this.batteryField;
            }
            set
            {
                this.batteryField = value;
            }
        }

        /// <remarks/>
        [XmlIgnoreAttribute()]
        public bool batterySpecified
        {
            get
            {
                return this.batteryFieldSpecified;
            }
            set
            {
                this.batteryFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class DatabaseGameCartridgeBoardChipPin
    {

        private byte numberField;

        private string functionField;

        /// <remarks/>
        [XmlAttribute]
        public byte number
        {
            get
            {
                return this.numberField;
            }
            set
            {
                this.numberField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public string function
        {
            get
            {
                return this.functionField;
            }
            set
            {
                this.functionField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class DatabaseGameCartridgeBoardChr
    {

        private DatabaseGameCartridgeBoardChrPin[] pinField;

        private string sizeField;

        /// <remarks/>
        [XmlElement("pin")]
        public DatabaseGameCartridgeBoardChrPin[] pin
        {
            get
            {
                return this.pinField;
            }
            set
            {
                this.pinField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public string size
        {
            get
            {
                return this.sizeField;
            }
            set
            {
                this.sizeField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class DatabaseGameCartridgeBoardChrPin
    {

        private byte numberField;

        private string functionField;

        /// <remarks/>
        [XmlAttribute]
        public byte number
        {
            get
            {
                return this.numberField;
            }
            set
            {
                this.numberField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public string function
        {
            get
            {
                return this.functionField;
            }
            set
            {
                this.functionField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class DatabaseGameCartridgeBoardPad
    {

        private byte hField;

        private byte vField;

        /// <remarks/>
        [XmlAttribute]
        public byte h
        {
            get
            {
                return this.hField;
            }
            set
            {
                this.hField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public byte v
        {
            get
            {
                return this.vField;
            }
            set
            {
                this.vField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class DatabaseGameCartridgeBoardPrg
    {

        private string sizeField;

        /// <remarks/>
        [XmlAttribute]
        public string size
        {
            get
            {
                return this.sizeField;
            }
            set
            {
                this.sizeField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class DatabaseGameCartridgeBoardVram
    {

        private string sizeField;

        /// <remarks/>
        [XmlAttribute]
        public string size
        {
            get
            {
                return this.sizeField;
            }
            set
            {
                this.sizeField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class DatabaseGameCartridgeBoardWram
    {

        private string sizeField;

        private byte batteryField;

        private bool batteryFieldSpecified;

        /// <remarks/>
        [XmlAttribute]
        public string size
        {
            get
            {
                return this.sizeField;
            }
            set
            {
                this.sizeField = value;
            }
        }

        /// <remarks/>
        [XmlAttribute]
        public byte battery
        {
            get
            {
                return this.batteryField;
            }
            set
            {
                this.batteryField = value;
            }
        }

        /// <remarks/>
        [XmlIgnoreAttribute()]
        public bool batterySpecified
        {
            get
            {
                return this.batteryFieldSpecified;
            }
            set
            {
                this.batteryFieldSpecified = value;
            }
        }
    }
}
