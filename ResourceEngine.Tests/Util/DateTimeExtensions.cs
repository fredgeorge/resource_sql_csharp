/*
 * Copyright (c) 2025 by Fred George
 * May be used freely except for training; license required for training.
 * @author Fred George  fredgeorge@acm.org
 */

using System;

namespace ResourceEngine.Tests.Util;

// Understands Convenience constructs for time, duration, and dates
internal static class DateTimeExtensions {
    internal static DateTime May(this int dayOfMonth) => new DateTime(2025, 5, dayOfMonth);
    
    internal static TimeSpan Days(this int dayCount) => new TimeSpan(dayCount, 0, 0, 0);
    
}