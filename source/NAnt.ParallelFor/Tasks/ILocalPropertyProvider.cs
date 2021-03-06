﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILocalPropertyProvider.cs" company="NAnt.CrossCompile team">
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
//   <historyitem date="2016-03-13" change="Created file"/>
//   <historyitem date="2016-03-19" change="Renamed from parallel to parallelfor"/>
// </history>
// --------------------------------------------------------------------------------------------------------------------
namespace NAnt.ParallelFor.Tasks
{
  /// <summary>
  /// Interface for providing local properties.
  /// </summary>
  public interface ILocalPropertyProvider
  {
    /// <summary>
    /// Adds the property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="propertyValue">The property value.</param>
    void AddProperty(string propertyName, string propertyValue);
  }
}
