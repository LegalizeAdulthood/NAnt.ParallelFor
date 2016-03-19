// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="NAnt.CrossCompile team">
//
// NAnt.ParallelFor, an extension for NAnt for parallel task execution.
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
//   <historyitem date="2016-03-11" change="Created file"/>
//   <historyitem date="2016-03-12" change="Fixed file header"/>
//   <historyitem date="2016-03-13" change="Updated version"/>
//   <historyitem date="2016-03-15" change="Updated version"/>
//   <historyitem date="2016-03-17" change="Updated version"/>
//   <historyitem date="2016-03-18" change="Updated version"/>
//   <historyitem date="2016-03-19" change="Renamed from parallel to parallelfor, updated version"/>
// </history>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("NAnt.ParallelFor")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("NAnt.ParallelFor")]
[assembly: AssemblyCopyright("Copyright ©  2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("fe7fabb4-48d4-4b2c-86f2-5e0d7c549b25")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.2.0.1")]
[assembly: AssemblyFileVersion("0.2.0.1")]

[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: InternalsVisibleTo("NAnt.ParallelFor.Tests")]
