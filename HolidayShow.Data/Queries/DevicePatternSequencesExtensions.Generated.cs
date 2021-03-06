﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
// </autogenerated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using CodeSmith.Data.Linq;
using CodeSmith.Data.Linq.Dynamic;

namespace HolidayShow.Data
{
    /// <summary>
    /// The query extension class for DevicePatternSequences.
    /// </summary>
    public static partial class DevicePatternSequencesExtensions
    {

        /// <summary>
        /// Gets an instance by the primary key.
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static HolidayShow.Data.DevicePatternSequences GetByKey(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, int devicePatternSeqenceId)
        {
            var entity = queryable as System.Data.Linq.Table<HolidayShow.Data.DevicePatternSequences>;
            if (entity != null && entity.Context.LoadOptions == null)
                return Query.GetByKey.Invoke((HolidayShow.Data.HolidayShowDataContext)entity.Context, devicePatternSeqenceId);

            return queryable.FirstOrDefault(d => d.DevicePatternSeqenceId == devicePatternSeqenceId);
        }

        /// <summary>
        /// Immediately deletes the entity by the primary key from the underlying data source with a single delete command.
        /// </summary>
        /// <param name="table">Represents a table for a particular type in the underlying database containing rows are to be deleted.</param>
        /// <returns>The number of rows deleted from the database.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static int Delete(this System.Data.Linq.Table<HolidayShow.Data.DevicePatternSequences> table, int devicePatternSeqenceId)
        {
            return table.Delete(d => d.DevicePatternSeqenceId == devicePatternSeqenceId);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.DevicePatternSeqenceId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="devicePatternSeqenceId">DevicePatternSeqenceId to search for.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByDevicePatternSeqenceId(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, int devicePatternSeqenceId)
        {
            return queryable.Where(d => d.DevicePatternSeqenceId == devicePatternSeqenceId);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.DevicePatternSeqenceId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="devicePatternSeqenceId">DevicePatternSeqenceId to search for. This is on the right side of the operator.</param>
        /// <param name="comparisonOperator">The comparison operator.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByDevicePatternSeqenceId(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, ComparisonOperator comparisonOperator, int devicePatternSeqenceId)
        {
            switch (comparisonOperator)
            {
                case ComparisonOperator.GreaterThan:
                    return queryable.Where(d => d.DevicePatternSeqenceId > devicePatternSeqenceId);
                case ComparisonOperator.GreaterThanOrEquals:
                    return queryable.Where(d => d.DevicePatternSeqenceId >= devicePatternSeqenceId);
                case ComparisonOperator.LessThan:
                    return queryable.Where(d => d.DevicePatternSeqenceId < devicePatternSeqenceId);
                case ComparisonOperator.LessThanOrEquals:
                    return queryable.Where(d => d.DevicePatternSeqenceId <= devicePatternSeqenceId);
                case ComparisonOperator.NotEquals:
                    return queryable.Where(d => d.DevicePatternSeqenceId != devicePatternSeqenceId);
                default:
                    return queryable.Where(d => d.DevicePatternSeqenceId == devicePatternSeqenceId);
            }
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.DevicePatternSeqenceId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="devicePatternSeqenceId">DevicePatternSeqenceId to search for.</param>
        /// <param name="additionalValues">Additional values to search for.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByDevicePatternSeqenceId(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, int devicePatternSeqenceId, params int[] additionalValues)
        {
            var devicePatternSeqenceIdList = new List<int> { devicePatternSeqenceId };

            if (additionalValues != null)
                devicePatternSeqenceIdList.AddRange(additionalValues);

            if (devicePatternSeqenceIdList.Count == 1)
                return queryable.ByDevicePatternSeqenceId(devicePatternSeqenceIdList[0]);

            return queryable.ByDevicePatternSeqenceId(devicePatternSeqenceIdList);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.DevicePatternSeqenceId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="values">The values to search for..</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByDevicePatternSeqenceId(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, IEnumerable<int> values)
        {
            return queryable.Where(d => values.Contains(d.DevicePatternSeqenceId));
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.DevicePatternId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="devicePatternId">DevicePatternId to search for.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByDevicePatternId(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, int devicePatternId)
        {
            return queryable.Where(d => d.DevicePatternId == devicePatternId);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.DevicePatternId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="devicePatternId">DevicePatternId to search for. This is on the right side of the operator.</param>
        /// <param name="comparisonOperator">The comparison operator.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByDevicePatternId(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, ComparisonOperator comparisonOperator, int devicePatternId)
        {
            switch (comparisonOperator)
            {
                case ComparisonOperator.GreaterThan:
                    return queryable.Where(d => d.DevicePatternId > devicePatternId);
                case ComparisonOperator.GreaterThanOrEquals:
                    return queryable.Where(d => d.DevicePatternId >= devicePatternId);
                case ComparisonOperator.LessThan:
                    return queryable.Where(d => d.DevicePatternId < devicePatternId);
                case ComparisonOperator.LessThanOrEquals:
                    return queryable.Where(d => d.DevicePatternId <= devicePatternId);
                case ComparisonOperator.NotEquals:
                    return queryable.Where(d => d.DevicePatternId != devicePatternId);
                default:
                    return queryable.Where(d => d.DevicePatternId == devicePatternId);
            }
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.DevicePatternId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="devicePatternId">DevicePatternId to search for.</param>
        /// <param name="additionalValues">Additional values to search for.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByDevicePatternId(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, int devicePatternId, params int[] additionalValues)
        {
            var devicePatternIdList = new List<int> { devicePatternId };

            if (additionalValues != null)
                devicePatternIdList.AddRange(additionalValues);

            if (devicePatternIdList.Count == 1)
                return queryable.ByDevicePatternId(devicePatternIdList[0]);

            return queryable.ByDevicePatternId(devicePatternIdList);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.DevicePatternId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="values">The values to search for..</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByDevicePatternId(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, IEnumerable<int> values)
        {
            return queryable.Where(d => values.Contains(d.DevicePatternId));
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.OnAt"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="onAt">OnAt to search for.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByOnAt(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, int onAt)
        {
            return queryable.Where(d => d.OnAt == onAt);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.OnAt"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="onAt">OnAt to search for. This is on the right side of the operator.</param>
        /// <param name="comparisonOperator">The comparison operator.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByOnAt(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, ComparisonOperator comparisonOperator, int onAt)
        {
            switch (comparisonOperator)
            {
                case ComparisonOperator.GreaterThan:
                    return queryable.Where(d => d.OnAt > onAt);
                case ComparisonOperator.GreaterThanOrEquals:
                    return queryable.Where(d => d.OnAt >= onAt);
                case ComparisonOperator.LessThan:
                    return queryable.Where(d => d.OnAt < onAt);
                case ComparisonOperator.LessThanOrEquals:
                    return queryable.Where(d => d.OnAt <= onAt);
                case ComparisonOperator.NotEquals:
                    return queryable.Where(d => d.OnAt != onAt);
                default:
                    return queryable.Where(d => d.OnAt == onAt);
            }
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.OnAt"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="onAt">OnAt to search for.</param>
        /// <param name="additionalValues">Additional values to search for.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByOnAt(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, int onAt, params int[] additionalValues)
        {
            var onAtList = new List<int> { onAt };

            if (additionalValues != null)
                onAtList.AddRange(additionalValues);

            if (onAtList.Count == 1)
                return queryable.ByOnAt(onAtList[0]);

            return queryable.ByOnAt(onAtList);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.OnAt"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="values">The values to search for..</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByOnAt(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, IEnumerable<int> values)
        {
            return queryable.Where(d => values.Contains(d.OnAt));
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.Duration"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="duration">Duration to search for.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByDuration(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, int duration)
        {
            return queryable.Where(d => d.Duration == duration);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.Duration"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="duration">Duration to search for. This is on the right side of the operator.</param>
        /// <param name="comparisonOperator">The comparison operator.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByDuration(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, ComparisonOperator comparisonOperator, int duration)
        {
            switch (comparisonOperator)
            {
                case ComparisonOperator.GreaterThan:
                    return queryable.Where(d => d.Duration > duration);
                case ComparisonOperator.GreaterThanOrEquals:
                    return queryable.Where(d => d.Duration >= duration);
                case ComparisonOperator.LessThan:
                    return queryable.Where(d => d.Duration < duration);
                case ComparisonOperator.LessThanOrEquals:
                    return queryable.Where(d => d.Duration <= duration);
                case ComparisonOperator.NotEquals:
                    return queryable.Where(d => d.Duration != duration);
                default:
                    return queryable.Where(d => d.Duration == duration);
            }
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.Duration"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="duration">Duration to search for.</param>
        /// <param name="additionalValues">Additional values to search for.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByDuration(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, int duration, params int[] additionalValues)
        {
            var durationList = new List<int> { duration };

            if (additionalValues != null)
                durationList.AddRange(additionalValues);

            if (durationList.Count == 1)
                return queryable.ByDuration(durationList[0]);

            return queryable.ByDuration(durationList);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.Duration"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="values">The values to search for..</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByDuration(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, IEnumerable<int> values)
        {
            return queryable.Where(d => values.Contains(d.Duration));
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.AudioId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="audioId">AudioId to search for.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByAudioId(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, int audioId)
        {
            return queryable.Where(d => d.AudioId == audioId);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.AudioId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="audioId">AudioId to search for. This is on the right side of the operator.</param>
        /// <param name="comparisonOperator">The comparison operator.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByAudioId(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, ComparisonOperator comparisonOperator, int audioId)
        {
            switch (comparisonOperator)
            {
                case ComparisonOperator.GreaterThan:
                    return queryable.Where(d => d.AudioId > audioId);
                case ComparisonOperator.GreaterThanOrEquals:
                    return queryable.Where(d => d.AudioId >= audioId);
                case ComparisonOperator.LessThan:
                    return queryable.Where(d => d.AudioId < audioId);
                case ComparisonOperator.LessThanOrEquals:
                    return queryable.Where(d => d.AudioId <= audioId);
                case ComparisonOperator.NotEquals:
                    return queryable.Where(d => d.AudioId != audioId);
                default:
                    return queryable.Where(d => d.AudioId == audioId);
            }
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.AudioId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="audioId">AudioId to search for.</param>
        /// <param name="additionalValues">Additional values to search for.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByAudioId(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, int audioId, params int[] additionalValues)
        {
            var audioIdList = new List<int> { audioId };

            if (additionalValues != null)
                audioIdList.AddRange(additionalValues);

            if (audioIdList.Count == 1)
                return queryable.ByAudioId(audioIdList[0]);

            return queryable.ByAudioId(audioIdList);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.AudioId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="values">The values to search for..</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByAudioId(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, IEnumerable<int> values)
        {
            return queryable.Where(d => values.Contains(d.AudioId));
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.DeviceIoPortId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="deviceIoPortId">DeviceIoPortId to search for.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByDeviceIoPortId(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, int deviceIoPortId)
        {
            return queryable.Where(d => d.DeviceIoPortId == deviceIoPortId);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.DeviceIoPortId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="deviceIoPortId">DeviceIoPortId to search for. This is on the right side of the operator.</param>
        /// <param name="comparisonOperator">The comparison operator.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByDeviceIoPortId(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, ComparisonOperator comparisonOperator, int deviceIoPortId)
        {
            switch (comparisonOperator)
            {
                case ComparisonOperator.GreaterThan:
                    return queryable.Where(d => d.DeviceIoPortId > deviceIoPortId);
                case ComparisonOperator.GreaterThanOrEquals:
                    return queryable.Where(d => d.DeviceIoPortId >= deviceIoPortId);
                case ComparisonOperator.LessThan:
                    return queryable.Where(d => d.DeviceIoPortId < deviceIoPortId);
                case ComparisonOperator.LessThanOrEquals:
                    return queryable.Where(d => d.DeviceIoPortId <= deviceIoPortId);
                case ComparisonOperator.NotEquals:
                    return queryable.Where(d => d.DeviceIoPortId != deviceIoPortId);
                default:
                    return queryable.Where(d => d.DeviceIoPortId == deviceIoPortId);
            }
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.DeviceIoPortId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="deviceIoPortId">DeviceIoPortId to search for.</param>
        /// <param name="additionalValues">Additional values to search for.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByDeviceIoPortId(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, int deviceIoPortId, params int[] additionalValues)
        {
            var deviceIoPortIdList = new List<int> { deviceIoPortId };

            if (additionalValues != null)
                deviceIoPortIdList.AddRange(additionalValues);

            if (deviceIoPortIdList.Count == 1)
                return queryable.ByDeviceIoPortId(deviceIoPortIdList[0]);

            return queryable.ByDeviceIoPortId(deviceIoPortIdList);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.DevicePatternSequences.DeviceIoPortId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="values">The values to search for..</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.DevicePatternSequences> ByDeviceIoPortId(this IQueryable<HolidayShow.Data.DevicePatternSequences> queryable, IEnumerable<int> values)
        {
            return queryable.Where(d => values.Contains(d.DeviceIoPortId));
        }

        #region Query
        /// <summary>
        /// A private class for lazy loading static compiled queries.
        /// </summary>
        private static partial class Query
        {

            [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
            internal static readonly Func<HolidayShow.Data.HolidayShowDataContext, int, HolidayShow.Data.DevicePatternSequences> GetByKey =
                System.Data.Linq.CompiledQuery.Compile(
                    (HolidayShow.Data.HolidayShowDataContext db, int devicePatternSeqenceId) =>
                        db.DevicePatternSequences.FirstOrDefault(d => d.DevicePatternSeqenceId == devicePatternSeqenceId));

        }
        #endregion
    }
}
#pragma warning restore 1591
