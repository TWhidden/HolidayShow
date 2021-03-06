﻿#pragma warning disable 1591
#pragma warning disable 0414        
//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
// </autogenerated>
//------------------------------------------------------------------------------
using System;
using System.Linq;

namespace HolidayShow.Data
{
    /// <summary>
    /// The class representing the dbo.DevicePatternSequences table.
    /// </summary>
    [System.Data.Linq.Mapping.Table(Name="dbo.DevicePatternSequences")]
    [System.Runtime.Serialization.DataContract(IsReference = true)]
    [System.ComponentModel.DataAnnotations.ScaffoldTable(true)]
    [System.ComponentModel.DataAnnotations.MetadataType(typeof(HolidayShow.Data.DevicePatternSequences.Metadata))]
    [System.Data.Services.Common.DataServiceKey("DevicePatternSeqenceId")]
    [System.Diagnostics.DebuggerDisplay("DevicePatternSeqenceId: {DevicePatternSeqenceId}")]
    public partial class DevicePatternSequences
        : LinqEntityBase, ICloneable 
    {
        #region Static Constructor
        /// <summary>
        /// Initializes the <see cref="DevicePatternSequences"/> class.
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        static DevicePatternSequences()
        {
            AddSharedRules();
        }
        #endregion

        #region Default Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="DevicePatternSequences"/> class.
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCode]
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public DevicePatternSequences()
        {
            Initialize();
        }

        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        private void Initialize()
        {
            _audioOptions = default(System.Data.Linq.EntityRef<AudioOptions>);
            _deviceIoPorts = default(System.Data.Linq.EntityRef<DeviceIoPorts>);
            _devicePatterns = default(System.Data.Linq.EntityRef<DevicePatterns>);
            OnCreated();
        }
        #endregion

        #region Column Mapped Properties

        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        private int _devicePatternSeqenceId = default(int);

        /// <summary>
        /// Gets the DevicePatternSeqenceId column value.
        /// </summary>
        [System.Data.Linq.Mapping.Column(Name = "DevicePatternSeqenceId", Storage = "_devicePatternSeqenceId", DbType = "int NOT NULL IDENTITY", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
        [System.Runtime.Serialization.DataMember(Order = 1)]
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public int DevicePatternSeqenceId
        {
            get { return _devicePatternSeqenceId; }
            set
            {
                if (_devicePatternSeqenceId != value)
                {
                    OnDevicePatternSeqenceIdChanging(value);
                    SendPropertyChanging("DevicePatternSeqenceId");
                    _devicePatternSeqenceId = value;
                    SendPropertyChanged("DevicePatternSeqenceId");
                    OnDevicePatternSeqenceIdChanged();
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        private int _devicePatternId;

        /// <summary>
        /// Gets or sets the DevicePatternId column value.
        /// </summary>
        [System.Data.Linq.Mapping.Column(Name = "DevicePatternId", Storage = "_devicePatternId", DbType = "int NOT NULL", CanBeNull = false, UpdateCheck = System.Data.Linq.Mapping.UpdateCheck.Never)]
        [System.Runtime.Serialization.DataMember(Order = 2)]
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public int DevicePatternId
        {
            get { return _devicePatternId; }
            set
            {
                if (_devicePatternId != value)
                {
                    if (_devicePatterns.HasLoadedOrAssignedValue)
                    {
                        throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
                    }
                    OnDevicePatternIdChanging(value);
                    SendPropertyChanging("DevicePatternId");
                    _devicePatternId = value;
                    SendPropertyChanged("DevicePatternId");
                    OnDevicePatternIdChanged();
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        private int _onAt;

        /// <summary>
        /// Gets or sets the OnAt column value.
        /// </summary>
        [System.Data.Linq.Mapping.Column(Name = "OnAt", Storage = "_onAt", DbType = "int NOT NULL", CanBeNull = false, UpdateCheck = System.Data.Linq.Mapping.UpdateCheck.Never)]
        [System.Runtime.Serialization.DataMember(Order = 3)]
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public int OnAt
        {
            get { return _onAt; }
            set
            {
                if (_onAt != value)
                {
                    OnOnAtChanging(value);
                    SendPropertyChanging("OnAt");
                    _onAt = value;
                    SendPropertyChanged("OnAt");
                    OnOnAtChanged();
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        private int _duration;

        /// <summary>
        /// Gets or sets the Duration column value.
        /// </summary>
        [System.Data.Linq.Mapping.Column(Name = "Duration", Storage = "_duration", DbType = "int NOT NULL", CanBeNull = false, UpdateCheck = System.Data.Linq.Mapping.UpdateCheck.Never)]
        [System.Runtime.Serialization.DataMember(Order = 4)]
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public int Duration
        {
            get { return _duration; }
            set
            {
                if (_duration != value)
                {
                    OnDurationChanging(value);
                    SendPropertyChanging("Duration");
                    _duration = value;
                    SendPropertyChanged("Duration");
                    OnDurationChanged();
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        private int _audioId;

        /// <summary>
        /// Gets or sets the AudioId column value.
        /// </summary>
        [System.Data.Linq.Mapping.Column(Name = "AudioId", Storage = "_audioId", DbType = "int NOT NULL", CanBeNull = false, UpdateCheck = System.Data.Linq.Mapping.UpdateCheck.Never)]
        [System.Runtime.Serialization.DataMember(Order = 5)]
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public int AudioId
        {
            get { return _audioId; }
            set
            {
                if (_audioId != value)
                {
                    if (_audioOptions.HasLoadedOrAssignedValue)
                    {
                        throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
                    }
                    OnAudioIdChanging(value);
                    SendPropertyChanging("AudioId");
                    _audioId = value;
                    SendPropertyChanged("AudioId");
                    OnAudioIdChanged();
                }
            }
        }

        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        private int _deviceIoPortId;

        /// <summary>
        /// Gets or sets the DeviceIoPortId column value.
        /// </summary>
        [System.Data.Linq.Mapping.Column(Name = "DeviceIoPortId", Storage = "_deviceIoPortId", DbType = "int NOT NULL", CanBeNull = false, UpdateCheck = System.Data.Linq.Mapping.UpdateCheck.Never)]
        [System.Runtime.Serialization.DataMember(Order = 6)]
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public int DeviceIoPortId
        {
            get { return _deviceIoPortId; }
            set
            {
                if (_deviceIoPortId != value)
                {
                    if (_deviceIoPorts.HasLoadedOrAssignedValue)
                    {
                        throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
                    }
                    OnDeviceIoPortIdChanging(value);
                    SendPropertyChanging("DeviceIoPortId");
                    _deviceIoPortId = value;
                    SendPropertyChanged("DeviceIoPortId");
                    OnDeviceIoPortIdChanged();
                }
            }
        }
        #endregion

        #region Association Mapped Properties

        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        private System.Data.Linq.EntityRef<AudioOptions> _audioOptions;

        /// <summary>
        /// Gets or sets the <see cref="AudioOptions"/> association.
        /// </summary>
        [System.Data.Linq.Mapping.Association(Name = "AudioOptions_DevicePatternSequences", Storage = "_audioOptions", ThisKey = "AudioId", OtherKey = "AudioId", IsForeignKey = true, DeleteRule = "CASCADE")]
        [System.Runtime.Serialization.DataMember(Order = 7, EmitDefaultValue = false)]
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public AudioOptions AudioOptions
        {
            get { return (serializing && !_audioOptions.HasLoadedOrAssignedValue) ? null : _audioOptions.Entity; }
            set
            {
                AudioOptions previousValue = _audioOptions.Entity;
                if (previousValue != value || _audioOptions.HasLoadedOrAssignedValue == false)
                {
                    OnAudioOptionsChanging(value);
                    SendPropertyChanging("AudioOptions");
                    if (previousValue != null)
                    {
                        _audioOptions.Entity = null;
                        previousValue.DevicePatternSequencesList.Remove(this);
                    }
                    _audioOptions.Entity = value;
                    if (value != null)
                    {
                        value.DevicePatternSequencesList.Add(this);
                        _audioId = value.AudioId;
                    }
                    else
                    {
                        _audioId = default(int);
                    }
                    SendPropertyChanged("AudioOptions");
                    OnAudioOptionsChanged();
                }
            }
        }
        
        
        

        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        private System.Data.Linq.EntityRef<DeviceIoPorts> _deviceIoPorts;

        /// <summary>
        /// Gets or sets the <see cref="DeviceIoPorts"/> association.
        /// </summary>
        [System.Data.Linq.Mapping.Association(Name = "DeviceIoPorts_DevicePatternSequences", Storage = "_deviceIoPorts", ThisKey = "DeviceIoPortId", OtherKey = "DeviceIoPortId", IsForeignKey = true)]
        [System.Runtime.Serialization.DataMember(Order = 8, EmitDefaultValue = false)]
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public DeviceIoPorts DeviceIoPorts
        {
            get { return (serializing && !_deviceIoPorts.HasLoadedOrAssignedValue) ? null : _deviceIoPorts.Entity; }
            set
            {
                DeviceIoPorts previousValue = _deviceIoPorts.Entity;
                if (previousValue != value || _deviceIoPorts.HasLoadedOrAssignedValue == false)
                {
                    OnDeviceIoPortsChanging(value);
                    SendPropertyChanging("DeviceIoPorts");
                    if (previousValue != null)
                    {
                        _deviceIoPorts.Entity = null;
                        previousValue.DevicePatternSequencesList.Remove(this);
                    }
                    _deviceIoPorts.Entity = value;
                    if (value != null)
                    {
                        value.DevicePatternSequencesList.Add(this);
                        _deviceIoPortId = value.DeviceIoPortId;
                    }
                    else
                    {
                        _deviceIoPortId = default(int);
                    }
                    SendPropertyChanged("DeviceIoPorts");
                    OnDeviceIoPortsChanged();
                }
            }
        }
        
        
        

        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        private System.Data.Linq.EntityRef<DevicePatterns> _devicePatterns;

        /// <summary>
        /// Gets or sets the <see cref="DevicePatterns"/> association.
        /// </summary>
        [System.Data.Linq.Mapping.Association(Name = "DevicePatterns_DevicePatternSequences", Storage = "_devicePatterns", ThisKey = "DevicePatternId", OtherKey = "DevicePatternId", IsForeignKey = true, DeleteRule = "CASCADE")]
        [System.Runtime.Serialization.DataMember(Order = 9, EmitDefaultValue = false)]
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public DevicePatterns DevicePatterns
        {
            get { return (serializing && !_devicePatterns.HasLoadedOrAssignedValue) ? null : _devicePatterns.Entity; }
            set
            {
                DevicePatterns previousValue = _devicePatterns.Entity;
                if (previousValue != value || _devicePatterns.HasLoadedOrAssignedValue == false)
                {
                    OnDevicePatternsChanging(value);
                    SendPropertyChanging("DevicePatterns");
                    if (previousValue != null)
                    {
                        _devicePatterns.Entity = null;
                        previousValue.DevicePatternSequencesList.Remove(this);
                    }
                    _devicePatterns.Entity = value;
                    if (value != null)
                    {
                        value.DevicePatternSequencesList.Add(this);
                        _devicePatternId = value.DevicePatternId;
                    }
                    else
                    {
                        _devicePatternId = default(int);
                    }
                    SendPropertyChanged("DevicePatterns");
                    OnDevicePatternsChanged();
                }
            }
        }
        
        
        
        #endregion

        #region Extensibility Method Definitions
        /// <summary>Called by the static constructor to add shared rules.</summary>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        static partial void AddSharedRules();
        /// <summary>Called when this instance is loaded.</summary>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnLoaded();
        /// <summary>Called when this instance is being saved.</summary>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnValidate(System.Data.Linq.ChangeAction action);
        /// <summary>Called when this instance is created.</summary>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnCreated();
        /// <summary>Called when <see cref="DevicePatternSeqenceId"/> is changing.</summary>
        /// <param name="value">The new value.</param>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnDevicePatternSeqenceIdChanging(int value);
        /// <summary>Called after <see cref="DevicePatternSeqenceId"/> has Changed.</summary>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnDevicePatternSeqenceIdChanged();
        /// <summary>Called when <see cref="DevicePatternId"/> is changing.</summary>
        /// <param name="value">The new value.</param>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnDevicePatternIdChanging(int value);
        /// <summary>Called after <see cref="DevicePatternId"/> has Changed.</summary>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnDevicePatternIdChanged();
        /// <summary>Called when <see cref="OnAt"/> is changing.</summary>
        /// <param name="value">The new value.</param>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnOnAtChanging(int value);
        /// <summary>Called after <see cref="OnAt"/> has Changed.</summary>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnOnAtChanged();
        /// <summary>Called when <see cref="Duration"/> is changing.</summary>
        /// <param name="value">The new value.</param>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnDurationChanging(int value);
        /// <summary>Called after <see cref="Duration"/> has Changed.</summary>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnDurationChanged();
        /// <summary>Called when <see cref="AudioId"/> is changing.</summary>
        /// <param name="value">The new value.</param>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnAudioIdChanging(int value);
        /// <summary>Called after <see cref="AudioId"/> has Changed.</summary>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnAudioIdChanged();
        /// <summary>Called when <see cref="DeviceIoPortId"/> is changing.</summary>
        /// <param name="value">The new value.</param>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnDeviceIoPortIdChanging(int value);
        /// <summary>Called after <see cref="DeviceIoPortId"/> has Changed.</summary>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnDeviceIoPortIdChanged();
        /// <summary>Called when <see cref="AudioOptions"/> is changing.</summary>
        /// <param name="value">The new value.</param>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnAudioOptionsChanging(AudioOptions value);
        /// <summary>Called after <see cref="AudioOptions"/> has Changed.</summary>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnAudioOptionsChanged();
        /// <summary>Called when <see cref="DeviceIoPorts"/> is changing.</summary>
        /// <param name="value">The new value.</param>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnDeviceIoPortsChanging(DeviceIoPorts value);
        /// <summary>Called after <see cref="DeviceIoPorts"/> has Changed.</summary>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnDeviceIoPortsChanged();
        /// <summary>Called when <see cref="DevicePatterns"/> is changing.</summary>
        /// <param name="value">The new value.</param>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnDevicePatternsChanging(DevicePatterns value);
        /// <summary>Called after <see cref="DevicePatterns"/> has Changed.</summary>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        partial void OnDevicePatternsChanged();

        #endregion

        #region Serialization
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        private bool serializing;

        /// <summary>
        /// Called when serializing.
        /// </summary>
        /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> for the serialization.</param>
        [System.Runtime.Serialization.OnSerializing]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public void OnSerializing(System.Runtime.Serialization.StreamingContext context) {
            serializing = true;
        }

        /// <summary>
        /// Called when serialized.
        /// </summary>
        /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> for the serialization.</param>
        [System.Runtime.Serialization.OnSerialized]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public void OnSerialized(System.Runtime.Serialization.StreamingContext context) {
            serializing = false;
        }

        /// <summary>
        /// Called when deserializing.
        /// </summary>
        /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> for the serialization.</param>
        [System.Runtime.Serialization.OnDeserializing]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public void OnDeserializing(System.Runtime.Serialization.StreamingContext context) {
            Initialize();
        }

        /// <summary>
        /// Deserializes an instance of <see cref="DevicePatternSequences"/> from XML.
        /// </summary>
        /// <param name="xml">The XML string representing a <see cref="DevicePatternSequences"/> instance.</param>
        /// <returns>An instance of <see cref="DevicePatternSequences"/> that is deserialized from the XML string.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static DevicePatternSequences FromXml(string xml)
        {
            var deserializer = new System.Runtime.Serialization.DataContractSerializer(typeof(DevicePatternSequences));

            using (var sr = new System.IO.StringReader(xml))
            using (var reader = System.Xml.XmlReader.Create(sr))
            {
                return deserializer.ReadObject(reader) as DevicePatternSequences;
            }
        }

        /// <summary>
        /// Deserializes an instance of <see cref="DevicePatternSequences"/> from a byte array.
        /// </summary>
        /// <param name="buffer">The byte array representing a <see cref="DevicePatternSequences"/> instance.</param>
        /// <returns>An instance of <see cref="DevicePatternSequences"/> that is deserialized from the byte array.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static DevicePatternSequences FromBinary(byte[] buffer)
        {
            var deserializer = new System.Runtime.Serialization.DataContractSerializer(typeof(DevicePatternSequences));

            using (var ms = new System.IO.MemoryStream(buffer))
            using (var reader = System.Xml.XmlDictionaryReader.CreateBinaryReader(ms, System.Xml.XmlDictionaryReaderQuotas.Max))
            {
                return deserializer.ReadObject(reader) as DevicePatternSequences;
            }
        }
        
        #endregion

        #region Clone
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        object ICloneable.Clone()
        {
            var serializer = new System.Runtime.Serialization.DataContractSerializer(GetType());
            using (var ms = new System.IO.MemoryStream())
            {
                serializer.WriteObject(ms, this);
                ms.Position = 0;
                return serializer.ReadObject(ms);
            }
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <remarks>
        /// Only loaded <see cref="T:System.Data.Linq.EntityRef`1"/> and <see cref="T:System.Data.Linq.EntitySet`1" /> child accessions will be cloned.
        /// </remarks>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public DevicePatternSequences Clone()
        {
            return (DevicePatternSequences)((ICloneable)this).Clone();
        }
        #endregion

        #region Detach Methods
        /// <summary>
        /// Detach this instance from the <see cref="System.Data.Linq.DataContext"/>.
        /// </summary>
        /// <remarks>
        /// Detaching the entity will stop all lazy loading and allow it to be added to another <see cref="System.Data.Linq.DataContext"/>.
        /// </remarks>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public override void Detach()
        {
            if (!IsAttached())
                return;

            base.Detach();
            _audioOptions = Detach(_audioOptions);
            _deviceIoPorts = Detach(_deviceIoPorts);
            _devicePatterns = Detach(_devicePatterns);
        }
        #endregion
    }
}
#pragma warning restore 1591
#pragma warning restore 0414
