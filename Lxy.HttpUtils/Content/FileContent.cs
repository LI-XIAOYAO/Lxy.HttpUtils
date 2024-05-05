using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Lxy.HttpUtils
{
    /// <summary>
    /// <inheritdoc cref="MultipartFormDataContent"/>
    /// </summary>
    public sealed class FileContent : MultipartFormDataContent
    {
        private readonly List<(string Name, string Value, Encoding encoding)> _stringContents = new List<(string Name, string Value, Encoding encoding)>();

        /// <summary>
        /// <inheritdoc cref="MultipartFormDataContent"/>
        /// </summary>
        public FileContent()
        {
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="boundary"></param>
        public FileContent(string boundary) : base(boundary)
        {
        }

        /// <summary>
        /// Add
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="encoding"></param>
        public FileContent Add(string name, string value, Encoding encoding = null)
        {
            if (null == name)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (null == value)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _stringContents.Add((name, value, encoding));

            return this;
        }

        /// <summary>
        /// Add file.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fileName"></param>
        /// <param name="stream"></param>
        public FileContent Add(string name, string fileName, Stream stream)
        {
            if (null == name)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (null == fileName)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (null == stream)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            Add(new StreamContent(stream), name, fileName);

            return this;
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="boundary"></param>
        /// <param name="isQuotedBoundary"></param>
        /// <returns></returns>
        public static FileContent Create(string boundary = null, bool isQuotedBoundary = false)
        {
            var fileContent = new FileContent();

            if (null != boundary)
            {
                if (string.IsNullOrWhiteSpace(boundary))
                {
                    throw new ArgumentException(nameof(boundary));
                }

                fileContent.Headers.ContentType.Parameters.Clear();
                fileContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue(nameof(boundary), isQuotedBoundary ? $@"""{boundary}""" : boundary));
            }

            return fileContent;
        }

        /// <summary>
        /// Build
        /// </summary>
        /// <param name="encoding"></param>
        internal FileContent Build(Encoding encoding)
        {
            foreach (var item in _stringContents)
            {
                Add(new StringContent(item.Value, item.encoding ?? encoding ?? Encoding.Default), item.Name);
            }

            _stringContents.Clear();

            return this;
        }

        /// <summary>
        /// <inheritdoc cref="MultipartContent.Dispose(bool)"/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _stringContents.Clear();
            }
        }
    }
}