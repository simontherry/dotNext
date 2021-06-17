using System;
using System.Net.Mime;

namespace DotNext.Net.Cluster.Messaging
{
    using Runtime.Serialization;

    /// <summary>
    /// Indicates that the type can be used as message payload.
    /// </summary>
    /// <seealso cref="MessagingClient"/>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class MessageAttribute : SerializableAttribute
    {
        private string? mimeType;

        /// <summary>
        /// Initializes a new instance of the attribute.
        /// </summary>
        /// <param name="name">The name of the message.</param>
        public MessageAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Gets the name of the message.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets MIME type of the message.
        /// </summary>
        public string MimeType
        {
            get => mimeType.IfNullOrEmpty(MediaTypeNames.Application.Octet);
            set => mimeType = value;
        }
    }
}