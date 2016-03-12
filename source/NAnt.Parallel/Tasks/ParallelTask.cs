// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParallelTask.cs" company="NAnt.CrossCompile team">
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
//   <historyitem date="2016-03-11" change="Created file based on BaseProcessorTask.cs of NAnt.Crosscompile 0.7.4.1"/>
//   <historyitem date="2016-03-12" change="Fixed file header"/>
// </history>
// --------------------------------------------------------------------------------------------------------------------
namespace NAnt.Parallel.Tasks
{
  using System;
  using System.IO;
  using Core;
  using Core.Attributes;
  using Core.Tasks;
  using Types;

  /// <summary>
  /// A NAnt task which allows parallel execution.
  /// </summary>
  /// <seealso cref="Task" />
  [TaskName("parallel")]
  public class ParallelTask : Task
  {
    /// <summary>
    /// Gets or sets the property.
    /// </summary>
    /// <value>
    /// The property.
    /// </value>
    [TaskAttribute("property", Required = true)]
    [StringValidator(AllowEmpty = false)]
    public string Property { get; set; }

    /// <summary>
    /// Gets the input files for the task by passing one or more NAnt <see cref="Core.Types.FileSet"/> items.
    /// The files specified by the <see cref="InputFilesCollection"/> element are always processed before the files specified by the 
    /// <see cref="InputFilesCollection"/> elements.
    /// </summary>
    [BuildElement("in")]
    public FileSetCollection InputFilesCollection { get; private set; } = new FileSetCollection();

    /// <summary>
    /// Gets or sets the type of iteration that should be done.
    /// </summary>
    [TaskAttribute("item", Required = true)]
    public LoopTask.LoopItem ItemType { get; set; }

    /// <summary>
    /// Gets or sets the task container.
    /// </summary>
    /// <value>
    /// The task container.
    /// </value>
    [BuildElement("do")]
    public ParallelTaskContainer TaskContainer { get; set; }

    /// <summary>
    /// Gets or sets the source of the iteration.
    /// </summary>
    [TaskAttribute("in", Required = false)]
    public string Source { get; set; }

    /// <summary>
    /// Gets or sets the delimiter char.
    /// </summary>
    [TaskAttribute("delim")]
    public string Delimiter { get; set; }

    /// <summary>
    /// Executes the task.
    /// </summary>
    protected override void ExecuteTask()
    {
      switch (this.ItemType)
      {
        case LoopTask.LoopItem.File:
          this.TaskContainer.AddItems(
            this.InputFilesCollection.GetAllFilesUsingRelativePaths(
              new Uri(Path.Combine(this.Project.BaseDirectory, "."))));
          break;
        case LoopTask.LoopItem.Folder:
          this.TaskContainer.AddItems(
            this.InputFilesCollection.GetAllDirectoriesUsingRelativePaths(
              new Uri(Path.Combine(this.Project.BaseDirectory, "."))));
          break;
        case LoopTask.LoopItem.Line:
          this.TaskContainer.AddItems(File.ReadAllLines(this.Source));
          break;
        case LoopTask.LoopItem.String:
          this.TaskContainer.AddItems(this.Source.Split(new[] { this.Delimiter }, StringSplitOptions.None));
          break;
      }

      this.TaskContainer.Execute();
    }
  }
}
