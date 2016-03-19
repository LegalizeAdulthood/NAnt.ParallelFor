// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseDataTypeCollection.cs" company="NAnt.CrossCompile team">
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
//   <historyitem date="2016-03-11" change="Created file based on BaseDataTypeCollection.cs of NAnt.Crosscompile 0.7.4.1"/>
//   <historyitem date="2016-03-12" change="Fixed file header"/>
//   <historyitem date="2016-03-19" change="Renamed from parallel to parallelfor"/>
// </history>
// --------------------------------------------------------------------------------------------------------------------
namespace NAnt.ParallelFor.Types
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using Core;

  /// <summary>
  /// A generic collection implementation which contains elements of the type <see cref="DataTypeBase" />.
  /// </summary>
  /// <typeparam name="T">The type of the collection items.</typeparam>
  public abstract class BaseDataTypeCollection<T> : DataTypeBase, ICollection, ICollection<T> where T : Element
  {
    /// <summary>
    /// The internal collection used for this instance.
    /// </summary>
    private readonly List<T> internalCollection = new List<T>();

    /// <summary>
    /// Gets the number of elements contained in the <see cref="T:System.Collections.ICollection" />.
    /// </summary>
    /// <returns>The number of elements contained in the <see cref="T:System.Collections.ICollection" />.</returns>
    public int Count => this.internalCollection.Count;

    /// <summary>
    /// Gets a value indicating whether this instance is read only.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
    /// </value>
    public bool IsReadOnly => false;

    /// <summary>
    /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).    
    /// </summary>    
    /// <returns>true if access to the  <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false. 
    /// In the default implementation of Collection, this property always returns false.</returns>
    public bool IsSynchronized => false;

    /// <summary>
    /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.    
    /// </summary>
    /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
    /// In the default implementation of Collection, this property always returns the current instance.</returns>
    public object SyncRoot => this;

    /// <summary>
    /// Adds a item to the end of the collection.
    /// </summary>
    /// <param name="item">The item to be added to the end of the collection.</param> 
    public void Add(T item)
    {
      this.internalCollection.Add(item);
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of the List.
    /// </summary>
    /// <param name="collection">The collection whose elements should be added to the end of the List. 
    /// The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
    public void AddRange(IEnumerable<T> collection)
    {
      this.internalCollection.AddRange(collection);
    }

    /// <summary>
    /// Determines whether a item is in the collection.
    /// </summary>
    /// <param name="item">The item to locate in the collection.</param> 
    /// <returns>
    /// <see langword="true" /> if <paramref name="item"/> is found in the 
    /// collection; otherwise, <see langword="false" />.
    /// </returns>
    public bool Contains(T item)
    {
      return this.internalCollection.Contains(item);
    }

    /// <summary>
    /// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.        
    /// </summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection. The array must have zero-based indexing.</param> 
    /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      this.internalCollection.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Removes a member from the collection.
    /// </summary>
    /// <param name="item">The item to remove from the collection.</param>
    /// <returns>
    /// true if <paramref name="item" />was successfully removed from the Collection; otherwise, false. 
    /// This method also returns false if item is not found in the original Collection.
    /// </returns>
    public bool Remove(T item)
    {
      return this.internalCollection.Remove(item);
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<T> GetEnumerator()
    {
      return this.internalCollection.GetEnumerator();
    }

    /// <summary>
    /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
    /// </summary>
    public void Clear()
    {
      this.internalCollection.Clear();
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.internalCollection.GetEnumerator();
    }

    /// <summary>
    /// Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, 
    /// starting at a particular <see cref="T:System.Array" /> index.    
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from 
    /// <see cref="T:System.Collections.ICollection" />.The <see cref="T:System.Array" /> must have zero-based indexing.</param>
    /// <param name="index">
    /// The zero-based index in <paramref name="array" /> at which copying begins.</param>
    public void CopyTo(Array array, int index)
    {
      this.CopyTo((T[])array, index);
    }
  }
}
