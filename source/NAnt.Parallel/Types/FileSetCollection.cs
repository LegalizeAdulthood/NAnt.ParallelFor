// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileSetCollection.cs" company="NAnt.CrossCompile team">
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
//   <historyitem date="2016-03-11" change="Created file based on FileSetCollection.cs of NAnt.Crosscompile 0.7.4.1"/>
// </history>
// --------------------------------------------------------------------------------------------------------------------
namespace NAnt.Parallel.Types
{
  using System;
  using System.Collections.Generic;
  using Core.Attributes;
  using Core.Types;

  /// <summary>
  /// This type allows grouping of multiple NAnt <see cref="FileSet"/> elements.
  /// </summary>
  [Serializable]
  [ElementName("filesetcollection")]
  public class FileSetCollection : BaseDataTypeCollection<FileSet>
  {
    /// <summary>
    /// Gets the items.
    /// </summary>
    /// <value>
    /// The items.
    /// </value>
    [BuildElementArray("items")]
    public FileSetCollection Items
    {
      get { return this; }
    }

    /// <summary>
    /// Gets all files using relative paths.
    /// </summary>
    /// <param name="baseUri">The project base URI.</param>
    /// <returns>A list containing all files of the instance relative to the <see cref="baseUri"/> parameter.</returns>
    internal IEnumerable<string> GetAllFilesUsingRelativePaths(Uri baseUri)
    {
      List<string> allFiles = new List<string>();
      foreach (FileSet fileset in this)
      {
        foreach (string item in fileset.FileNames)
        {
          Uri relativeUri = baseUri.MakeRelativeUri(new Uri(item));
          string unescapedUri = Uri.UnescapeDataString(relativeUri.ToString());
          allFiles.Add(unescapedUri);

          // TODO: Logger.LogDebug(DebugMessages.DEBUG_009, unescapedUri, item);
        }
      }

      return allFiles;
    }

    /// <summary>
    /// Gets all directories using relative paths.
    /// </summary>
    /// <param name="baseUri">The project base URI.</param>
    /// <returns>A list containing all directories of the instance relative to the <see cref="baseUri"/> parameter.</returns>
    internal IEnumerable<string> GetAllDirectoriesUsingRelativePaths(Uri baseUri)
    {
      List<string> allFiles = new List<string>();
      foreach (FileSet fileset in this)
      {
        foreach (string item in fileset.DirectoryNames)
        {
          Uri relativeUri = baseUri.MakeRelativeUri(new Uri(item));
          string unescapedUri = Uri.UnescapeDataString(relativeUri.ToString());
          allFiles.Add(unescapedUri);

          // TODO: Logger.LogDebug(DebugMessages.DEBUG_009, unescapedUri, item);
        }
      }

      return allFiles;
    }
  }
}
