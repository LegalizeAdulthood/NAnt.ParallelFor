// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SortingCriterion.cs" company="NAnt.CrossCompile team">
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
//   <historyitem date="2016-03-11" change="Created file based on SortingCriterium.cs of NAnt.Crosscompile 0.7.4.1"/>
// </history>
// --------------------------------------------------------------------------------------------------------------------
namespace NAnt.Parallel.Tasks.Sorting
{
  /// <summary>
  /// Determines how the task will sort the input before execution.
  /// </summary>
  public enum SortingCriterion
  {
    /// <summary>
    /// Source files will not be sorted.
    /// </summary>
    None,

    /// <summary>
    /// Source files will be sorted by file name.
    /// </summary>
    Name,

    /// <summary>
    /// Source files will be sorted by file size.
    /// </summary>
    Size
  }
}
