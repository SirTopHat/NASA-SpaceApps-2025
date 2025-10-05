using System;
using UnityEngine;

namespace NasaSpaceApps.FarmFromSpace.Game.Utils
{
	/// <summary>
	/// Utility class for handling date-based week calculations
	/// Maps game turns to actual calendar weeks for NASA data integration
	/// </summary>
	public static class DateUtils
	{
		/// <summary>
	/// Calculate the actual date for a given turn number
	/// </summary>
	/// <param name="startDate">The starting date of the game</param>
	/// <param name="turnNumber">The turn number (0-based)</param>
	/// <returns>The actual date for this turn</returns>
	public static DateTime GetDateForTurn(DateTime startDate, int turnNumber)
		{
			return startDate.AddDays(turnNumber * 7);
		}

		/// <summary>
	/// Get the week number within the month for a given date
	/// </summary>
	/// <param name="date">The date to calculate the week for</param>
	/// <returns>Week number (1-5, where 5 represents the last week of the month)</returns>
	public static int GetWeekInMonth(DateTime date)
		{
			// Calculate which week of the month this date falls in
			int dayOfMonth = date.Day;
			int weekInMonth = ((dayOfMonth - 1) / 7) + 1;
			
			// Cap at 5 weeks (some months have 5 weeks)
			return Mathf.Min(weekInMonth, 5);
		}

		/// <summary>
	/// Get a formatted string representing the date for a turn
	/// </summary>
	/// <param name="startDate">The starting date of the game</param>
	/// <param name="turnNumber">The turn number (0-based)</param>
	/// <returns>Formatted string like "March, Week 1" or "April, Week 2"</returns>
	public static string GetFormattedWeek(DateTime startDate, int turnNumber)
		{
			DateTime date = GetDateForTurn(startDate, turnNumber);
			string monthName = date.ToString("MMMM");
			int weekInMonth = GetWeekInMonth(date);
			
			return $"{monthName}, Week {weekInMonth}";
		}

		/// <summary>
	/// Get a detailed formatted string with the full date
	/// </summary>
	/// <param name="startDate">The starting date of the game</param>
	/// <param name="turnNumber">The turn number (0-based)</param>
	/// <returns>Formatted string like "March 1-7, 2024"</returns>
	public static string GetFormattedDateRange(DateTime startDate, int turnNumber)
		{
			DateTime weekStart = GetDateForTurn(startDate, turnNumber);
			DateTime weekEnd = weekStart.AddDays(6);
			
			if (weekStart.Month == weekEnd.Month)
			{
				return $"{weekStart:MMMM} {weekStart.Day}-{weekEnd.Day}, {weekStart.Year}";
			}
			else
			{
				return $"{weekStart:MMM} {weekStart.Day} - {weekEnd:MMM} {weekEnd.Day}, {weekStart.Year}";
			}
		}

		/// <summary>
	/// Get the season for a given date
	/// </summary>
	/// <param name="date">The date to get the season for</param>
	/// <returns>Season name</returns>
	public static string GetSeason(DateTime date)
		{
			int month = date.Month;
			
			if (month >= 3 && month <= 5) return "Spring";
			if (month >= 6 && month <= 8) return "Summer";
			if (month >= 9 && month <= 11) return "Autumn";
			return "Winter";
		}

		/// <summary>
	/// Get the day of year for a given date
	/// </summary>
	/// <param name="date">The date to get the day of year for</param>
	/// <returns>Day of year (1-366)</returns>
	public static int GetDayOfYear(DateTime date)
		{
			return date.DayOfYear;
		}

		/// <summary>
	/// Check if a date falls within a specific month
	/// </summary>
	/// <param name="date">The date to check</param>
	/// <param name="month">The month to check against (1-12)</param>
	/// <returns>True if the date is in the specified month</returns>
	public static bool IsInMonth(DateTime date, int month)
		{
			return date.Month == month;
		}

		/// <summary>
	/// Get the number of days in a month
	/// </summary>
	/// <param name="year">The year</param>
	/// <param name="month">The month (1-12)</param>
	/// <returns>Number of days in the month</returns>
	public static int GetDaysInMonth(int year, int month)
		{
			return DateTime.DaysInMonth(year, month);
		}

		/// <summary>
	/// Get a short formatted string for display in UI
	/// </summary>
	/// <param name="startDate">The starting date of the game</param>
	/// <param name="turnNumber">The turn number (0-based)</param>
	/// <returns>Short string like "Mar W1" or "Apr W2"</returns>
	public static string GetShortWeek(DateTime startDate, int turnNumber)
		{
			DateTime date = GetDateForTurn(startDate, turnNumber);
			string monthAbbr = date.ToString("MMM");
			int weekInMonth = GetWeekInMonth(date);
			
			return $"{monthAbbr} W{weekInMonth}";
		}
	}
}
