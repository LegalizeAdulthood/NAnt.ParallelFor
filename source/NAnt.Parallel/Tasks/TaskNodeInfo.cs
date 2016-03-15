// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TaskNodeInfo.cs" company="NAnt.CrossCompile team">
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
//   <historyitem date="2016-03-14" change="Created file"/>
// </history>
// --------------------------------------------------------------------------------------------------------------------
namespace NAnt.Parallel.Tasks
{
  using System.Xml;
  using Core;

  /// <summary>
  /// Class to collect information about the current task and its XML node.
  /// </summary>
  internal class TaskNodeInfo
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TaskNodeInfo"/> class.
    /// </summary>
    /// <param name="task">The next executed task.</param>
    /// <param name="xmlNode">The XML node of the <paramref name="task"/>.</param>
    public TaskNodeInfo(Task task, XmlNode xmlNode) : this(task, xmlNode, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskNodeInfo"/> class.
    /// </summary>
    /// <param name="task">The next executed task.</param>
    /// <param name="xmlNode">The XML node of the <paramref name="task"/>.</param>
    /// <param name="previous">The instance of the previously executed task.</param>
    public TaskNodeInfo(Task task, XmlNode xmlNode, TaskNodeInfo previous)
    {
      this.XmlNode = xmlNode;
      this.Task = task;
      this.Previous = previous;
    }

    /// <summary>
    /// Gets or sets the current task.
    /// </summary>
    public Task Task { get; set; }

    /// <summary>
    /// Gets or sets the current XML node.
    /// </summary>
    public XmlNode XmlNode { get; set; }

    /// <summary>
    /// Gets or sets the previously used instance.
    /// </summary>
    public TaskNodeInfo Previous { get; set; }
  }
}
