﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace CodeSmith.Data.Audit
{
    /// <summary>
    /// A class for logging the changes to an entity.
    /// </summary>
    [XmlRoot(Namespace = AuditLog.AuditNamespace, ElementName = "entity")]
    [DebuggerDisplay("Action: {Action}, Type: {Type}")]
    public class AuditEntity : IEquatable<AuditEntity>
    {
        private readonly AuditPropertyCollection _properties = new AuditPropertyCollection();
        private readonly AuditKeyCollection _keys = new AuditKeyCollection();

        /// <summary>
        /// Gets or sets the action that was taken on the entity.
        /// </summary>
        /// <value>The action that was taken on the entity.</value>
        [XmlAttribute("action")]
        public AuditAction Action { get; set; }

        /// <summary>
        /// Gets or sets the data type of the entity.
        /// </summary>
        /// <value>The data type of the entity.</value>
        [XmlAttribute("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets the list of properties that are the key for the entity.
        /// </summary>
        /// <value>The list of properties that are the key for the entity.</value>
        [XmlElement("key", typeof(AuditKey))]
        public AuditKeyCollection Keys
        {
            get { return _keys; }
        }

        /// <summary>
        /// Gets the entity in its original unmodified state.
        /// </summary>
        [XmlIgnore]
        public object Original { get; set; }

        /// <summary>
        /// Gets the entity in its current modified state.
        /// </summary>
        [XmlIgnore]
        public object Current { get; set; }

        /// <summary>
        /// Gets the list of properties that action was taken on.
        /// </summary>
        /// <value>The list of properties that action was taken on.</value>
        [XmlElement("property", typeof(AuditProperty))]
        public AuditPropertyCollection Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(AuditEntity other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return GetHashCode().Equals(other.GetHashCode()); ;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof (AuditEntity))
                return false;

            return Equals((AuditEntity) obj);
        }

        private const int HASH_SEED = 397;

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object" />.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                // using the Type, Action and Key values
                int result = (Type != null ? Type.GetHashCode() : 0);
                result = (result * HASH_SEED) ^ Action.GetHashCode();

                foreach (var key in _keys)
                    if (key.Value != null)
                        result = (result * HASH_SEED) ^ key.Value.GetHashCode();

                return result;
            }
        }
    }
}