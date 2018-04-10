using System;
using System.Collections.Generic;
using System.Text;

namespace MICE.Nintendo.NstDatabase
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class database
    {

        private databaseGame[] gameField;

        private decimal versionField;

        private string conformanceField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("game")]
        public databaseGame[] game
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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class databaseGame
    {

        private databaseGameDevice[] peripheralsField;

        private databaseGameArcade arcadeField;

        private databaseGameCartridge[] cartridgeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("device", IsNullable = false)]
        public databaseGameDevice[] peripherals
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
        public databaseGameArcade arcade
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
        [System.Xml.Serialization.XmlElementAttribute("cartridge")]
        public databaseGameCartridge[] cartridge
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
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class databaseGameDevice
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
    public partial class databaseGameArcade
    {

        private databaseGameArcadeBoard boardField;

        private string systemField;

        private string dumpField;

        private string crcField;

        private string sha1Field;

        /// <remarks/>
        public databaseGameArcadeBoard board
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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class databaseGameArcadeBoard
    {

        private object[] itemsField;

        private byte mapperField;

        private string typeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("chr", typeof(databaseGameArcadeBoardChr))]
        [System.Xml.Serialization.XmlElementAttribute("pad", typeof(databaseGameArcadeBoardPad))]
        [System.Xml.Serialization.XmlElementAttribute("prg", typeof(databaseGameArcadeBoardPrg))]
        [System.Xml.Serialization.XmlElementAttribute("vram", typeof(databaseGameArcadeBoardVram))]
        [System.Xml.Serialization.XmlElementAttribute("wram", typeof(databaseGameArcadeBoardWram))]
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
    public partial class databaseGameArcadeBoardChr
    {

        private string sizeField;

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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class databaseGameArcadeBoardPad
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
    public partial class databaseGameArcadeBoardPrg
    {

        private string sizeField;

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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class databaseGameArcadeBoardVram
    {

        private string sizeField;

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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class databaseGameArcadeBoardWram
    {

        private string sizeField;

        private byte batteryField;

        private bool batteryFieldSpecified;

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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class databaseGameCartridge
    {

        private databaseGameCartridgeBoard boardField;

        private string systemField;

        private string dumpField;

        private string crcField;

        private string sha1Field;

        /// <remarks/>
        public databaseGameCartridgeBoard board
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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class databaseGameCartridgeBoard
    {

        private object[] itemsField;

        private string typeField;

        private byte mapperField;

        private bool mapperFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("chip", typeof(databaseGameCartridgeBoardChip))]
        [System.Xml.Serialization.XmlElementAttribute("chr", typeof(databaseGameCartridgeBoardChr))]
        [System.Xml.Serialization.XmlElementAttribute("pad", typeof(databaseGameCartridgeBoardPad))]
        [System.Xml.Serialization.XmlElementAttribute("prg", typeof(databaseGameCartridgeBoardPrg))]
        [System.Xml.Serialization.XmlElementAttribute("vram", typeof(databaseGameCartridgeBoardVram))]
        [System.Xml.Serialization.XmlElementAttribute("wram", typeof(databaseGameCartridgeBoardWram))]
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
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
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class databaseGameCartridgeBoardChip
    {

        private databaseGameCartridgeBoardChipPin[] pinField;

        private string typeField;

        private byte batteryField;

        private bool batteryFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("pin")]
        public databaseGameCartridgeBoardChipPin[] pin
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
    public partial class databaseGameCartridgeBoardChipPin
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
    public partial class databaseGameCartridgeBoardChr
    {

        private databaseGameCartridgeBoardChrPin[] pinField;

        private string sizeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("pin")]
        public databaseGameCartridgeBoardChrPin[] pin
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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class databaseGameCartridgeBoardChrPin
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
    public partial class databaseGameCartridgeBoardPad
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
    public partial class databaseGameCartridgeBoardPrg
    {

        private string sizeField;

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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class databaseGameCartridgeBoardVram
    {

        private string sizeField;

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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class databaseGameCartridgeBoardWram
    {

        private string sizeField;

        private byte batteryField;

        private bool batteryFieldSpecified;

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
    }
}
