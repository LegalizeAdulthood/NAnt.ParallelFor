// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Logger.cs" company="NAnt.CrossCompile team">
//
// NAnt.Make, an extension for NAnt which performs file processing similar to make.
// Copyright (C) 2016 NAnt.CrossCompile team.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
// </copyright>
// <history>
//   <historyitem date="2016-03-11" change="Created file based on Logger.cs of NAnt.Crosscompile 0.7.4.1"/>
// </history>
// --------------------------------------------------------------------------------------------------------------------
namespace NAnt.Parallel.Helpers
{
  using System;
  using System.Globalization;
  using Core;

  /// <summary>
  /// Logging abstraction for NAnt.CrossCompile.
  /// </summary>
  public static class Logger
  {
    /// <summary>
    /// Lock for the last caught exception which is stored in <see cref="lastException"/>.
    /// </summary>
    private static readonly object LastExceptionLock = new object();

    /// <summary>
    /// The currently executed task.
    /// </summary>
    private static Task currentTask;

    /// <summary>
    /// The last caught exception.
    /// </summary>
    private static BuildException lastException;

    /// <summary>
    /// Indicates if build threads are currently active.
    /// </summary>
    private static bool buildThreadsActive;

    /// <summary>
    /// Sets the currently executed task.
    /// </summary>
    /// <param name="value">The currently executed task.</param>
    public static void SetCurrentTask(Task value)
    {
      currentTask = value;
      lock (LastExceptionLock)
      {
        lastException = null;
      }
    }

    /// <summary>
    /// Sets a value indicating if build threads are currently active.
    /// </summary>
    /// <param name="value">A value indicating if build threads are currently active.</param>
    public static void SetBuildThreadsActive(bool value)
    {
      buildThreadsActive = value;
    }

    /// <summary>
    /// Formats the debug message and writes it to the build log.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="arguments">The format string arguments.</param>
    public static void LogDebug(string format, params object[] arguments)
    {
      LogDebug(currentTask, format, arguments);
    }

    /// <summary>
    /// Formats the debug message and writes it to the build log.
    /// </summary>
    /// <param name="currentInstance">The current Task instance which is performing the log.</param>
    /// <param name="format">The format string.</param>
    /// <param name="arguments">The format string arguments.</param>
    public static void LogDebug(Element currentInstance, string format, params object[] arguments)
    {
      string logMessage = string.Format(CultureInfo.CurrentCulture, format, arguments);
      currentInstance.Log(Level.Debug, logMessage);
    }

    /// <summary>
    /// Formats the info message and writes it to the build log.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="arguments">The format string arguments.</param>
    public static void LogInfo(string format, params object[] arguments)
    {
      LogInfo(currentTask, format, arguments);
    }

    /// <summary>
    /// Formats the info message and writes it to the build log.
    /// </summary>
    /// <param name="currentInstance">The current Task instance which is performing the log.</param>
    /// <param name="format">The format string.</param>
    /// <param name="arguments">The format string arguments.</param>
    public static void LogInfo(Element currentInstance, string format, params object[] arguments)
    {
      string logMessage = string.Format(CultureInfo.CurrentCulture, format, arguments);
      currentInstance.Log(Level.Info, logMessage);
    }

    /// <summary>
    /// Formats the error message and writes it to the build log.
    /// If the build threads aren't executed at the moment, a <see cref="BuildException"/> will be thrown.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="arguments">The format string arguments.</param>
    public static void LogError(string format, params object[] arguments)
    {
      LogError(currentTask, null, format, arguments);
    }

    /// <summary>
    /// Formats the error message and writes it to the build log.
    /// If the build threads aren't executed at the moment, a <see cref="BuildException" /> will be thrown.
    /// </summary>
    /// <param name="innerException">The inner exception.</param>
    /// <param name="format">The format string.</param>
    /// <param name="arguments">The format string arguments.</param>
    public static void LogError(Exception innerException, string format, params object[] arguments)
    {
      LogError(currentTask, innerException, format, arguments);
    }

    /// <summary>
    /// Formats the error message and writes it to the build log.
    /// If the build threads aren't executed at the moment, a <see cref="BuildException" /> will be thrown.
    /// </summary>
    /// <param name="currentInstance">The current Task instance which is performing the log.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <param name="format">The format string.</param>
    /// <param name="arguments">The format string arguments.</param>
    public static void LogError(Element currentInstance, Exception innerException, string format, params object[] arguments)
    {
      string logMessage = string.Format(CultureInfo.CurrentCulture, format, arguments);
      currentInstance.Log(Level.Error, logMessage);
      lock (LastExceptionLock)
      {
        BuildException buildException = new BuildException(logMessage, innerException);
        if (buildThreadsActive == false)
        {
          throw buildException;
        }

        if (lastException == null)
        {
          lastException = buildException;
        }
      }
    }

    /// <summary>
    /// Returns a value indicating if an error has occurred during the execution.
    /// </summary>
    /// <returns>A value indicating if an error has occurred during the execution.</returns>
    public static bool HasErrorOccurred()
    {
      lock (LastExceptionLock)
      {
        if (lastException != null)
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Returns the last occurred exception.
    /// </summary>
    /// <returns>The last occurred exception.</returns>
    public static BuildException GetLastException()
    {
      lock (LastExceptionLock)
      {
        return lastException;
      }
    }
  }
}
