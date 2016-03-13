// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocalPropertyTask.cs" company="NAnt.CrossCompile team">
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
// </history>
// --------------------------------------------------------------------------------------------------------------------
namespace NAnt.Parallel.Tasks
{
  using Core;
  using Core.Attributes;

  /// <summary>
  /// Task which handles local properties.
  /// </summary>
  /// <seealso cref="Task" />
  [TaskName("localproperty")]
  public class LocalPropertyTask : Task
  {
    /// <summary>
    /// Gets or sets the name of the property.
    /// </summary>
    /// <value>
    /// The name of the property.
    /// </value>
    [TaskAttribute("name", Required = true)]
    [StringValidator(AllowEmpty = false)]
    public string PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the property value.
    /// </summary>
    /// <value>
    /// The property value.
    /// </value>
    [TaskAttribute("value", Required = true)]
    public string PropertyValue { get; set; }

    /// <summary>
    /// Executes the task.
    /// </summary>
    protected override void ExecuteTask()
    {
      Element parent = this.Parent as Element;
      while ((null != parent) && ((parent is ILocalPropertyProvider) == false))
      {
        parent = parent.Parent as Element;
      }

      ILocalPropertyProvider propertyProvider = parent as ILocalPropertyProvider;
      if (propertyProvider == null)
      {
        throw new BuildException(this.Name + " must have a ILocalPropertyProvider parent");
      }
      else
      {
        propertyProvider.AddProperty(this.PropertyName, this.PropertyValue);
      }
    }
  }
}
