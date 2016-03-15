# NAnt.Parallel
An extension for NAnt for parallel task execution

This plugin allows you to execute a sequence of task for a set of files, directories, file lines or string items. It's similar to NAnt's foreach task (http://nant.sourceforge.net/release/0.92/help/tasks/foreach.html), but allows you the tasks in parallel for each item.

The plugin is still under development. If you find any bugs or request a feature, feel free to create an issue.

## Remarks
* Access to the loop property is only available in the <do> block of the task and will only work in XML attributes.
* Although the order of the input can be specified by the orderby attribute, the execution order can vary due to the execution time required for one item.
* If you want to create data types like properties or file sets in the <do> block, ensure that they get an unique name across all threads.
* If the maxthreads attribute is not set, the plugin will automatically use the number of available processor cores for execution. For debugging or testing the attribute maxthreads can be set to 1 to achieve sequential execution.


## Examples
### Looping over files
```xml
<parallel item="File" property="file">
	<in>
    <items>
      <include name="**/*.c"/>
    </items>
  </in>
	<do maxthreads="8" orderby="Name">      
		<echo message="File: ${file}"/>
		<exec program="cmd.exe">
			<arg value="/c"/>
			<arg value="echo"/>
			<arg value="${file}"/>
		</exec>
	</do>
</parallel>
```

### Looping over directories
```xml
<parallel item="Folder" property="directory">
	<in>
    <items>
      <include name="*"/>
    </items>
  </in>
	<do maxthreads="8" orderby="Name">      
		<echo message="Directory: @{directory}"/>
		<exec program="cmd.exe">
			<arg value="/c"/>
			<arg value="echo"/>
			<arg value="@{directory}"/>
		</exec>
	</do>
</parallel>
```

### Looping over string items
```xml
<parallel item="String" property="stringItem" delim=";" in="Item1;Item2;Item3;Item4">
	<do maxthreads="8" orderby="Name">      
		<echo message="String Item: ${stringItem}"/>
		<exec program="cmd.exe">
			<arg value="/c"/>
			<arg value="echo"/>
			<arg value="${stringItem}"/>
		</exec>
	</do>
</parallel>
```

### Looping over lines of a file
```xml
<parallel item="Line" property="lineContent" in="SampleFile.txt">
	<do maxthreads="8" orderby="Name">      
		<echo message="Line: ${lineContent}"/>
		<exec program="cmd.exe">
			<arg value="/c"/>
			<arg value="echo"/>
			<arg value="${lineContent}"/>
		</exec>
	</do>
</parallel>
```
