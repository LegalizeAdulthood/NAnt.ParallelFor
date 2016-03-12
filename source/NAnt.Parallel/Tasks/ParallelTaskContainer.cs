// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParallelTaskContainer.cs" company="NAnt.CrossCompile team">
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
//   <historyitem date="2016-03-11" change="Created file based on BaseProcessorTask.cs of NAnt.Crosscompile 0.7.4.1 
// and TaskContainer.cs of NAnt 0.9.2"/>
// </history>
// --------------------------------------------------------------------------------------------------------------------
namespace NAnt.Parallel.Tasks
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Threading;
  using System.Xml;
  using Core;
  using Core.Attributes;
  using Helpers;
  using Sorting;

  /// <summary>
  /// Task container which executes the tasks in parallel.
  /// </summary>
  /// <seealso cref="TaskContainer" />
  public class ParallelTaskContainer : TaskContainer
  {
    /// <summary>
    /// The initial list of items to process.
    /// </summary>
    private readonly List<string> itemsToProcessList = new List<string>(); 

    /// <summary>
    /// The output files queue.
    /// </summary>
    private readonly Queue<string> itemsToProcessQueue = new Queue<string>();

    /// <summary>
    /// The <see cref="ParallelTask"/> instance which is the parent of this instance.
    /// </summary>
    private ParallelTask parent;

    /// <summary>
    /// Gets or sets the maximum number of threads which are used for parallel execution.
    /// The default is the number of CPU cores. Increasing this value is only reasonable if more than one output file will be created.
    /// </summary>
    /// <value>
    /// The max number of threads.
    /// </value>
    [TaskAttribute("maxthreads")]
    public int MaxThreads { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// Gets or sets a value indicating how the task will sort the source files before execution.
    /// </summary>
    /// <value>
    /// The value indicating how the task will sort the source files before execution.
    /// </value>
    [TaskAttribute("orderby")]
    public SortingCriterion OrderBy { get; set; } = SortingCriterion.None;

    /// <summary>
    /// Gets or sets the sorting direction.
    /// </summary>
    /// <value>
    /// The sorting direction.
    /// </value>
    [TaskAttribute("orderdirection")]
    public SortingDirection SortingDirection { get; set; } = SortingDirection.Ascending;

    /// <summary>
    /// Gets or sets a value indicating whether the execution should be stopped on the first occurring error.
    /// The default value is <see langword="false"/>.
    /// </summary>
    /// <value>
    ///   <c>true</c> if [stop on first error]; otherwise, <c>false</c>.
    /// </value>
    [TaskAttribute("stoponfirsterror")]
    public bool StopOnFirstError { get; set; }

    /// <summary>
    /// Adds the items.
    /// </summary>
    /// <param name="items">The items.</param>
    public void AddItems(IEnumerable<string> items)
    {
      this.itemsToProcessList.AddRange(items);
    }

    /// <summary>
    /// Executes the task.
    /// </summary>    
    protected override void ExecuteTask()
    {
      this.parent = this.Parent as ParallelTask;

      if (this.parent == null)
      {
        throw new BuildException("MakeTaskContainer must have a make parent");
      }      

      SortSourceFiles(this.itemsToProcessList, this.OrderBy, this.SortingDirection);

      // Copy source files to queue
      foreach (string sourcePath in this.itemsToProcessList)
      {
        // ReSharper disable once InconsistentlySynchronizedField
        // Lock not required as build threads are not running at this moment
        this.itemsToProcessQueue.Enqueue(sourcePath);
      }

      Logger.SetBuildThreadsActive(true);
      List<Thread> buildThreads = new List<Thread>();
      for (int threadIndex = 0; threadIndex < this.MaxThreads; threadIndex++)
      {
        Thread t = new Thread(this.ExecutChildTasks);
        buildThreads.Add(t);
        t.Start();
      }

      // Wait until all threads are finished
      foreach (Thread t in buildThreads)
      {
        t.Join();
      }
    }

    /// <summary>
    /// Sorts the source files.
    /// </summary>
    /// <param name="sources">The source files.</param>
    /// <param name="criterion">The criterion.</param>
    /// <param name="direction">The direction.</param>
    private static void SortSourceFiles(List<string> sources, SortingCriterion criterion, SortingDirection direction)
    {
      switch (criterion)
      {
        case SortingCriterion.None:
          // Nothing to do, keep order
          break;
        case SortingCriterion.Name:
          sources.Sort((a, b) => string.Compare(Path.GetFileName(a), Path.GetFileName(b), StringComparison.OrdinalIgnoreCase));
          break;
        case SortingCriterion.Size:
          sources.Sort((a, b) => new FileInfo(a).Length.CompareTo(new FileInfo(b).Length));
          break;
      }

      if ((criterion != SortingCriterion.None) && (direction == SortingDirection.Descending))
      {
        sources.Reverse();
      }
    }

    /// <summary>
    /// Processes the file.
    /// </summary>
    private void ExecutChildTasks()
    {
      while (true)
      {
        string currentItem;
        lock (this.itemsToProcessQueue)
        {
          if (this.itemsToProcessQueue.Count > 0)
          {
            currentItem = this.itemsToProcessQueue.Dequeue();
          }
          else
          {
            break;
          }
        }

        foreach (XmlNode childNode in this.XmlNode)
        {
          // we only care about xmlnodes (elements) that are of the right namespace.
          // ignore any private xml elements (by def. this includes any property with a BuildElementAttribute (name).
          if (childNode.NodeType != XmlNodeType.Element ||
              this.IsPrivateXmlElement(childNode) ||
              (childNode.NamespaceURI.Equals(NamespaceManager.LookupNamespace("nant")) == false))
          {
            continue;
          }

          XmlNode clone = childNode.CloneNode(true);
          this.ReplaceAttributeValues(clone, currentItem);

          if (TypeFactory.TaskBuilders.Contains(clone.Name))
          {
            try
            {
              // create task instance
              Task task = CreateChildTask(clone);

              // for now, we should assume null tasks are because of 
              // incomplete metadata about the XML
              if (task != null)
              {
                task.Parent = this;

                // execute task
                task.Execute();
              }
            }
            catch (BuildException ex)
            {
              Logger.LogError(ex, ex.Message);
            }
          }
          else if (TypeFactory.DataTypeBuilders.Contains(clone.Name))
          {
            try
            {
              // we are an datatype declaration
              DataTypeBase dataType = CreateChildDataTypeBase(clone);

              this.Log(Level.Debug, "Adding a {0} reference with id '{1}'.", clone.Name, dataType.ID);
              if (!Project.DataTypeReferences.Contains(dataType.ID))
              {
                Project.DataTypeReferences.Add(dataType.ID, dataType);
              }
              else
              {
                Project.DataTypeReferences[dataType.ID] = dataType; // overwrite with the new reference.
              }
            }
            catch (BuildException ex)
            {
              Logger.LogError(ex, ex.Message);
            }
          }
          else
          {
            Logger.LogError(null, "Error during processing node {0}", childNode.Name, Project.GetLocation(childNode));
          }

          // If StopOnFirstError is set to true, check if an exception has already been caught by any thread.
          // If an exception has been caught, leave the thread.
          if (this.StopOnFirstError && Logger.HasErrorOccurred())
          {
            return;
          }
        }
      }
    }

    /// <summary>
    /// Replaces the attribute values.
    /// </summary>
    /// <param name="xmlNode">The cloned XML node.</param>
    /// <param name="newValue">The new value for replacement.</param>
    private void ReplaceAttributeValues(XmlNode xmlNode, string newValue)
    {
      XmlNodeList attributes = xmlNode.SelectNodes("//@*");
      if (attributes != null)
      {
        string oldValue = "@{" + this.parent.Property + "}";
        foreach (XmlAttribute attribute in attributes)
        {
          attribute.Value = attribute.Value.Replace(oldValue, newValue);
        }
      }
    }
  }
}
