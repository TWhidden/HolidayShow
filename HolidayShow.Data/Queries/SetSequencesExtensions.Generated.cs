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
    /// The query extension class for SetSequences.
    /// </summary>
    public static partial class SetSequencesExtensions
    {

        /// <summary>
        /// Gets an instance by the primary key.
        /// </summary>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static HolidayShow.Data.SetSequences GetByKey(this IQueryable<HolidayShow.Data.SetSequences> queryable, int setSequenceId)
        {
            var entity = queryable as System.Data.Linq.Table<HolidayShow.Data.SetSequences>;
            if (entity != null && entity.Context.LoadOptions == null)
                return Query.GetByKey.Invoke((HolidayShow.Data.HolidayShowDataContext)entity.Context, setSequenceId);

            return queryable.FirstOrDefault(s => s.SetSequenceId == setSequenceId);
        }

        /// <summary>
        /// Immediately deletes the entity by the primary key from the underlying data source with a single delete command.
        /// </summary>
        /// <param name="table">Represents a table for a particular type in the underlying database containing rows are to be deleted.</param>
        /// <returns>The number of rows deleted from the database.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static int Delete(this System.Data.Linq.Table<HolidayShow.Data.SetSequences> table, int setSequenceId)
        {
            return table.Delete(s => s.SetSequenceId == setSequenceId);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.SetSequences.SetId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="setId">SetId to search for.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.SetSequences> BySetId(this IQueryable<HolidayShow.Data.SetSequences> queryable, int setId)
        {
            return queryable.Where(s => s.SetId == setId);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.SetSequences.SetId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="setId">SetId to search for. This is on the right side of the operator.</param>
        /// <param name="comparisonOperator">The comparison operator.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.SetSequences> BySetId(this IQueryable<HolidayShow.Data.SetSequences> queryable, ComparisonOperator comparisonOperator, int setId)
        {
            switch (comparisonOperator)
            {
                case ComparisonOperator.GreaterThan:
                    return queryable.Where(s => s.SetId > setId);
                case ComparisonOperator.GreaterThanOrEquals:
                    return queryable.Where(s => s.SetId >= setId);
                case ComparisonOperator.LessThan:
                    return queryable.Where(s => s.SetId < setId);
                case ComparisonOperator.LessThanOrEquals:
                    return queryable.Where(s => s.SetId <= setId);
                case ComparisonOperator.NotEquals:
                    return queryable.Where(s => s.SetId != setId);
                default:
                    return queryable.Where(s => s.SetId == setId);
            }
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.SetSequences.SetId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="setId">SetId to search for.</param>
        /// <param name="additionalValues">Additional values to search for.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.SetSequences> BySetId(this IQueryable<HolidayShow.Data.SetSequences> queryable, int setId, params int[] additionalValues)
        {
            var setIdList = new List<int> { setId };

            if (additionalValues != null)
                setIdList.AddRange(additionalValues);

            if (setIdList.Count == 1)
                return queryable.BySetId(setIdList[0]);

            return queryable.BySetId(setIdList);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.SetSequences.SetId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="values">The values to search for..</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.SetSequences> BySetId(this IQueryable<HolidayShow.Data.SetSequences> queryable, IEnumerable<int> values)
        {
            return queryable.Where(s => values.Contains(s.SetId));
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.SetSequences.OnAt"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="onAt">OnAt to search for.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.SetSequences> ByOnAt(this IQueryable<HolidayShow.Data.SetSequences> queryable, int onAt)
        {
            return queryable.Where(s => s.OnAt == onAt);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.SetSequences.OnAt"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="onAt">OnAt to search for. This is on the right side of the operator.</param>
        /// <param name="comparisonOperator">The comparison operator.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.SetSequences> ByOnAt(this IQueryable<HolidayShow.Data.SetSequences> queryable, ComparisonOperator comparisonOperator, int onAt)
        {
            switch (comparisonOperator)
            {
                case ComparisonOperator.GreaterThan:
                    return queryable.Where(s => s.OnAt > onAt);
                case ComparisonOperator.GreaterThanOrEquals:
                    return queryable.Where(s => s.OnAt >= onAt);
                case ComparisonOperator.LessThan:
                    return queryable.Where(s => s.OnAt < onAt);
                case ComparisonOperator.LessThanOrEquals:
                    return queryable.Where(s => s.OnAt <= onAt);
                case ComparisonOperator.NotEquals:
                    return queryable.Where(s => s.OnAt != onAt);
                default:
                    return queryable.Where(s => s.OnAt == onAt);
            }
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.SetSequences.OnAt"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="onAt">OnAt to search for.</param>
        /// <param name="additionalValues">Additional values to search for.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.SetSequences> ByOnAt(this IQueryable<HolidayShow.Data.SetSequences> queryable, int onAt, params int[] additionalValues)
        {
            var onAtList = new List<int> { onAt };

            if (additionalValues != null)
                onAtList.AddRange(additionalValues);

            if (onAtList.Count == 1)
                return queryable.ByOnAt(onAtList[0]);

            return queryable.ByOnAt(onAtList);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.SetSequences.OnAt"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="values">The values to search for..</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.SetSequences> ByOnAt(this IQueryable<HolidayShow.Data.SetSequences> queryable, IEnumerable<int> values)
        {
            return queryable.Where(s => values.Contains(s.OnAt));
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.SetSequences.DevicePatternId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="devicePatternId">DevicePatternId to search for.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.SetSequences> ByDevicePatternId(this IQueryable<HolidayShow.Data.SetSequences> queryable, int devicePatternId)
        {
            return queryable.Where(s => s.DevicePatternId == devicePatternId);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.SetSequences.DevicePatternId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="devicePatternId">DevicePatternId to search for. This is on the right side of the operator.</param>
        /// <param name="comparisonOperator">The comparison operator.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.SetSequences> ByDevicePatternId(this IQueryable<HolidayShow.Data.SetSequences> queryable, ComparisonOperator comparisonOperator, int devicePatternId)
        {
            switch (comparisonOperator)
            {
                case ComparisonOperator.GreaterThan:
                    return queryable.Where(s => s.DevicePatternId > devicePatternId);
                case ComparisonOperator.GreaterThanOrEquals:
                    return queryable.Where(s => s.DevicePatternId >= devicePatternId);
                case ComparisonOperator.LessThan:
                    return queryable.Where(s => s.DevicePatternId < devicePatternId);
                case ComparisonOperator.LessThanOrEquals:
                    return queryable.Where(s => s.DevicePatternId <= devicePatternId);
                case ComparisonOperator.NotEquals:
                    return queryable.Where(s => s.DevicePatternId != devicePatternId);
                default:
                    return queryable.Where(s => s.DevicePatternId == devicePatternId);
            }
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.SetSequences.DevicePatternId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="devicePatternId">DevicePatternId to search for.</param>
        /// <param name="additionalValues">Additional values to search for.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.SetSequences> ByDevicePatternId(this IQueryable<HolidayShow.Data.SetSequences> queryable, int devicePatternId, params int[] additionalValues)
        {
            var devicePatternIdList = new List<int> { devicePatternId };

            if (additionalValues != null)
                devicePatternIdList.AddRange(additionalValues);

            if (devicePatternIdList.Count == 1)
                return queryable.ByDevicePatternId(devicePatternIdList[0]);

            return queryable.ByDevicePatternId(devicePatternIdList);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.SetSequences.DevicePatternId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="values">The values to search for..</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.SetSequences> ByDevicePatternId(this IQueryable<HolidayShow.Data.SetSequences> queryable, IEnumerable<int> values)
        {
            return queryable.Where(s => values.Contains(s.DevicePatternId));
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.SetSequences.SetSequenceId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="setSequenceId">SetSequenceId to search for.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.SetSequences> BySetSequenceId(this IQueryable<HolidayShow.Data.SetSequences> queryable, int setSequenceId)
        {
            return queryable.Where(s => s.SetSequenceId == setSequenceId);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.SetSequences.SetSequenceId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="setSequenceId">SetSequenceId to search for. This is on the right side of the operator.</param>
        /// <param name="comparisonOperator">The comparison operator.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.SetSequences> BySetSequenceId(this IQueryable<HolidayShow.Data.SetSequences> queryable, ComparisonOperator comparisonOperator, int setSequenceId)
        {
            switch (comparisonOperator)
            {
                case ComparisonOperator.GreaterThan:
                    return queryable.Where(s => s.SetSequenceId > setSequenceId);
                case ComparisonOperator.GreaterThanOrEquals:
                    return queryable.Where(s => s.SetSequenceId >= setSequenceId);
                case ComparisonOperator.LessThan:
                    return queryable.Where(s => s.SetSequenceId < setSequenceId);
                case ComparisonOperator.LessThanOrEquals:
                    return queryable.Where(s => s.SetSequenceId <= setSequenceId);
                case ComparisonOperator.NotEquals:
                    return queryable.Where(s => s.SetSequenceId != setSequenceId);
                default:
                    return queryable.Where(s => s.SetSequenceId == setSequenceId);
            }
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.SetSequences.SetSequenceId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="setSequenceId">SetSequenceId to search for.</param>
        /// <param name="additionalValues">Additional values to search for.</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.SetSequences> BySetSequenceId(this IQueryable<HolidayShow.Data.SetSequences> queryable, int setSequenceId, params int[] additionalValues)
        {
            var setSequenceIdList = new List<int> { setSequenceId };

            if (additionalValues != null)
                setSequenceIdList.AddRange(additionalValues);

            if (setSequenceIdList.Count == 1)
                return queryable.BySetSequenceId(setSequenceIdList[0]);

            return queryable.BySetSequenceId(setSequenceIdList);
        }

        /// <summary>
        /// Gets a query for <see cref="HolidayShow.Data.SetSequences.SetSequenceId"/>.
        /// </summary>
        /// <param name="queryable">Query to append where clause.</param>
        /// <param name="values">The values to search for..</param>
        /// <returns><see cref="IQueryable"/> with additional where clause.</returns>
        [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
        public static IQueryable<HolidayShow.Data.SetSequences> BySetSequenceId(this IQueryable<HolidayShow.Data.SetSequences> queryable, IEnumerable<int> values)
        {
            return queryable.Where(s => values.Contains(s.SetSequenceId));
        }

        #region Query
        /// <summary>
        /// A private class for lazy loading static compiled queries.
        /// </summary>
        private static partial class Query
        {

            [System.CodeDom.Compiler.GeneratedCode("CodeSmith", "6.0.0.0")]
            internal static readonly Func<HolidayShow.Data.HolidayShowDataContext, int, HolidayShow.Data.SetSequences> GetByKey =
                System.Data.Linq.CompiledQuery.Compile(
                    (HolidayShow.Data.HolidayShowDataContext db, int setSequenceId) =>
                        db.SetSequences.FirstOrDefault(s => s.SetSequenceId == setSequenceId));

        }
        #endregion
    }
}
#pragma warning restore 1591
