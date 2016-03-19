// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParallelTaskContainer.cs" company="NAnt.CrossCompile team">
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
//   <historyitem date="2016-03-11" change="Created file based on BaseProcessorTask.cs of NAnt.Crosscompile 0.7.4.1 
// and TaskContainer.cs of NAnt 0.9.2"/>
//   <historyitem date="2016-03-12" change="Fixed file header"/>
//   <historyitem date="2016-03-13" change="Fixed file header, added local property handling, fixed logging exception"/>
//   <historyitem date="2016-03-14" change="Simplified property handling"/>
//   <historyitem date="2016-03-14" change="Changed property handling and error handling, added stoplooponerror attribute"/>
//   <historyitem date="2016-03-17" change="Fixed issue #1"/>
//   <historyitem date="2016-03-18" change="Improved  thread safety"/>
// </history>
// --------------------------------------------------------------------------------------------------------------------
namespace NAnt.Parallel.Tasks
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Reflection;
  using System.Threading;
  using System.Xml;
  using Core;
  using Core.Attributes;
  using Sorting;

  /// <summary>
  /// Task container which executes the tasks in parallel.
  /// </summary>
  /// <seealso cref="TaskContainer" />
  [TaskName("do")]
  public class ParallelTaskContainer : TaskContainer, ILocalPropertyProvider
  {
    /// <summary>
    /// The local properties. 
    /// The key of the first dictionary is the current thread.
    /// The value of the first dictionary is a dictionary containing the local properties of the current thread.
    /// The key of the second dictionary is the properties name.
    /// The value of the second dictionary is the properties value.
    /// </summary>
    private readonly Dictionary<Thread, Dictionary<string, string>> localProperties = new Dictionary<Thread, Dictionary<string, string>>();

    /// <summary>
    /// Dictionary to store the reference to the currently executed task and its XML node in the build file.
    /// The key is the current thread, the value is an instance referencing the current task and its XML node.
    /// </summary>
    private readonly Dictionary<Thread, TaskNodeInfo> currentTaskAndNode = new Dictionary<Thread, TaskNodeInfo>();

    /// <summary>
    /// Field information which contains the location of an element. As the location is not writable normally, reflection is used to set its value.
    /// The root cause is that the <see cref="Project"/> instance doesn't known anything about the cloned XML nodes used in this task, so it cannot determine its location.
    /// Solution: Inject location manually during task execution.
    /// </summary>
    private readonly FieldInfo fieldInfo = typeof(Element).GetField("_location", BindingFlags.Instance | BindingFlags.NonPublic);

    /// <summary>
    /// Instance for locking concurrent access to NAnts type factory which seems not to be thread-safe.
    /// </summary>
    private readonly object typeFactoryLock = new object();

    /// <summary>
    /// The initial list of items to process.
    /// </summary>
    private readonly List<string> itemsToProcessList = new List<string>();

    /// <summary>
    /// The output files queue.
    /// </summary>
    private readonly Queue<string> itemsToProcessQueue = new Queue<string>();

    /// <summary>
    /// The task exceptions.
    /// </summary>
    private readonly List<BuildException> taskExceptions = new List<BuildException>();

    /// <summary>
    /// The <see cref="ParallelForTask"/> instance which is the parent of this instance.
    /// </summary>
    private ParallelForTask parent;

    /// <summary>
    /// Delegate for creating NAnt <see cref="Element"/> instances.
    /// </summary>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    /// <param name="node">The node.</param>
    /// <returns>The created instance.</returns>
    private delegate TElement CreateElement<out TElement>(XmlNode node);

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
    public bool StopOnFirstError { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether a &lt;do&gt; loop shall be exited after an occurred.
    /// </summary>
    [TaskAttribute("stoplooponerror")]
    public bool StopLoopOnError { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether [error detected].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [error detected]; otherwise, <c>false</c>.
    /// </value>
    private bool ErrorDetected
    {
      get
      {
        lock (this.taskExceptions)
        {
          return this.taskExceptions.Count > 0;
        }
      }
    }

    /// <summary>
    /// Gets the next item to process.
    /// </summary>
    /// <value>
    /// The next item to process.
    /// </value>
    private string NextItemToProcess
    {
      get
      {
        lock (this.itemsToProcessQueue)
        {
          if (this.itemsToProcessQueue.Count > 0)
          {
            return this.itemsToProcessQueue.Dequeue();
          }

          return null;
        }
      }
    }

    /// <summary>
    /// Adds the items.
    /// </summary>
    /// <param name="items">The items.</param>
    public void AddItems(IEnumerable<string> items)
    {
      this.itemsToProcessList.AddRange(items);
    }

    /// <summary>
    /// Adds the property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="propertyValue">The property value.</param>
    public void AddProperty(string propertyName, string propertyValue)
    {
      lock (this.localProperties)
      {
        this.localProperties[Thread.CurrentThread].Add(propertyName, propertyValue);
      }
    }

    /// <summary>
    /// Executes the task.
    /// </summary>    
    protected override void ExecuteTask()
    {
      this.parent = this.Parent as ParallelForTask;

      if (this.parent == null)
      {
        throw new BuildException("MakeTaskContainer must have a <parallel> parent");
      }

      SortSourceFiles(this.itemsToProcessList, this.OrderBy, this.SortingDirection);

      // Copy source files to queue
      foreach (string sourcePath in this.itemsToProcessList)
      {
        // ReSharper disable once InconsistentlySynchronizedField
        // Lock not required as build threads are not running at this moment
        this.itemsToProcessQueue.Enqueue(sourcePath);
      }

      this.Project.TaskStarted += this.Project_TaskStarted;

      // Reduce number of threads if there are less items to process
      if (this.MaxThreads > this.itemsToProcessList.Count)
      {
        this.MaxThreads = this.itemsToProcessList.Count;
      }
      
      if (this.MaxThreads > 1)
      {
        List<Thread> buildThreads = new List<Thread>();
        for (int threadIndex = 0; threadIndex < this.MaxThreads; threadIndex++)
        {
          Thread t = new Thread(this.CreateAndExecutChildElements);
          buildThreads.Add(t);
          lock (this.localProperties)
          {
            this.localProperties.Add(t, new Dictionary<string, string>());
          }

          t.Start();
        }

        // Wait until all threads are finished
        foreach (Thread t in buildThreads)
        {
          t.Join();
        }
      }
      else
      {
        lock (this.localProperties)
        {
          this.localProperties.Add(Thread.CurrentThread, new Dictionary<string, string>());
        }

        this.CreateAndExecutChildElements();
      }

      this.Project.TaskStarted -= this.Project_TaskStarted;
      
      if (this.taskExceptions.Count > 0)
      {
        throw this.taskExceptions[0];
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
    private void CreateAndExecutChildElements()
    {
      string currentItem;
      while ((currentItem = this.NextItemToProcess) != null)
      {
        lock (this.localProperties)
        {
          this.localProperties[Thread.CurrentThread].Clear();
          this.localProperties[Thread.CurrentThread].Add(this.parent.Property, currentItem);
        }

        foreach (XmlNode childNode in this.XmlNode)
        {
          // we only care about xmlnodes (elements) that are of the right namespace.
          // ignore any private xml elements (by def. this includes any property with a BuildElementAttribute (name).
          if (childNode.NodeType != XmlNodeType.Element || this.IsPrivateXmlElement(childNode) ||
              (childNode.NamespaceURI.Equals(NamespaceManager.LookupNamespace("nant")) == false))
          {
            continue;
          }

          BuildException loopException = null;

          XmlNode clone = childNode.CloneNode(true);

          this.ReplaceAttributeValues(clone);
          try
          {
            Task task = null;

            // Lock access to type factory as it is not thread safe
            lock (this.typeFactoryLock)
            {
              if (TypeFactory.TaskBuilders.Contains(clone.Name))
              {
                task = this.CreateBuildElement(childNode, clone, this.CreateChildTask);
              }
              else if (TypeFactory.DataTypeBuilders.Contains(clone.Name))
              {
                // we are an datatype declaration
                DataTypeBase dataType = this.CreateBuildElement(childNode, clone, this.CreateChildDataTypeBase);
                if (Project.DataTypeReferences.Contains(dataType.ID) == false)
                {
                  this.Log(Level.Debug, "Adding a {0} reference with id '{1}'.", clone.Name, dataType.ID);
                  Project.DataTypeReferences.Add(dataType.ID, dataType);
                }
                else
                {
                  this.Log(Level.Debug, "Replacing a {0} reference with id '{1}'.", clone.Name, dataType.ID);
                  Project.DataTypeReferences[dataType.ID] = dataType; // overwrite with the new reference.
                }
              }
              else
              {
                throw new BuildException($"Error during processing node {childNode.Name}");
              }
            }

            if (task != null)
            {
              this.ExecuteChildTask(task, childNode);
            }
          }
          catch (Exception ex)
          {
            loopException = ex as BuildException;
            if (loopException == null)
            {
              loopException = new BuildException("Unknown error caught during execution", ex);
            }

            lock (this.taskExceptions)
            { 
              this.taskExceptions.Add(loopException);
            }

            Location lastKnwonLocation;

            // First, try to get the location of last executed task of the current thread
            lock (this.currentTaskAndNode)
            {
              lastKnwonLocation = this.Project.GetLocation(this.currentTaskAndNode[Thread.CurrentThread].XmlNode);
            }

            // If the location of the last executed task of the thrad is unknown, try to get the location of the direct child task
            if (lastKnwonLocation == Location.UnknownLocation)
            {
              lastKnwonLocation = this.Project.GetLocation(childNode);
            }

            if (lastKnwonLocation != Location.UnknownLocation)
            {
              this.Log(Level.Info, $"Last knwon location during error: {lastKnwonLocation}");
            }
          }

          // If StopOnFirstError is set to true, check if an exception has already been caught by any thread.
          // If an exception has been caught, leave the thread.
          if (this.StopOnFirstError && this.ErrorDetected)
          {
            return;
          }

          if (this.StopLoopOnError && (loopException != null))
          {
            break;
          }
        }
      }
    }

    /// <summary>
    /// Executes one child task.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <param name="childNode">The child node.</param>
    private void ExecuteChildTask(Task task, XmlNode childNode)
    {
      lock (this.currentTaskAndNode)
      {
        this.currentTaskAndNode[Thread.CurrentThread] = new TaskNodeInfo(task, childNode);
      }

      task.Execute();
    }

    /// <summary>
    /// Handles the TaskStarted event of the current <see cref="Project"/> instance.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="BuildEventArgs"/> instance containing the event data.</param>
    private void Project_TaskStarted(object sender, BuildEventArgs e)
    {
      TaskNodeInfo taskNodeInfo;
      lock (this.currentTaskAndNode)
      {
        taskNodeInfo = this.currentTaskAndNode[Thread.CurrentThread];
      }

      // Location injection makes only sense if 
      // - the fieldinfo for injection is present
      // - the location of the started task is unknown
      // - the location of the previously executed task is known
      if ((this.fieldInfo != null) && (e.Task.GetLocation() == Location.UnknownLocation) && (taskNodeInfo.Task.GetLocation() != Location.UnknownLocation))
      {
        if (e.Task.Parent == taskNodeInfo.Task)
        {
          // The previously executed task is the parent of the started task, 
          // so it must be the first child task of the previously executed task
          // Search for the node by starting with the first child node
          XmlNode taskNode = taskNodeInfo.XmlNode.FirstChild;

          // Continue searching while there is a child node of the parent task (next sibling of first child) which is known as task
          while ((taskNode != null) && (TypeFactory.TaskBuilders.Contains(taskNode.Name) == false))
          {
            taskNode = taskNode.NextSibling;
          }

          this.SetNewTaskNodeInfo(e.Task, taskNode, taskNodeInfo);
        }
        else
        {
          // The previously executed task isn't the parent of the started task, 
          // so it must be the sibling of the last executed task or of the parents task
          // Start with the last executed XML node
          XmlNode previousTaskNode = taskNodeInfo.XmlNode;
          XmlNode newTaskNode = null;

          while ((newTaskNode == null) && (previousTaskNode != null))
          {
            // Continue searching while there is a next sibling of the last executed (parent) task which is known as task
            newTaskNode = previousTaskNode;
            do
            {
              newTaskNode = newTaskNode.NextSibling;
            }
            while ((newTaskNode != null) && (TypeFactory.TaskBuilders.Contains(newTaskNode.Name) == false));

            // No task found in siblings of (parent) node, move to next parent
            if (newTaskNode == null)
            {
              previousTaskNode = previousTaskNode.ParentNode;
            }
          }

          this.SetNewTaskNodeInfo(e.Task, newTaskNode, taskNodeInfo);
        }
      }
    }

    /// <summary>
    /// Sets the current task and its corresponding XML node.
    /// </summary>
    /// <param name="nextTask">The task which will be executed next.</param>
    /// <param name="xmlNode">The XML node describing the task which will be executed next.</param>
    /// <param name="taskNodeInfo">The <see cref="TaskNodeInfo"/> instance of the last executed task.</param>
    private void SetNewTaskNodeInfo(Task nextTask, XmlNode xmlNode, TaskNodeInfo taskNodeInfo)
    {
      if (xmlNode != null)
      {
        this.SetElementLocation(nextTask, xmlNode);
      }
        
      lock (this.currentTaskAndNode)
      {
        this.currentTaskAndNode[Thread.CurrentThread] = new TaskNodeInfo(nextTask, xmlNode, taskNodeInfo);
      }
    }

    /// <summary>
    /// Creates the build element.
    /// </summary>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    /// <param name="originalNode">The original node.</param>
    /// <param name="clonedNode">The cloned node.</param>
    /// <param name="createAction">The create action.</param>
    /// <returns>The created element or null if an error occurred.</returns>
    private TElement CreateBuildElement<TElement>(XmlNode originalNode, XmlNode clonedNode, CreateElement<TElement> createAction) where TElement : Element
    {
      TElement element = createAction(clonedNode);
      this.SetAdditionalElementData(element, originalNode);
      return element;
    }

    /// <summary>
    /// Sets the additional element data.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="childNode">The original child node.</param>
    private void SetAdditionalElementData(Element element, XmlNode childNode)
    {
      element.Parent = this;

      // Transfer location information, it got lost by creating the task instance using a clone
      // Use reflection because location cannot be overriden normally      
      this.SetElementLocation(element, childNode);
    }

    /// <summary>
    /// Sets the element location.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="elementNode">The element node.</param>
    private void SetElementLocation(Element element, XmlNode elementNode)
    {
      if (this.fieldInfo != null)
      {
        lock (this.fieldInfo)
        {
          this.fieldInfo.SetValue(element, Project.GetLocation(elementNode));
        }
      }
    }

    /// <summary>
    /// Replaces the attribute values.
    /// </summary>
    /// <param name="xmlNode">The cloned XML node.</param>
    private void ReplaceAttributeValues(XmlNode xmlNode)
    {
      XmlNodeList attributes = xmlNode.SelectNodes("//@*");
      if (attributes != null)
      {
        Dictionary<string, string> threadLocalProperties;
        lock (this.localProperties)
        {
          threadLocalProperties = this.localProperties[Thread.CurrentThread];
        }

        foreach (XmlAttribute attribute in attributes)
        {
          foreach (string propertyName in threadLocalProperties.Keys)
          {
            string oldValue = "${" + propertyName + "}";
            attribute.Value = attribute.Value.Replace(oldValue, threadLocalProperties[propertyName]);
          }
        }
      }
    }
  }
}
