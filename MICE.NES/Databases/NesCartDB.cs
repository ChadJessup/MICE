namespace MICE.Nintendo.Databases.NesCartDB
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class database
    {

        private DatabaseGame[] gameField;

        private decimal versionField;

        private string conformanceField;

        private string agentField;

        private string authorField;

        private string timestampField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("game")]
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
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

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string agent
        {
            get
            {
                return this.agentField;
            }
            set
            {
                this.agentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string author
        {
            get
            {
                return this.authorField;
            }
            set
            {
                this.authorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string timestamp
        {
            get
            {
                return this.timestampField;
            }
            set
            {
                this.timestampField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DatabaseGame
    {

        private DatabaseGameDevice[] peripheralsField;

        private DatabaseGameCartridge[] cartridgeField;

        private string nameField;

        private string altnameField;

        private string classField;

        private string catalogField;

        private string publisherField;

        private string developerField;

        private string regionField;

        private byte playersField;

        private string dateField;

        private string subclassField;

        private string portdeveloperField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("device", IsNullable = false)]
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
        [System.Xml.Serialization.XmlElementAttribute("cartridge")]
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

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string altname
        {
            get
            {
                return this.altnameField;
            }
            set
            {
                this.altnameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string @class
        {
            get
            {
                return this.classField;
            }
            set
            {
                this.classField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string catalog
        {
            get
            {
                return this.catalogField;
            }
            set
            {
                this.catalogField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string publisher
        {
            get
            {
                return this.publisherField;
            }
            set
            {
                this.publisherField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string developer
        {
            get
            {
                return this.developerField;
            }
            set
            {
                this.developerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string region
        {
            get
            {
                return this.regionField;
            }
            set
            {
                this.regionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte players
        {
            get
            {
                return this.playersField;
            }
            set
            {
                this.playersField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string date
        {
            get
            {
                return this.dateField;
            }
            set
            {
                this.dateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string subclass
        {
            get
            {
                return this.subclassField;
            }
            set
            {
                this.subclassField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string portdeveloper
        {
            get
            {
                return this.portdeveloperField;
            }
            set
            {
                this.portdeveloperField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DatabaseGameDevice
    {

        private string typeField;

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DatabaseGameCartridge
    {

        private DatabaseGameCartridgeBoard boardField;

        private string systemField;

        private string crcField;

        private string sha1Field;

        private string dumpField;

        private string dumperField;

        private System.DateTime datedumpedField;

        private string revisionField;

        private byte prototypeField;

        private bool prototypeFieldSpecified;

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
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
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

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string dumper
        {
            get
            {
                return this.dumperField;
            }
            set
            {
                this.dumperField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime datedumped
        {
            get
            {
                return this.datedumpedField;
            }
            set
            {
                this.datedumpedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string revision
        {
            get
            {
                return this.revisionField;
            }
            set
            {
                this.revisionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte prototype
        {
            get
            {
                return this.prototypeField;
            }
            set
            {
                this.prototypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool prototypeSpecified
        {
            get
            {
                return this.prototypeFieldSpecified;
            }
            set
            {
                this.prototypeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DatabaseGameCartridgeBoard
    {

        private object[] itemsField;

        private string typeField;

        private string pcbField;

        private byte mapperField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("chip", typeof(DatabaseGameCartridgeBoardChip))]
        [System.Xml.Serialization.XmlElementAttribute("chr", typeof(DatabaseGameCartridgeBoardChr))]
        [System.Xml.Serialization.XmlElementAttribute("cic", typeof(DatabaseGameCartridgeBoardCic))]
        [System.Xml.Serialization.XmlElementAttribute("pad", typeof(DatabaseGameCartridgeBoardPad))]
        [System.Xml.Serialization.XmlElementAttribute("prg", typeof(DatabaseGameCartridgeBoardPrg))]
        [System.Xml.Serialization.XmlElementAttribute("vram", typeof(DatabaseGameCartridgeBoardVram))]
        [System.Xml.Serialization.XmlElementAttribute("wram", typeof(DatabaseGameCartridgeBoardWram))]
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string pcb
        {
            get
            {
                return this.pcbField;
            }
            set
            {
                this.pcbField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DatabaseGameCartridgeBoardChip
    {

        private DatabaseGameCartridgeBoardChipPin[] pinField;

        private string typeField;

        private byte batteryField;

        private bool batteryFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("pin")]
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
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
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DatabaseGameCartridgeBoardChipPin
    {

        private byte numberField;

        private string functionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DatabaseGameCartridgeBoardChr
    {

        private DatabaseGameCartridgeBoardChrPin[] pinField;

        private string sizeField;

        private string crcField;

        private string sha1Field;

        private string nameField;

        private byte idField;

        private bool idFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("pin")]
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
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

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idSpecified
        {
            get
            {
                return this.idFieldSpecified;
            }
            set
            {
                this.idFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DatabaseGameCartridgeBoardChrPin
    {

        private byte numberField;

        private string functionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DatabaseGameCartridgeBoardCic
    {

        private string typeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DatabaseGameCartridgeBoardPad
    {

        private byte hField;

        private byte vField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DatabaseGameCartridgeBoardPrg
    {

        private string nameField;

        private string sizeField;

        private string crcField;

        private string sha1Field;

        private byte idField;

        private bool idFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
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

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idSpecified
        {
            get
            {
                return this.idFieldSpecified;
            }
            set
            {
                this.idFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DatabaseGameCartridgeBoardVram
    {

        private string sizeField;

        private byte idField;

        private bool idFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idSpecified
        {
            get
            {
                return this.idFieldSpecified;
            }
            set
            {
                this.idFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DatabaseGameCartridgeBoardWram
    {
        private string sizeField;

        private byte batteryField;

        private bool batteryFieldSpecified;

        private byte idField;

        private bool idFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
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

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idSpecified { get; set; }
    }
}
