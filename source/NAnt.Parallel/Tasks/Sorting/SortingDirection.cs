// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SortingDirection.cs" company="NAnt.CrossCompile team">
//
// NAnt.Parallel, an extension for NAnt for parallel task execution.
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
//   <historyitem date="2016-03-11" change="Created file based on SortingDirection.cs of NAnt.Crosscompile 0.7.4.1"/>
//   <historyitem date="2016-03-12" change="Fixed file header"/>
// </history>
// --------------------------------------------------------------------------------------------------------------------
namespace NAnt.Parallel.Tasks.Sorting
{
  /// <summary>
  /// Sorting direction for a tasks input.
  /// </summary>
  public enum SortingDirection
  {
    /// <summary>
    /// Sort source files ascending.
    /// </summary>
    Ascending,

    /// <summary>
    /// Sort source files descending.
    /// </summary>
    Descending,
  }
}
