﻿// <copyright file="MetricSnapshotGraphitePlainTextProtocolWriter.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using App.Metrics.Formatters.Graphite.Internal;
using App.Metrics.Serialization;

namespace App.Metrics.Formatters.Graphite
{
    public class MetricSnapshotGraphitePlainTextProtocolWriter : IMetricSnapshotWriter
    {
        private readonly TextWriter _textWriter;
        private readonly IGraphitePointTextWriter _metricNameFormatter;
        private readonly GraphitePoints _points;

        public MetricSnapshotGraphitePlainTextProtocolWriter(
            TextWriter textWriter,
            IGraphitePointTextWriter metricNameFormatter = null,
            GeneratedMetricNameMapping dataKeys = null)
        {
            _textWriter = textWriter ?? throw new ArgumentNullException(nameof(textWriter));
            _points = new GraphitePoints();

            _metricNameFormatter = metricNameFormatter ?? new DefaultGraphitePointTextWriter();

            MetricNameMapping = dataKeys ?? new GeneratedMetricNameMapping();
        }

        /// <inheritdoc />
        public GeneratedMetricNameMapping MetricNameMapping { get; }

        /// <inheritdoc />
        public void Write(string context, string name, object value, MetricTags tags, DateTime timestamp)
        {
            _points.Add(new GraphitePoint(context, name, new Dictionary<string, object> { { "value", value } }, tags, _metricNameFormatter, timestamp));
        }

        /// <inheritdoc />
        public void Write(string context, string name, IEnumerable<string> columns, IEnumerable<object> values, MetricTags tags, DateTime timestamp)
        {
            var fields = columns.Zip(values, (column, data) => new { column, data }).ToDictionary(pair => pair.column, pair => pair.data);

            _points.Add(new GraphitePoint(context, name, fields, tags, _metricNameFormatter, timestamp));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        ///     unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _points.Write(_textWriter);
#if !NETSTANDARD1_6
                _textWriter?.Close();
#endif
                _textWriter?.Dispose();
            }
        }
    }
}
