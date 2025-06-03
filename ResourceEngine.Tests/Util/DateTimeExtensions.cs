/*
 * Copyright (c) 2025 by Fred George
 * May be used freely except for training; license required for training.
 * @author Fred George  fredgeorge@acm.org
 */

using System;

namespace ResourceEngine.Tests.Util;

// Understands Convenience constructs for time, duration, and dates
internal static class DateTimeExtensions {
    internal static DateTime May(this int dayOfMonth) => new(2025, 5, dayOfMonth);
    
    internal static TimeSpan Days(this int dayCount) => new(dayCount, 0, 0, 0);
    internal static TimeSpan Hours(this int hourCount) => new(hourCount, 0, 0);
    
}