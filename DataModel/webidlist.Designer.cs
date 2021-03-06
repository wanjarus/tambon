// ------------------------------------------------------------------------------
//  <auto-generated>
//    Generated by Xsd2Code. Version 3.4.3.29394
//    <NameSpace>De.AHoerstemeier.Tambon</NameSpace><Collection>List</Collection><codeType>CSharp</codeType><EnableDataBinding>False</EnableDataBinding><EnableLazyLoading>False</EnableLazyLoading><TrackingChangesEnable>False</TrackingChangesEnable><GenTrackingClasses>False</GenTrackingClasses><HidePrivateFieldInIDE>False</HidePrivateFieldInIDE><EnableSummaryComment>True</EnableSummaryComment><VirtualProp>False</VirtualProp><IncludeSerializeMethod>False</IncludeSerializeMethod><UseBaseClass>False</UseBaseClass><GenBaseClass>False</GenBaseClass><GenerateCloneMethod>False</GenerateCloneMethod><GenerateDataContracts>True</GenerateDataContracts><CodeBaseTag>Net40</CodeBaseTag><SerializeMethodName>Serialize</SerializeMethodName><DeserializeMethodName>Deserialize</DeserializeMethodName><SaveToFileMethodName>SaveToFile</SaveToFileMethodName><LoadFromFileMethodName>LoadFromFile</LoadFromFileMethodName><GenerateXMLAttributes>True</GenerateXMLAttributes><EnableEncoding>False</EnableEncoding><AutomaticProperties>False</AutomaticProperties><GenerateShouldSerialize>False</GenerateShouldSerialize><DisableDebug>False</DisableDebug><PropNameSpecified>Default</PropNameSpecified><Encoder>UTF8</Encoder><CustomUsings></CustomUsings><ExcludeIncludedTypes>True</ExcludeIncludedTypes><EnableInitializeFields>True</EnableInitializeFields>
//  </auto-generated>
// ------------------------------------------------------------------------------
namespace De.AHoerstemeier.Tambon {
    using System;
    using System.Diagnostics;
    using System.Xml.Serialization;
    using System.Collections;
    using System.Xml.Schema;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    
    
    /// <summary>
    /// Translation table between webid and geocode
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1064.2")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://hoerstemeier.com/tambon/")]
    [System.Xml.Serialization.XmlRootAttribute("webid", Namespace="http://hoerstemeier.com/tambon/", IsNullable=false)]
    [System.Runtime.Serialization.DataContractAttribute(Name="WebIdList", Namespace="http://hoerstemeier.com/tambon/", IsReference=true)]
    public partial class WebIdList {
        
        private List<WebIdListEntry> itemField;
        
        /// <summary>
        /// Creates a new instance of WebIdList.
        /// </summary>
        public WebIdList() {
            this.itemField = new List<WebIdListEntry>();
        }
        
        [System.Xml.Serialization.XmlElementAttribute("item", Order=0)]
        [System.Runtime.Serialization.DataMemberAttribute()]
        public List<WebIdListEntry> item {
            get {
                return this.itemField;
            }
            set {
                this.itemField = value;
            }
        }
    }
    
    /// <summary>
    /// Link between geocode and web id.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1064.2")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://hoerstemeier.com/tambon/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://hoerstemeier.com/tambon/", IsNullable=true)]
    [System.Runtime.Serialization.DataContractAttribute(Name="WebIdListEntry", Namespace="http://hoerstemeier.com/tambon/", IsReference=true)]
    public partial class WebIdListEntry {
        
        private int idField;
        
        private uint geocodeField;
        
        private string commentField;
        
        /// <summary>
        /// Id at the website to access the data.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        /// <summary>
        /// Geocode of corresponding entity.
        /// </summary>
        /// <value>
        /// The geocode.
        /// </value>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.Runtime.Serialization.DataMemberAttribute()]
        public uint geocode {
            get {
                return this.geocodeField;
            }
            set {
                this.geocodeField = value;
            }
        }
        
        /// <summary>
        /// Auto generated comment tag to suppress XML code documentation warning.
        /// </summary>
        /// <value>
        /// Auto generated value tag to suppress XML code documentation warning.
        /// </value>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string comment {
            get {
                return this.commentField;
            }
            set {
                this.commentField = value;
            }
        }
    }
}
